import { TaskList, TaskListSection, TaskListTask } from '@/components/TaskList/TaskList'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Product record (full design)">
        <TaskList>
          <TaskListSection title="Provide product record details">
            <TaskListTask
              id="product-record-details"
              title="Name and identifiers"
              href="#product-record-details"
              description="Enter the company codes generic names, and branded names ..."
              status="in-progress"
            />
          </TaskListSection>

          <TaskListSection title="Provide indication and development information">
            <TaskListTask
              id="indication"
              title="Indication details"
              href="#indication"
              description="Enter the condition this product treats and its disease area"
              status="completed"
            />
            <TaskListTask
              id="development"
              title="Development background"
              href="#development"
              description="Enter the development details for this product ..."
              status="not-started"
            />
          </TaskListSection>

          <TaskListSection
            title="Provide service readiness information (optional)"
            description="This section is optional ..."
            status="in-progress"
            collapsible
          >
            <TaskListTask
              id="lab-testing"
              title="Laboratory testing"
              href="#lab-testing"
              description="Enter details of requireed laboratory tests ..."
              status="in-progress"
            />
            <TaskListTask
              id="patient-and-clinical-requirements"
              title="Patient and clinical requirements"
              href="#patient-and-clinical-requirements"
              description="Describe how you identify patients and calculate ..."
              status="not-started"
            />
          </TaskListSection>
        </TaskList>
      </Example>
    </>
  )
}
