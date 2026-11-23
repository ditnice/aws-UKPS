import { PageHeader } from '@/components/PageHeader/PageHeader'

type NotPermittedProps = {
  /** Whether a champion user could help. Not the case for a super user. */
  contactChampion?: boolean
}

/** Shown on a page the current user's role cannot use. */
export const NotPermitted = ({ contactChampion = true }: NotPermittedProps) => (
  <section>
    <PageHeader heading="User is not permitted to view this page" />
    <p role="alert">
      The current user is not permitted to view this page.
      {contactChampion && ' Please contact your champion user.'}
    </p>
  </section>
)
