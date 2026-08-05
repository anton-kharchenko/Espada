export type BffErrorKind = 'unauthorized' | 'forbidden' | 'rate-limited' | 'offline' | 'unavailable';

export class BffError extends Error {
  constructor(
    public readonly kind: BffErrorKind,
    message: string,
    public readonly status?: number,
  ) {
    super(message);
    this.name = 'BffError';
  }
}

const bffBaseUrl = 'https://espada.invalid';
const antiforgeryCookieNames = ['__Host-Espada.Console.Csrf', 'Espada.Console.Csrf'] as const;

const normalizeBffPath = (path: string): string => {
  if (!path.startsWith('/bff/') || path.startsWith('//') || path.includes('\\') || path.includes('#')) {
    throw new TypeError('BFF path must be a normalized /bff/... relative path.');
  }

  const url = new URL(path, bffBaseUrl);
  if (url.origin !== bffBaseUrl || `${url.pathname}${url.search}` !== path || !url.pathname.startsWith('/bff/')) {
    throw new TypeError('BFF path must be a normalized /bff/... relative path.');
  }

  return path;
};

const getErrorKind = (status: number): BffErrorKind => {
  if (status === 401) return 'unauthorized';
  if (status === 403) return 'forbidden';
  if (status === 429) return 'rate-limited';
  return 'unavailable';
};

const getAntiforgeryToken = (): string => {
  const cookies = document.cookie.split(';');

  for (const cookieName of antiforgeryCookieNames) {
    const prefix = `${cookieName}=`;
    const cookie = cookies.map((value) => value.trim()).find((value) => value.startsWith(prefix));
    if (cookie) return decodeURIComponent(cookie.slice(prefix.length));
  }

  throw new BffError('unavailable', 'Refresh the console session before making changes.');
};

const getErrorMessage = async (response: Response): Promise<string> => {
  try {
    const value: unknown = await response.json();
    if (typeof value === 'object' && value !== null && 'message' in value && typeof value.message === 'string') {
      return value.message;
    }
  } catch {
    // The status remains the canonical error when the response has no JSON body.
  }

  return `The Espada BFF returned ${response.status}.`;
};

interface BffRequestOptions {
  method?: 'GET' | 'POST' | 'DELETE';
  body?: unknown;
  idempotencyKey?: string;
  signal?: AbortSignal;
}

const bffRequest = async <T>(
  path: string,
  parse: (value: unknown) => T,
  options: BffRequestOptions = {},
): Promise<T> => {
  const normalizedPath = normalizeBffPath(path);
  const method = options.method ?? 'GET';
  const headers = new Headers({ Accept: 'application/json' });

  if (method !== 'GET') {
    headers.set('X-CSRF-TOKEN', getAntiforgeryToken());
  }
  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json');
  }
  if (options.idempotencyKey) {
    headers.set('Idempotency-Key', options.idempotencyKey);
  }

  let response: Response;

  try {
    response = await fetch(normalizedPath, {
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
      credentials: 'same-origin',
      headers,
      method,
      signal: options.signal,
    });
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw error;
    throw new BffError('offline', 'The Espada BFF is not reachable.');
  }

  if (!response.ok) {
    throw new BffError(getErrorKind(response.status), await getErrorMessage(response), response.status);
  }

  if (response.status === 204) return parse(undefined);
  return parse(await response.json());
};

export const bffGet = <T>(path: string, parse: (value: unknown) => T, signal?: AbortSignal): Promise<T> =>
  bffRequest(path, parse, { signal });

export const bffPost = <T>(
  path: string,
  body: unknown,
  parse: (value: unknown) => T,
  signal?: AbortSignal,
  idempotencyKey?: string,
): Promise<T> =>
  bffRequest(path, parse, {
    method: 'POST',
    body,
    idempotencyKey,
    signal,
  });

export const bffDelete = (path: string, signal?: AbortSignal): Promise<void> =>
  bffRequest(path, () => undefined, { method: 'DELETE', signal });

export const bffPostCommand = (path: string, signal?: AbortSignal): Promise<void> =>
  bffRequest(path, () => undefined, { method: 'POST', signal });
