import { notFound } from 'next/navigation'

import { getRecord } from '@/client/generated'
import type { RecordType } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { canMakeStatusChange, type RecordStatusChangeAction } from '../_lib/statusChangeActions'

import type { ReactNode } from 'react'

type StatusChangePageProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
  action: RecordStatusChangeAction
  heading: string
  backHref: string
  /** Shown above the page heading. */
  banner?: ReactNode
  children: ReactNode
}

/**
 * A step of one status change, headed with the record title. Not found unless the user can make
 * that change to the record.
 */
export const StatusChangePage = async ({
  organisationId,
  recordId,
  recordType,
  action,
  heading,
  backHref,
  banner,
  children,
}: StatusChangePageProps) => {
  const {
    data: record,
    error,
    response,
  } = await getRecord({
    client: await createServerApiClient(),
    path: { id: recordId },
    query: { recordType, organisationId },
  })

  if (
    response?.status === 404 ||
    (record && !canMakeStatusChange(record, organisationId, action))
  ) {
    notFound()
  }

  const backLink = <BackLink href={backHref}>Back</BackLink>

  if (error || !record) {
    return (
      <section>
        <PageHeader heading="Unable to load record" backLink={backLink} />
        <ErrorState>There was a problem retrieving the record. Please try again later.</ErrorState>
      </section>
    )
  }

  //TODO: needs updating when we add vaccine DTOs
  const productDetail = record.recordType === 'Medicine' ? record.recordProductDetail : undefined

  return (
    <>
      {banner}

      <PageHeader backLink={backLink} heading={heading} preheading={productDetail?.recordTitle} />

      {children}
    </>
  )
}
