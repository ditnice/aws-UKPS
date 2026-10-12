import { beforeEach, describe, expect, it, vi } from 'vitest'

import { GET } from './route'

const getPayload = vi.hoisted(() => vi.fn())

vi.mock('payload', () => ({ getPayload }))
vi.mock('@payload-config', () => ({ default: {} }))

describe('GET /health', () => {
  beforeEach(() => {
    getPayload.mockReset()
  })

  it('reports healthy after Payload initializes', async () => {
    getPayload.mockResolvedValue({})

    const response = await GET()

    expect(response.status).toBe(200)
    expect(await response.json()).toEqual({ status: 'ok' })
    expect(response.headers.get('cache-control')).toBe('no-store')
    expect(getPayload).toHaveBeenCalledOnce()
  })

  it('reports unavailable when Payload initialization fails', async () => {
    const error = new Error('Database unavailable')
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    getPayload.mockRejectedValue(error)

    const response = await GET()

    expect(response.status).toBe(503)
    expect(await response.json()).toEqual({ status: 'unavailable' })
    expect(response.headers.get('cache-control')).toBe('no-store')
    expect(consoleError).toHaveBeenCalledWith('Payload initialization failed', error)

    consoleError.mockRestore()
  })
})
