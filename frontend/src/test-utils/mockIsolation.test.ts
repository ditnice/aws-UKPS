import { describe, expect, it, vi } from 'vitest'

const mock = vi.fn(() => 'default')
const originalConsoleError = console.error

describe('global mock isolation', () => {
  it.each([1, 2, 3])('starts test %i with clean defaults and restored spies', () => {
    expect(mock).not.toHaveBeenCalled()
    expect(mock()).toBe('default')
    expect(console.error).toBe(originalConsoleError)

    // Leave both an implementation and a queued response for the next test.
    mock.mockImplementation(() => 'changed').mockReturnValueOnce('queued')
    vi.spyOn(console, 'error').mockImplementation(() => undefined)
  })
})
