import { PageHeader } from '@/components/PageHeader/PageHeader'

/** Shown to a standard user on a page only champion and super users can use. */
export const NotPermitted = () => (
  <section>
    <PageHeader heading="User is not permitted to view this page" />
    <p role="alert">
      The current user is not permitted to view this page. Please contact your champion user.
    </p>
  </section>
)
