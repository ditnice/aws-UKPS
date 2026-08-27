// Any setup scripts you might need go here

// Load .env files
import 'dotenv/config'

process.env.AUTHENTICATION_MODE ??= 'DEV'
if (!process.env.PAYLOAD_SECRET || process.env.PAYLOAD_SECRET.length < 32) {
  process.env.PAYLOAD_SECRET = 'test-payload-secret-at-least-32-characters'
}

// jsdom doesn't implement ResizeObserver
if (typeof globalThis.ResizeObserver === 'undefined') {
  globalThis.ResizeObserver = class ResizeObserver {
    observe() {}
    unobserve() {}
    disconnect() {}
  }
}
