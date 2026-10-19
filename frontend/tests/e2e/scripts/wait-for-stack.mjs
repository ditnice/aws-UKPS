// Waits for the Docker Compose stack to be ready before Playwright runs. The backend and
// frontend images are distroless, so Compose healthchecks can't run inside them.
// Backend /health only answers once the database has been reseeded and migrated; the first
// frontend /health call initialises Payload and applies its migrations.

const timeoutMs = Number(process.env.E2E_STACK_TIMEOUT_MS ?? 180_000)
const targets = [
  process.env.E2E_BACKEND_HEALTH_URL ?? 'http://backend:8080/health',
  new URL('/health', process.env.PLAYWRIGHT_BASE_URL ?? 'http://localhost:3000').href,
]
const deadline = Date.now() + timeoutMs

for (const url of targets) {
  let lastError = 'no response'

  while (true) {
    try {
      const response = await fetch(url, { signal: AbortSignal.timeout(5_000) })
      if (response.ok) {
        console.log(`Ready: ${url}`)
        break
      }
      lastError = `HTTP ${response.status}`
    } catch (error) {
      lastError = error instanceof Error ? error.message : String(error)
    }

    if (Date.now() > deadline) {
      console.error(`Timed out after ${timeoutMs}ms waiting for ${url} (${lastError}).`)
      process.exit(1)
    }

    await new Promise((resolve) => setTimeout(resolve, 2_000))
  }
}
