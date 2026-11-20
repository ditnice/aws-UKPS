import Link from 'next/link'
import { notFound } from 'next/navigation'

import { RecordActionAlert } from '@/app/portal/organisations/[id]/records/_components/RecordActionAlert'
import {
  parseRecordAction,
  type RecordActionSearchParams,
} from '@/app/portal/organisations/[id]/records/_lib/recordActionAlert'
import {
  recordStatusLabels,
  recordStatusTagColours,
} from '@/app/portal/organisations/[id]/records/labels'
import { getRecordTaskList, type RecordTaskStatus } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import PlainTextContent from '@/components/PlainText/PlainTextContent'
import { Tag } from '@/components/Tag/Tag'
import {
  TaskList,
  TaskListSection,
  TaskListTask,
  type TaskStatus,
} from '@/components/TaskList/TaskList'
import { checkAnswersHref } from '@/lib/dynamic-form/links'
import { parsePositiveInteger } from '@/lib/valueParsing'

const taskStatuses: Record<RecordTaskStatus, TaskStatus> = {
  NotStarted: 'not-started',
  InProgress: 'in-progress',
  Completed: 'completed',
}

type RecordTaskListPageProps = {
  params: Promise<{ recordId: string }>
  searchParams: Promise<RecordActionSearchParams>
}

const RecordTaskListPage = async ({ params, searchParams }: RecordTaskListPageProps) => {
  const { recordId: recordIdParam } = await params
  const action = parseRecordAction(await searchParams)
  const recordId = parsePositiveInteger(recordIdParam)

  if (!recordId) {
    notFound()
  }

  const {
    data: taskList,
    error,
    response,
  } = await getRecordTaskList({
    client: await createServerApiClient(),
    path: { recordId },
  })

  if (response?.status === 404) {
    notFound()
  }

  if (!taskList || error) {
    return (
      <section>
        <PageHeader heading="Unable to load task list" />
        <ErrorState>
          There was a problem retrieving this task list. Please try again later.
        </ErrorState>
      </section>
    )
  }

  // TODO: service readiness becomes mandatory within 12 months of market authorisation;
  // that rule requires backend work, so it is marked always optional here for now. When
  // that rule is implemented, the optional section IDs should be determined by the backend
  const optionalSectionIds = ['service-readiness']
  const allMandatoryComplete = taskList.sections
    .filter((section) => !optionalSectionIds.includes(section.id))
    .every((section) => section.tasks.every((task) => task.status === 'Completed'))

  return (
    <>
      {action === 'draft-saved' && <RecordActionAlert action="draft-saved" />}
      <PageHeader
        backLink={
          <BackLink href={`/portal/organisations/${taskList.organisationId}/records`}>
            Back
          </BackLink>
        }
        heading={`Record ${recordId}${taskList.companyCode ? `: ${taskList.companyCode}` : ''}`}
        lead={
          <>
            <strong>Record title:</strong> {taskList.recordTitle}
          </>
        }
        metadata={[
          <Tag key="status" colour={recordStatusTagColours[taskList.displayStatus]}>
            {recordStatusLabels[taskList.displayStatus]}
          </Tag>,
        ]}
      />

      <TaskList>
        {taskList.sections.map((section) => (
          <TaskListSection
            key={section.id}
            title={section.title}
            description={section.subText ? <PlainTextContent text={section.subText} /> : undefined}
            titleHeadingLevel={2}
            optional={optionalSectionIds.includes(section.id)}
          >
            {section.tasks.map((task) => (
              <TaskListTask
                key={task.id}
                id={task.id}
                title={task.title}
                description={task.hint ? <PlainTextContent text={task.hint} /> : undefined}
                href={`/portal/records/${recordId}/${task.firstPageId}`}
                status={taskStatuses[task.status]}
              />
            ))}
          </TaskListSection>
        ))}
      </TaskList>

      <ButtonGroup>
        {allMandatoryComplete && (
          <Button elementType={Link} href={checkAnswersHref(recordId)} variant="cta">
            Review and submit record
          </Button>
        )}
        <Button
          elementType={Link}
          href={`/portal/organisations/${taskList.organisationId}/records`}
        >
          Return to my records
        </Button>
      </ButtonGroup>
    </>
  )
}

export default RecordTaskListPage
