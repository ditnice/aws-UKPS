'use client'
import Link from 'next/link'
import { useParams } from 'next/navigation'
import { useSyncExternalStore } from 'react'

import { RegisterUserConfirmationDto } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'

const subscribe = () => () => undefined

export default function RequestSubmitted() {
  const { id } = useParams<{ id: string }>()
  const isClient = useSyncExternalStore(
    subscribe,
    () => true,
    () => false,
  )
  const userJson = isClient ? sessionStorage.getItem(id) : null
  const user = userJson ? (JSON.parse(userJson) as RegisterUserConfirmationDto) : null

  return (
    <>
      <PageHeader heading="Account request submitted" />
      <p>
        Your request to access UK PharmaScan has been sent to your organisation&#39;s champion user
        for review
      </p>
      {user && (
        <>
          <h2>What you told us</h2>
          <SummaryList variant="two-column">
            <SummaryListRow label="Organisation" value={user.organisationName} />
            <SummaryListRow label="Full name" value={user.fullName} />
            <SummaryListRow label="Email address" value={user.workEmail} />
            <SummaryListRow label="Contact number" value={user.phoneNumber} />
          </SummaryList>
        </>
      )}
      <Link href="/portal">
        <Button>Return to UK PharmaScan home</Button>
      </Link>
    </>
  )
}
