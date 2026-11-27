import { getServerEnv } from './env/server'

// Kept apart from instrumentation.ts so that Node.js-only APIs stay out of the Edge bundle.
export async function validateEnvOrExit() {
  try {
    getServerEnv()
  } catch (error) {
    // Next.js can log a rejected hook without stopping the standalone process.
    const message = error instanceof Error ? error.message : 'Invalid frontend environment'
    await new Promise<void>((resolve) => {
      process.stderr.write(`${message}\n`, () => resolve())
    })
    process.exit(1)
  }
}
