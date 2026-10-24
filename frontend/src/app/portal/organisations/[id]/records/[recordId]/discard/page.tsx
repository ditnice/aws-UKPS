import { notFound } from 'next/navigation'

import { getRecord, RecordType } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { parsePositiveInteger } from '@/lib/valueParsing'

import OrganisationPageWrapper from '../../../_components/OrganisationPageWrapper'

import { DiscardRecordControls } from './_components/DiscardRecordControls'

type DiscardRecordPageProps = {
  params: Promise<{ id: string; recordId: string }>
  searchParams: Promise<{ recordType?: string | string[] }>
}

const DiscardRecordPage = async ({ params, searchParams }: DiscardRecordPageProps) => {
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
        <DiscardRecord
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
        />
      )}
    </OrganisationPageWrapper>
  )
}

type DiscardRecordProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
}

const DiscardRecord = async ({ organisationId, recordId, recordType }: DiscardRecordProps) => {
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
    (record && (record.organisationId !== organisationId || !record.canDiscard))
  ) {
    notFound()
  }

  const backLink = (
    <BackLink
      href={`/portal/organisations/${organisationId}/records/${recordId}?recordType=${recordType}`}
    >
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
      <Alert type="caution">
        <h2>If you confirm, this record will be discarded</h2>
        <p>
          The record will be discarded and no longer available to submit to QA for review. You
          cannot undo this and this record cannot be recovered in the future.
        </p>
      </Alert>

      <PageHeader
        backLink={backLink}
        heading="Confirm decision to discard record"
        lead={
          <>
            <strong>Record title:</strong> {productDetail?.recordTitle}
          </>
        }
      />

      <p>What happens when this record is discarded:</p>
      <ul>
        <li>
          the product will no longer be accessible to view by you or other users in your
          organisation.
        </li>
        <li>
          the company code, generic and brand names will remain in any other records that use them.
        </li>
      </ul>

      <DiscardRecordControls organisationId={organisationId} recordId={recordId} />
    </>
  )
}

export default DiscardRecordPage
