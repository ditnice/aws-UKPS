/* Every user a spec creates has an email starting with this, so specs that read the user list can
 * tell them apart from seeded users, which other specs never remove. */
export const testEmailPrefix = 'playwright-'

let emailCount = 0

/** An email no earlier run, retry or parallel worker has used, such as
 * `playwright-invite-1761480000000-2-1@example.com`. Playwright sets TEST_WORKER_INDEX in each
 * worker, so this also works at module scope, where test.info() isn't available. */
export function uniqueEmail(purpose: string): string {
  emailCount += 1
  const workerIndex = process.env.TEST_WORKER_INDEX ?? '0'
  return `${testEmailPrefix}${purpose}-${Date.now()}-${workerIndex}-${emailCount}@example.com`
}
