import { notFound } from 'next/navigation'

import { getRecordTaskList, type RecordTaskStatus } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'
import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import {
  TaskList,
  TaskListSection,
  TaskListTask,
  type TaskStatus,
} from '@/components/TaskList/TaskList'
import { parsePositiveInteger } from '@/lib/valueParsing'

const taskStatuses: Record<RecordTaskStatus, TaskStatus> = {
  NotStarted: 'not-started',
  InProgress: 'in-progress',
  Completed: 'completed',
}

type RecordTaskListPageProps = {
  params: Promise<{ recordId: string }>
}

const RecordTaskListPage = async ({ params }: RecordTaskListPageProps) => {
  const { recordId: recordIdParam } = await params
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

  return (
    <>
      <PageHeader
        backLink={
          <BackLink href={`/portal/organisations/${taskList.organisationId}/records/${recordId}`}>
            Back
          </BackLink>
        }
        heading="Task list"
      />

      <TaskList>
        {taskList.sections.map((section) => (
          <TaskListSection key={section.id} title={section.title} titleHeadingLevel={2}>
            {section.tasks.map((task) => (
              <TaskListTask
                key={task.id}
                id={task.id}
                title={task.title}
                href={`/portal/records/${recordId}/${task.firstPageId}`}
                status={taskStatuses[task.status]}
              />
            ))}
          </TaskListSection>
        ))}
      </TaskList>
    </>
  )
}

export default RecordTaskListPage
