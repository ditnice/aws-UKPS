import 'dotenv/config'
import { afterEach, beforeEach, vi } from 'vitest'

import { resetNextNavigation } from './src/test-utils/nextNavigation'

beforeEach(resetNextNavigation)

afterEach(() => {
  vi.clearAllMocks()
  vi.unstubAllEnvs()
  vi.unstubAllGlobals()
})
