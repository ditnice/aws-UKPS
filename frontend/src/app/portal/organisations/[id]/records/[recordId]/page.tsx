import { notFound } from 'next/navigation'

import { getRecord, RecordType } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { Tag } from '@/components/Tag/Tag'

import OrganisationPageWrapper from '../../_components/OrganisationPageWrapper'
import { recordStatusLabels, recordStatusTagColours } from '../labels'

import { ManageRecordActionBanner } from './_components/ManageRecordActionBanner'
import { MedicineRecordDetails } from './_components/MedicineRecordDetails'

const parseRecordId = (value: string): number | null =>
  /^[1-9]\d*$/.test(value) && Number.isSafeInteger(Number(value)) ? Number(value) : null

const parseRecordType = (value: string | string[] | undefined): RecordType | null =>
  value === 'Medicine' || value === 'Vaccine' ? value : null

type RecordPageProps = {
  params: Promise<{ id: string; recordId: string }>
  searchParams: Promise<{ recordType?: string | string[] }>
}

const RecordPage = async ({ params, searchParams }: RecordPageProps) => {
  const { id: organisationId, recordId: recordIdParam } = await params
  const recordId = parseRecordId(recordIdParam)
  const recordType = parseRecordType((await searchParams).recordType)

  if (!recordId || !recordType) {
    notFound()
  }

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <RecordView organisationId={organisation.id} recordId={recordId} recordType={recordType} />
      )}
    </OrganisationPageWrapper>
  )
}

type RecordViewProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
}

const RecordView = async ({ organisationId, recordId, recordType }: RecordViewProps) => {
  const {
    data: record,
    error,
    response,
  } = await getRecord({
    client: await createServerApiClient(),
    path: { id: recordId },
    query: { recordType },
  })

  if (response?.status === 404 || (record && record.organisationId !== organisationId)) {
    notFound()
  }

  const backLink = (
    <BackLink href={`/portal/organisations/${organisationId}/records`}>Back</BackLink>
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
        heading={`Record ${record.recordId}${productDetail ? `: ${productDetail.companyCode}` : ''}`}
        lead={
          <>
            <strong>Record title:</strong> {productDetail?.recordTitle}
          </>
        }
        metadata={[
          <Tag key="status" colour={recordStatusTagColours[record.displayStatus]}>
            {recordStatusLabels[record.displayStatus]}
          </Tag>,
        ]}
      />

      <ManageRecordActionBanner />

      <h2>View record details</h2>
      <Button variant="secondary">Export record to PDF</Button>
      {record.recordType === 'Medicine' ? (
        <MedicineRecordDetails record={record} />
      ) : (
        <p>Viewing vaccine record details is not yet available.</p>
      )}
    </>
  )
}

export default RecordPage
