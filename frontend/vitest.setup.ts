// Any setup scripts you might need go here

// jest-dom/vitest registers the jest-dom matchers (toBeVisible, toHaveValue, ...) and their
// types (see ADR-005); dotenv/config loads .env files.
import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import 'dotenv/config'
import { afterEach, vi } from 'vitest'

// `globals` is off, so RTL can't register its own cleanup. Run it once here instead of in every spec.
afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

// jsdom doesn't implement ResizeObserver
if (typeof globalThis.ResizeObserver === 'undefined') {
  globalThis.ResizeObserver = class ResizeObserver {
    observe() {}
    unobserve() {}
    disconnect() {}
  }
}
