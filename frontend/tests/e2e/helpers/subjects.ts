import { isDevAuthMode } from './test-environment'

/* Fixed users the backend seeds in the dev Champion's organisation for tests running as a Champion
 * to act on, so they never pick a user that a test running as another role is changing. They must
 * match E2eSubjectUsers in the backend. They only exist with dev authentication, where the
 * backend seeds its own data. */
export const championSubjects = {
  /** Active standard user that the promote/demote flow changes and then restores. */
  promote: 'e2e.subject.promote@email.com',
  /** Active standard user that the deactivate/reactivate flow changes and then restores. */
  deactivate: 'e2e.subject.deactivate@email.com',
  /** Active standard user that only tests which cancel open, so it never changes. */
  readonly: 'e2e.subject.readonly@email.com',
  /** Active champion user, never changed. */
  champion: 'e2e.subject.champion@email.com',
  /** Deactivated standard user, never changed. */
  deactivated: 'e2e.subject.deactivated@email.com',
} as const

export type ChampionSubject = keyof typeof championSubjects

export function hasSeededSubjects(): boolean {
  return isDevAuthMode()
}
