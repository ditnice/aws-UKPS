import { notFound } from 'next/navigation'

import { getRecord, RecordType } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

import OrganisationPageWrapper from '../../../_components/OrganisationPageWrapper'
import { canChangeRecordStatus, buildRecordHref } from '../_lib/recordActions'

import { ChangeRecordStatusActions } from './_components/ChangeRecordStatusActions'

type ChangeRecordStatusPageProps = {
  params: Promise<{ id: string; recordId: string }>
  searchParams: Promise<{ recordType?: string | string[] }>
}

const ChangeRecordStatusPage = async ({ params, searchParams }: ChangeRecordStatusPageProps) => {
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
        <ChangeRecordStatus
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
        />
      )}
    </OrganisationPageWrapper>
  )
}

type ChangeRecordStatusProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
}

const ChangeRecordStatus = async ({
  organisationId,
  recordId,
  recordType,
}: ChangeRecordStatusProps) => {
  const {
    data: record,
    error,
    response,
  } = await getRecord({
    client: await createServerApiClient(),
    path: { id: recordId },
    query: { recordType },
  })

  if (
    response?.status === 404 ||
    (record && (record.organisationId !== organisationId || !canChangeRecordStatus(record)))
  ) {
    notFound()
  }

  const backLink = (
    <BackLink href={buildRecordHref(organisationId, recordId, recordType)}>Back</BackLink>
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
        heading="What would you like to do with this record?"
        preheading={productDetail?.recordTitle}
      />

      <p>
        If development has permanently discontinued or you no longer own the rights to the medicine
        then you should archive the record.
      </p>

      <p>
        {record.recordStatus === 'OnHold'
          ? 'If development has restarted, you should remove the record hold.'
          : 'If development has paused, you should place the record on hold.'}
      </p>

      <ChangeRecordStatusActions
        organisationId={organisationId}
        recordId={recordId}
        recordType={recordType}
        recordStatus={record.recordStatus}
      />
    </>
  )
}

export default ChangeRecordStatusPage
