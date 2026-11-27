import Link from 'next/link'
import { notFound } from 'next/navigation'

import { buildRecordHref } from '@/app/portal/organisations/[id]/records/[recordId]/_lib/recordLinks'
import { RecordActionAlert } from '@/app/portal/organisations/[id]/records/_components/RecordActionAlert'
import {
  parseRecordAction,
  type RecordActionSearchParams,
} from '@/app/portal/organisations/[id]/records/_lib/recordActionAlert'
import {
  recordStatusLabels,
  recordStatusTagColours,
} from '@/app/portal/organisations/[id]/records/labels'
import { getRecordTaskList, ProblemDetails, type RecordTaskStatus } from '@/client/generated'
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
import { checkAnswersHref, pageHref, taskListReturnTo } from '@/lib/dynamic-form/links'
import { parsePositiveInteger } from '@/lib/valueParsing'

const taskStatuses: Record<RecordTaskStatus, TaskStatus> = {
  NotStarted: 'not-started',
  InProgress: 'in-progress',
  Completed: 'completed',
  // Both read "Cannot start yet", though only NotApplicable counts as complete for review and
  // submit below.
  NotApplicable: 'cannot-start',
  CannotStart: 'cannot-start',
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

  if (response?.status === 403) {
    // The organisation is unknown here, so go via /portal, which redirects to the user's records list.
    const { detail } = (error ?? {}) as ProblemDetails
    return (
      <section>
        <PageHeader
          backLink={<BackLink href="/portal">Back</BackLink>}
          heading="You can't edit this record"
        />
        <ErrorState>{detail ?? 'You do not have permission to edit this record.'}</ErrorState>
      </section>
    )
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

  const allMandatoryComplete = taskList.sections
    .filter((section) => !section.optional)
    .every((section) =>
      section.tasks.every((task) => task.status === 'Completed' || task.status === 'NotApplicable'),
    )

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
            optional={section.optional}
          >
            {section.tasks.map((task) => (
              <TaskListTask
                key={task.id}
                id={task.id}
                title={task.title}
                description={task.hint ? <PlainTextContent text={task.hint} /> : undefined}
                href={
                  task.status === 'NotApplicable' || task.status === 'CannotStart'
                    ? undefined
                    : pageHref({ recordId }, task.firstPageId)
                }
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

        {taskList.canDiscard && (
          <Button
            elementType={Link}
            href={`${buildRecordHref(
              taskList.organisationId,
              recordId,
              taskList.recordType,
              'discard',
            )}&returnTo=${taskListReturnTo}`}
            variant="secondary"
          >
            Discard record
          </Button>
        )}
      </ButtonGroup>
    </>
  )
}

export default RecordTaskListPage
