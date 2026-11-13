import { notFound } from 'next/navigation'

import { getRecord, RecordType } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

import OrganisationPageWrapper from '../../../_components/OrganisationPageWrapper'
import { buildRecordHref } from '../_lib/recordLinks'

import { StatusChangeActionForm } from './_components/StatusChangeActionForm'

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

type StatusChangeActionFormProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
}

const ChangeRecordStatus = async ({
  organisationId,
  recordId,
  recordType,
}: StatusChangeActionFormProps) => {
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
    (record &&
      (record.organisationId !== organisationId || !record.actions.includes('ChangeStatus')))
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
        heading="Change record status"
        preheading={productDetail?.recordTitle}
      />

      <p>
        If development has ended and will not restart or you no longer own the rights to the{' '}
        {recordType === 'Vaccine' ? 'vaccine' : 'medicine'} then you should archive the record. The
        record cannot be updated once it has been archived.
      </p>

      <p>
        {record.recordStatus === 'OnHold'
          ? 'Removing a record from hold means development has restarted and the information in this record may now need updating.'
          : 'If development has paused and may restart in the future, you should place the record on hold. Update requests for records on hold will be reduced to every 6 months.'}
      </p>

      <StatusChangeActionForm
        organisationId={organisationId}
        recordId={recordId}
        recordType={recordType}
        recordStatus={record.recordStatus}
      />
    </>
  )
}

export default ChangeRecordStatusPage
