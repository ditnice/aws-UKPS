import Link from 'next/link'
import { notFound } from 'next/navigation'

import { GetUserRegistrationById } from '@/client/generated/sdk.gen'
import { createServerApiClient } from '@/client/server-api'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'

import styles from './page.module.scss'

interface Props {
  params: Promise<{ id: string }>
}

export default async function RequestSumbitted({ params }: Props) {
  const { id } = await params
  const userId = Number(id)

  if (!Number.isInteger(userId)) {
    notFound()
  }

  const apiClient = await createServerApiClient()
  const { data: user, error } = await GetUserRegistrationById({
    client: apiClient,
    path: { id: userId },
  })

  if (error || !user) {
    return (
      <section>
        <PageHeader heading="Unable to load user" />
        <p role="alert">There was a problem retrieving the organisation. Please try again later.</p>
      </section>
    )
  }

  return (
    <>
      <PageHeader heading="Account request submitted" />
      <p>
        Your request to access UK PharmaScan has been sent to your organisation&#39;s champion user
        for review
      </p>
      <hr></hr>
      <h2>What you told us</h2>
      <SummaryList variant="two-column" className={styles.marginBottom}>
        <SummaryListRow label="Organisation" value={user.organisationName} />
        <SummaryListRow label="Full name" value={user.fullName} />
        <SummaryListRow label="Email address" value={user.workEmail} />
        <SummaryListRow label="Contact number" value={user.phoneNumber} />
      </SummaryList>
      <Link href="/portal">
        <Button>Return to UK PharmaScan home</Button>
      </Link>
    </>
  )
}
