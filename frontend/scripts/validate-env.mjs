import nextEnv from '@next/env'

// Run before Next launches: next dev can announce readiness before loading its config.
// Node 24 loads the validator's TypeScript directly; standalone uses its bundled preflight.
nextEnv.loadEnvConfig(process.cwd(), process.argv.includes('--dev'))
await import('../src/env.ts')
