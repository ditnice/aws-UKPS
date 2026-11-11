import { redirect } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { buildMyDetailsHref } from '../navigation'

import { generateOtp } from './generateOtpAction'
import { getOtpCookieData } from './mfaHelpers'
import SetupMfaFormWrapper from './SetupMfaFormWrapper'

type SearchParams = { error?: string; verify?: string }
type PageProps = {
  searchParams: Promise<SearchParams>
}

const errorMessages: Record<string, string> = {
  'generate-failed':
    'We could not generate a one-time passcode. Please try again, and contact support if the problem continues.',
}

const PageWrapper = ({ children }: React.PropsWithChildren) => {
  return (
    <>
      <PageHeader
        backLink={<BackLink href={buildMyDetailsHref()} />}
        heading="Update two-factor authentication"
      />
      {children}
    </>
  )
}

const Page = async ({ searchParams }: PageProps) => {
  const { error, verify } = await searchParams

  if (verify) {
    const setup = await getOtpCookieData()
    // No challenge (expired after 10 mins, or the user landed here directly)
    if (!setup) {
      redirect('?')
    }
    return (
      <PageWrapper>
        <SetupMfaFormWrapper
          redirectOnSuccess={buildMyDetailsHref({ mfaUpdated: 'primary' })}
          setup={setup}
        />
      </PageWrapper>
    )
  }

  return (
    <PageWrapper>
      <p>
        To update your two-factor authentication, we will first generate a one-time passcode (OTP).
        You will be asked to enter this code on the next step to confirm it is you.
      </p>
      {error && (
        <ErrorState>{errorMessages[error] ?? 'Something went wrong. Please try again.'}</ErrorState>
      )}
      <form action={generateOtp}>
        <Button buttonType="submit" type="cta">
          Generate OTP
        </Button>
      </form>
    </PageWrapper>
  )
}

export default Page
