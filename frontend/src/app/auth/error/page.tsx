import { AuthenticationFailCode } from '@/client/generated'
import { PageHeader } from '@/components/PageHeader/PageHeader'

const MembershipDeactivated = async ({
  searchParams,
}: {
  searchParams: Promise<{ code: string }>
}) => {
  const { code: codeString } = await searchParams
  const code = Object.values(AuthenticationFailCode).find((x) => x == codeString)

  if (code === 'MembershipDeactivated') {
    return (
      <>
        <PageHeader heading="Your account has been deactivated" />
        <p>
          Please contact your organisation&apos;s champion user to request account reactivation.
          {/* TODO URP 548 - Add the organisations champion user */}
        </p>
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
