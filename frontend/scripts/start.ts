import '../src/env'

// Validation runs before importing Next's standalone server (which does not load next.config).
// This bundle contains its dependencies, so distroless needs no TS loader or dev packages.
if (!process.argv.includes('--check-env')) {
  const serverUrl = new URL('./server.js', import.meta.url)
  await import(serverUrl.href)
}
