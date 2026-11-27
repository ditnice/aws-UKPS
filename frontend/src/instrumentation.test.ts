import { beforeEach, describe, expect, it, vi } from 'vitest'

import { register } from './instrumentation'

const getServerEnv = vi.hoisted(() => vi.fn())
vi.mock('./env/server', () => ({ getServerEnv }))

describe('register', () => {
  const exit = vi.fn()
  const write = vi.fn()

  beforeEach(() => {
    vi.stubEnv('NEXT_RUNTIME', 'nodejs')
    vi.spyOn(process, 'exit').mockImplementation(exit as never)
    write.mockImplementation((_message: string, done: () => void) => {
      done()
      return true
    })
    vi.spyOn(process.stderr, 'write').mockImplementation(write)
  })

  it('continues when the environment is valid', async () => {
    await register()

    expect(getServerEnv).toHaveBeenCalledOnce()
    expect(exit).not.toHaveBeenCalled()
  })

  it.each([
    [new Error('Invalid frontend environment:\nPAYLOAD_SECRET: Must not be blank')],
    ['not an error'],
  ])('reports an invalid environment and exits (%s)', async (thrown) => {
    getServerEnv.mockImplementation(() => {
      throw thrown
    })

    await register()

    const message = thrown instanceof Error ? thrown.message : 'Invalid frontend environment'
    expect(write).toHaveBeenCalledWith(`${message}\n`, expect.any(Function))
    expect(exit).toHaveBeenCalledWith(1)
  })

  it('does not validate outside the Node.js runtime', async () => {
    vi.stubEnv('NEXT_RUNTIME', 'edge')

    await register()

    expect(getServerEnv).not.toHaveBeenCalled()
  })
})
