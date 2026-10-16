import { notFound } from 'next/navigation'

import { getRecord, RecordDto, RecordType } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { Tag } from '@/components/Tag/Tag'
import { parsePositiveInteger } from '@/lib/valueParsing'

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
  const { id, recordId: recordIdParam } = await params
  const organisationId = parsePositiveInteger(id)
  const recordId = parseRecordId(recordIdParam)
  const recordType = parseRecordType((await searchParams).recordType)

  if (!organisationId || !recordId || !recordType) {
    notFound()
  }

  // Fetch the record before the organisation, so the record endpoint can check whether the record
  // exists in this organisation, whether the user may see it, and whether this is their selected
  // organisation. Otherwise the organisation wrapper below would offer to switch organisation
  // first, even for a record that doesn't exist.
  const { data: record, response } = await getRecord({
    client: await createServerApiClient(),
    path: { id: recordId },
    query: { recordType, organisationId },
  })

  if (response?.status === 404) {
    notFound()
  }

  return (
    <OrganisationPageWrapper organisationId={id}>
      {(organisation) => <RecordView organisationId={organisation.id} record={record} />}
    </OrganisationPageWrapper>
  )
}

type RecordViewProps = {
  organisationId: number
  record: RecordDto | undefined
}

const RecordView = ({ organisationId, record }: RecordViewProps) => {
  const backLink = (
    <BackLink href={`/portal/organisations/${organisationId}/records`}>Back</BackLink>
  )

  if (!record) {
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
