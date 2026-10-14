import { AuthenticationFailCode, getCurrentOrganisationChampions } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'

export const dynamic = 'force-dynamic'

const MembershipDeactivated = async ({
  searchParams,
}: {
  searchParams: Promise<{ code: string }>
}) => {
  const { code: codeString } = await searchParams
  const code = Object.values(AuthenticationFailCode).find((x) => x == codeString)

  if (code === 'MembershipDeactivated') {
    const { data: championEmails, error } = await getCurrentOrganisationChampions({
      client: await createServerApiClient(),
    })

    if (!championEmails || error) {
      return (
        <>
          <PageHeader heading="Your account has been deactivated" />
          <ErrorState data-testid="champion-retrieval-error">
            An error occurred when retrieving your organisation&apos;s champion users. Please try
            again later.
          </ErrorState>
        </>
      )
    }

    if (championEmails.length === 1) {
      return (
        <>
          <PageHeader heading="Your account has been deactivated" />
          <p data-testid="single-champion">
            Contact your organisation&apos;s champion user to reactivate your account:{' '}
            <a href={`mailto:${championEmails[0]}`}>{championEmails[0]}</a>
          </p>
        </>
      )
    }

    return (
      <>
        <PageHeader heading="Your account has been deactivated" />
        <div data-testid="multiple-champions">
          <p>
            Contact one of your organisation&apos;s champion users to reactivate your account. Their
            email addresses are:
          </p>
          <ul>
            {championEmails.map((email) => (
              <li key={email}>
                <a href={`mailto:${email}`}>{email}</a>
              </li>
            ))}
          </ul>
        </div>
      </>
    )
  }
  if (code === 'OrganisationNotActive') {
    return (
      <>
        <PageHeader heading="Your organisation is not active" />
        <p>Please contact your organisation&apos;s champion user for more information.</p>
      </>
    )
  }
  return (
    <>
      <PageHeader heading="An unexpected authentication error has occurred." />
    </>
  )
}

export default MembershipDeactivated
