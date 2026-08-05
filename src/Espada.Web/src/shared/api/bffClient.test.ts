import { afterEach, describe, expect, it, vi } from 'vitest';
import { bffGet, bffPost } from './bffClient';

afterEach(() => {
  document.cookie = 'Espada.Console.Csrf=; Max-Age=0; path=/';
  vi.unstubAllGlobals();
});

describe('bffGet', () => {
  it('allows normalized same-origin BFF paths', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(JSON.stringify({ value: 'ok' }), { status: 200 }));
    vi.stubGlobal('fetch', fetchMock);

    await expect(bffGet('/bff/session?mode=local', (value) => value)).resolves.toEqual({ value: 'ok' });
    expect(fetchMock).toHaveBeenCalledWith('/bff/session?mode=local', expect.any(Object));
  });

  it.each([
    'https://example.com/bff/session',
    '//example.com/bff/session',
    '/api/session',
    'bff/session',
    '/bff/../api/session',
    '/bff\\session',
    '/bff/session#fragment',
    '/bff/session with space',
  ])('rejects paths outside the normalized BFF boundary: %s', async (path) => {
    const fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);

    await expect(bffGet(path, (value) => value)).rejects.toThrow(
      'BFF path must be a normalized /bff/... relative path.',
    );
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it('sends mutations through the same-origin antiforgery boundary', async () => {
    document.cookie = 'Espada.Console.Csrf=request-token; path=/';
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ created: true }), {
        status: 201,
        headers: { 'Content-Type': 'application/json' },
      }),
    );
    vi.stubGlobal('fetch', fetchMock);

    await expect(bffPost('/bff/workspaces', { name: 'Espada' }, (value) => value)).resolves.toEqual({ created: true });

    const [, options] = fetchMock.mock.calls[0] as [string, RequestInit];
    const headers = options.headers as Headers;
    expect(options.credentials).toBe('same-origin');
    expect(options.method).toBe('POST');
    expect(options.body).toBe(JSON.stringify({ name: 'Espada' }));
    expect(headers.get('X-CSRF-TOKEN')).toBe('request-token');
    expect(headers.get('Content-Type')).toBe('application/json');
  });

  it('does not send a mutation without the readable antiforgery token', async () => {
    const fetchMock = vi.fn();
    vi.stubGlobal('fetch', fetchMock);

    await expect(bffPost('/bff/workspaces', { name: 'Espada' }, (value) => value)).rejects.toThrow(
      'Refresh the console session',
    );
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
