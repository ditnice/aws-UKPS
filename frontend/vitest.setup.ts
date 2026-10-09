import 'dotenv/config'
import { afterEach, beforeEach, vi } from 'vitest'

import { resetNextNavigation } from './src/test-utils/nextNavigation'

beforeEach(() => {
  // Clear implementations and queued one-shot responses as well as call histories.
  // vi.fn(initialImplementation) retains its original default after a reset.
  vi.resetAllMocks()
  resetNextNavigation()
})

afterEach(() => {
  vi.restoreAllMocks()
  vi.unstubAllEnvs()
  vi.unstubAllGlobals()
})
