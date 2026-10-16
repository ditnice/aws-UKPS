import { TaskList, TaskListSection, TaskListTask } from '@/components/TaskList/TaskList'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Product record (full design)">
        <TaskList>
          <TaskListSection title="Example Title">
            <TaskListTask
              title="Example Title"
              href="#href"
              description="Example description"
              status="in-progress"
            />
          </TaskListSection>

          <TaskListSection title="Example Title">
            <TaskListTask
              title="Example Title"
              href="#href"
              description="Example description"
              status="in-progress"
            />
            <TaskListTask
              title="Example Title"
              href="#href"
              description="Example description"
              status="in-progress"
            />
          </TaskListSection>

          <TaskListSection
            title="Example Title"
            description="Example description"
            status="in-progress"
            collapsible
          >
            <TaskListTask
              title="Example Title"
              href="#href"
              description="Example description"
              status="in-progress"
            />
            <TaskListTask
              title="Example Title"
              href="#href"
              description="Example description"
              status="not-started"
            />
          </TaskListSection>
        </TaskList>
      </Example>
    </>
  )
}
