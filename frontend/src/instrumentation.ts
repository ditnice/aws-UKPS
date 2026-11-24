export async function register() {
  if (process.env.NEXT_RUNTIME === 'nodejs') {
    const { validateServerEnv } = await import('./env/server')
    try {
      validateServerEnv()
    } catch (error) {
      // Next.js can log a rejected hook without stopping the standalone process.
      const message = error instanceof Error ? error.message : 'Invalid frontend environment'
      await new Promise<void>((resolve) => {
        process.stderr.write(`${message}\n`, () => resolve())
      })
      process.exit(1)
    }
  }
}
