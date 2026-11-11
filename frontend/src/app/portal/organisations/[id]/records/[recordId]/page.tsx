import { notFound } from 'next/navigation'

import { getOrganisationRecords, getRecord, RecordType } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { Alert } from '@/components/Alert/Alert'
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
import {
  parseRecordViewAlert,
  RecordViewAlert,
  type RecordViewAlertAction,
} from './_components/RecordViewAlert'

type RecordPageProps = {
  params: Promise<{ id: string; recordId: string }>
  searchParams: Promise<{ recordType?: string | string[]; action?: string | string[] }>
}

const isRecordOverdue = (nextUpdateDue?: string | null) => {
  if (!nextUpdateDue) return false

  const dueDate = new Date(nextUpdateDue)

  if (Number.isNaN(dueDate.getTime())) return false

  return dueDate.getTime() < Date.now()
}

const OverdueRecordAlert = () => (
  <Alert type="info">
    <h3>Record update overdue</h3>
    <p>
      This record needs to be reviewed for any updates. You can edit the record to update published
      record information or confirm the information is up-to-date.
    </p>
  </Alert>
)

const RecordPage = async ({ params, searchParams }: RecordPageProps) => {
  const { id, recordId: recordIdParam } = await params
  const { recordType: recordTypeParam, action } = await searchParams
  const organisationId = parsePositiveInteger(id)
  const recordId = parsePositiveInteger(recordIdParam)
  const recordType = Object.values(RecordType).find((type) => type === recordTypeParam)

  if (!organisationId || !recordId || !recordType) {
    notFound()
  }

  return (
    <OrganisationPageWrapper organisationId={id}>
      {(organisation) => (
        <RecordView
          organisationId={organisation.id}
          recordId={recordId}
          recordType={recordType}
          alert={parseRecordViewAlert(action)}
        />
      )}
    </OrganisationPageWrapper>
  )
}

type RecordViewProps = {
  organisationId: number
  recordId: number
  recordType: RecordType
  alert?: RecordViewAlertAction
}

const RecordView = async ({ organisationId, recordId, recordType, alert }: RecordViewProps) => {
  const {
    data: record,
    error,
    response,
  } = await getRecord({
    client: await createServerApiClient(),
    path: { id: recordId },
    query: { recordType, organisationId },
  })

  if (response?.status === 404) {
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

  const { data: recordList } = await getOrganisationRecords({
    client: await createServerApiClient(),
    path: { organisationId },
  })

  const recordListItem = recordList?.items?.find((item) => item.id === recordId)

  const nextUpdateDue = recordListItem?.nextUpdateDue

  const isOverdue = isRecordOverdue(nextUpdateDue)

  //TODO: needs updating when we add vaccine DTOs
  const productDetail = record.recordType === 'Medicine' ? record.recordProductDetail : undefined

  return (
    <>
      {alert && <RecordViewAlert action={alert} />}
      {backLink}
      {isOverdue && <OverdueRecordAlert />}

      <PageHeader
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

      <ManageRecordActionBanner
        record={record}
        updateRecordHref={
          record.canEditContent ? `/portal/records/${record.recordId}/task-list` : undefined
        }
      />

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
