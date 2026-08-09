import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { createClientConfig } from './hey-api'

const protectedUrl = '/backend-api/widgets'

function configuredFetch() {
  return createClientConfig({}).fetch as typeof fetch
}

beforeEach(() => {
  document.cookie = 'csrf_token=test%20token; path=/'
})

afterEach(() => {
  document.cookie = 'csrf_token=; Max-Age=0; path=/'
  vi.unstubAllGlobals()
  vi.restoreAllMocks()
})

describe('createClientConfig', () => {
  it('uses the environment base URL and native fetch on the server without a fallback', () => {
    const nativeFetch = globalThis.fetch
    const previousBaseUrl = process.env.BACKEND_API_BASE_URL
    vi.stubGlobal('window', undefined)

    process.env.BACKEND_API_BASE_URL = 'https://api.example.test'
    expect(createClientConfig({})).toMatchObject({
      baseUrl: 'https://api.example.test',
      fetch: nativeFetch,
    })

    delete process.env.BACKEND_API_BASE_URL
    expect(createClientConfig({}).baseUrl).toBeUndefined()

    if (previousBaseUrl === undefined) {
      delete process.env.BACKEND_API_BASE_URL
    } else {
      process.env.BACKEND_API_BASE_URL = previousBaseUrl
    }
  })

  it('uses the backend proxy and refreshes a protected 401 before one retry', async () => {
    const original401 = new Response(null, { status: 401 })
    const retried200 = new Response('{}', { status: 200 })
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValueOnce(original401)
      .mockResolvedValueOnce(new Response(null, { status: 204 }))
      .mockResolvedValueOnce(retried200)
    vi.stubGlobal('fetch', fetchMock)

    const config = createClientConfig({})
    const response = await (config.fetch as typeof fetch)(protectedUrl, {
      method: 'POST',
      body: JSON.stringify({ name: 'replayable' }),
      headers: { 'Content-Type': 'application/json' },
    })

    expect(config.baseUrl).toBe('/backend-api')
    expect(response).toBe(retried200)
    expect(fetchMock).toHaveBeenNthCalledWith(
      2,
      '/backend-api/auth/refresh',
      expect.objectContaining({
        method: 'POST',
        credentials: 'include',
        headers: { 'X-CSRF-Token': 'test token' },
      }),
    )
    expect(fetchMock.mock.calls[2]).toEqual(fetchMock.mock.calls[0])
  })

  it.each([403, 401])('does not refresh a %s response from an excluded request', async (status) => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(new Response(null, { status }))
    vi.stubGlobal('fetch', fetchMock)
    const url = status === 401 ? '/backend-api/auth/login' : protectedUrl

    await configuredFetch()(url)

    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('returns the original 401 when refresh fails', async () => {
    const original401 = new Response(null, { status: 401 })
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValueOnce(original401)
      .mockRejectedValueOnce(new Error('refresh unavailable'))
    vi.stubGlobal('fetch', fetchMock)

    const response = await configuredFetch()(protectedUrl)

    expect(response).toBe(original401)
    expect(fetchMock).toHaveBeenCalledTimes(2)
  })

  it('retries at most once when the protected request remains unauthorized', async () => {
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(null, { status: 204 }))
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
    vi.stubGlobal('fetch', fetchMock)

    const response = await configuredFetch()(protectedUrl)

    expect(response.status).toBe(401)
    expect(fetchMock).toHaveBeenCalledTimes(3)
  })

  it('clones Request input for replay', async () => {
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValueOnce(new Response(null, { status: 401 }))
      .mockResolvedValueOnce(new Response(null, { status: 204 }))
      .mockResolvedValueOnce(new Response('{}', { status: 200 }))
    vi.stubGlobal('fetch', fetchMock)
    const request = new Request('https://example.test/backend-api/widgets', {
      method: 'POST',
      body: JSON.stringify({ id: 1 }),
    })

    await configuredFetch()(request)

    const replay = fetchMock.mock.calls[2][0]
    expect(replay).toBeInstanceOf(Request)
    expect(replay).not.toBe(request)
    expect(await (replay as Request).text()).toBe('{"id":1}')
  })

  it('does not refresh a request with a non-replayable stream body', async () => {
    const fetchMock = vi.fn<typeof fetch>().mockResolvedValue(new Response(null, { status: 401 }))
    vi.stubGlobal('fetch', fetchMock)

    await configuredFetch()(protectedUrl, { method: 'POST', body: new ReadableStream() })

    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('shares one refresh across concurrent protected requests', async () => {
    let resolveRefresh: (response: Response) => void = () => undefined
    const refreshResponse = new Promise<Response>((resolve) => {
      resolveRefresh = resolve
    })
    let protectedCalls = 0
    const fetchMock = vi.fn<typeof fetch>(async (input) => {
      if (input === '/backend-api/auth/refresh') {
        return refreshResponse
      }

      protectedCalls += 1
      return new Response(null, { status: protectedCalls <= 2 ? 401 : 200 })
    })
    vi.stubGlobal('fetch', fetchMock)
    const clientFetch = configuredFetch()

    const first = clientFetch('/backend-api/one')
    const second = clientFetch('/backend-api/two')
    await vi.waitFor(() => {
      expect(
        fetchMock.mock.calls.filter(([input]) => input === '/backend-api/auth/refresh'),
      ).toHaveLength(1)
    })
    resolveRefresh(new Response(null, { status: 204 }))

    expect((await first).status).toBe(200)
    expect((await second).status).toBe(200)
    expect(fetchMock).toHaveBeenCalledTimes(5)
  })

  it('does not refresh again when a delayed 401 arrives after refresh completed', async () => {
    let resolveDelayedRequest: (response: Response) => void = () => undefined
    const delayedResponse = new Promise<Response>((resolve) => {
      resolveDelayedRequest = resolve
    })
    let protectedCalls = 0
    const fetchMock = vi.fn<typeof fetch>(async (input) => {
      if (input === '/backend-api/auth/refresh') {
        return new Response(null, { status: 204 })
      }

      protectedCalls += 1
      if (protectedCalls === 2) return delayedResponse
      return new Response(null, { status: protectedCalls === 1 ? 401 : 200 })
    })
    vi.stubGlobal('fetch', fetchMock)
    const clientFetch = configuredFetch()

    const first = clientFetch('/backend-api/one')
    const delayed = clientFetch('/backend-api/two')
    expect((await first).status).toBe(200)
    resolveDelayedRequest(new Response(null, { status: 401 }))

    expect((await delayed).status).toBe(200)
    expect(
      fetchMock.mock.calls.filter(([input]) => input === '/backend-api/auth/refresh'),
    ).toHaveLength(1)
  })
})
