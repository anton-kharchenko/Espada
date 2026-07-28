import { afterEach, describe, expect, it, vi } from 'vitest';
import { bffGet } from './bffClient';

afterEach(() => {
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
});
