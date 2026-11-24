import { notFound } from 'next/navigation'

import { getRecordDetail, RecordType } from '@/client/generated'
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
import { RecordDetails } from './_components/RecordDetails'
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

const OverdueRecordAlert = ({ canEditOrConfirm }: { canEditOrConfirm: boolean }) => (
  <Alert type="info">
    <h3>Record update overdue</h3>
    <p>
      This record needs to be reviewed for any updates.
      {canEditOrConfirm &&
        ' You can edit the record to update published record information or confirm the information is up to date.'}
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
  const { data, error, response } = await getRecordDetail({
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

  if (error || !data) {
    return (
      <section>
        <PageHeader heading="Unable to load record" backLink={backLink} />
        <ErrorState>There was a problem retrieving the record. Please try again later.</ErrorState>
      </section>
    )
  }

  const { overview: record, sections } = data
  const isOverdue = isRecordOverdue(record.nextUpdateDue)

  // Based on what the user is permitted to do, so it is never true for a super user.
  const canEditOrConfirm =
    record.canEditContent ||
    record.actions.includes('ConfirmCurrent') ||
    record.actions.includes('ConfirmOnHold')

  return (
    <>
      {alert && <RecordViewAlert action={alert} />}
      {backLink}
      {isOverdue && record.recordStatus !== 'Archived' && (
        <OverdueRecordAlert canEditOrConfirm={canEditOrConfirm} />
      )}

      <PageHeader
        heading={`Record ${record.recordId}${record.companyCode ? `: ${record.companyCode}` : ''}`}
        lead={
          <>
            <strong>Record title:</strong> {record.recordTitle}
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
      {sections.length > 0 ? (
        <RecordDetails sections={sections} />
      ) : (
        <p>Viewing the details of this type of record is not yet available.</p>
      )}
    </>
  )
}

export default RecordPage
