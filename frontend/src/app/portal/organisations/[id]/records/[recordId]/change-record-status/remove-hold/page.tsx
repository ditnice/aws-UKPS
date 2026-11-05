import { notFound } from 'next/navigation'

import { getRecord, RecordType } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

import OrganisationPageWrapper from '../../../../_components/OrganisationPageWrapper'
import { buildRecordHref } from '../../_lib/recordLinks'

import { RemoveHoldReasons } from './_components/RemoveHoldReasons'

type RemoveHoldPageProps = {
  params: Promise<{ id: string; recordId: string }>
  searchParams: Promise<{ recordType?: string | string[] }>
}

const RemoveHoldPage = async ({ params, searchParams }: RemoveHoldPageProps) => {
  const { id: organisationId, recordId: recordIdParam } = await params
  const { recordType: recordTypeParam } = await searchParams
  const recordId = parsePositiveInteger(recordIdParam)
  const recordType = Object.values(RecordType).find((type) => type === recordTypeParam)

  if (!recordId || !recordType) {
    notFound()
  }

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <RemoveHold organisationId={organisation.id} recordId={recordId} recordType={recordType} />
      )}
    </OrganisationPageWrapper>
  )
}

type RemoveHoldProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
}

const RemoveHold = async ({ organisationId, recordId, recordType }: RemoveHoldProps) => {
  const {
    data: record,
    error,
    response,
  } = await getRecord({
    client: await createServerApiClient(),
    path: { id: recordId },
    query: { recordType, organisationId },
  })

  // Only an on-hold record whose status the user can change can have its hold removed.
  if (
    response?.status === 404 ||
    (record &&
      (record.organisationId !== organisationId ||
        !record.actions.includes('ChangeStatus') ||
        record.recordStatus !== 'OnHold'))
  ) {
    notFound()
  }

  const backLink = (
    <BackLink href={buildRecordHref(organisationId, recordId, recordType, 'change-record-status')}>
      Back
    </BackLink>
  )

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
      <PageHeader
        backLink={backLink}
        heading="Change record status"
        preheading={productDetail?.recordTitle}
      />

      <p>
        Removing a record from hold means development has restarted and the information in this
        record may now need updating.
      </p>

      <RemoveHoldReasons
        organisationId={organisationId}
        recordId={recordId}
        recordType={recordType}
      />
    </>
  )
}

export default RemoveHoldPage
