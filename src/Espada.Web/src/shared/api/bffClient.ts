export type BffErrorKind = 'unauthorized' | 'forbidden' | 'rate-limited' | 'offline' | 'unavailable';

export class BffError extends Error {
  constructor(
    public readonly kind: BffErrorKind,
    message: string,
  ) {
    super(message);
    this.name = 'BffError';
  }
}

const bffBaseUrl = 'https://espada.invalid';

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

export const bffGet = async <T>(path: string, parse: (value: unknown) => T, signal?: AbortSignal): Promise<T> => {
  const normalizedPath = normalizeBffPath(path);
  let response: Response;

  try {
    response = await fetch(normalizedPath, {
      credentials: 'same-origin',
      headers: { Accept: 'application/json' },
      signal,
    });
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw error;
    throw new BffError('offline', 'The Espada BFF is not reachable.');
  }

  if (!response.ok) {
    throw new BffError(getErrorKind(response.status), `The Espada BFF returned ${response.status}.`);
  }

  return parse(await response.json());
};
