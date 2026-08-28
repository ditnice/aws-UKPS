export const authStatePath = 'tests/e2e/.auth/authenticated-dev.json'

export function requireEnvironmentVariable(name: string): string {
  const value = process.env[name]?.trim()

  if (!value) {
    throw new Error(`${name} must be set to run authenticated Playwright tests.`)
  }

  return value
}
