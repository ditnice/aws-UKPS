import { TaskList, TaskListSection, TaskListTask } from '@/components/TaskList/TaskList'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Product record (full design)">
        <TaskList>
          <TaskListSection title="Provide product record details">
            <TaskListTask
              id="names-and-identifiers"
              title="Names and identifiers"
              href="#names-and-identifiers"
              status="completed"
            />
          </TaskListSection>

          <TaskListSection title="Provide indication and development information">
            <TaskListTask
              id="indication-details"
              title="Indication details"
              href="#indication-details"
              status="completed"
            />
            <TaskListTask
              id="development-background"
              title="Development background"
              href="#development-background"
              description="Enter development details for this product, such as the originator company and whether it is a repurposed medicine."
              status="not-started"
            />
          </TaskListSection>

          <TaskListSection
            title="Provide service readiness information (optional)"
            description="This section is optional. It becomes mandatory within 12 months of the market authorisation date."
            optional
          >
            <TaskListTask
              id="laboratory-testing"
              title="Laboratory testing"
              href="#laboratory-testing"
              description="Enter details of required laboratory tests. Indicate whether genomic or non-genomic biomarkers are used for diagnostic tests."
              status="not-started"
            />
            <TaskListTask
              id="patient-clinical-requirements"
              title="Patient and clinical requirements"
              href="#patient-clinical-requirements"
              description="Describe how you identify patients and calculate eligible patient population ranges. Enter clinical setting requirements for this product, for example, storage requirements."
              status="not-started"
            />
            <TaskListTask
              id="pricing-budget-impact"
              title="Pricing and budget impact"
              href="#pricing-budget-impact"
              description="Enter anticipated costs, funding arrangements, and pricing flexibility."
              status="not-started"
            />
          </TaskListSection>
        </TaskList>
      </Example>

      <Example title="Cannot start yet status">
        <TaskList>
          <TaskListSection title="Provide product record details">
            <TaskListTask
              id="names-and-identifiers-2"
              title="Names and identifiers"
              href="#names-and-identifiers-2"
              status="completed"
            />
          </TaskListSection>

          <TaskListSection title="Provide indication and development information">
            <TaskListTask
              id="indication-details-2"
              title="Indication details"
              href="#indication-details-2"
              status="not-started"
            />
          </TaskListSection>

          <TaskListSection title="Provide special designation information">
            <TaskListTask
              title="Promising Innovative Medicine (PIM) and Early Access to Medicines Scheme (EAMS)"
              description="Complete the indication details section first."
              status="cannot-start"
            />
          </TaskListSection>
        </TaskList>
      </Example>

      <Example title="Not applicable status">
        <TaskList>
          <TaskListSection title="Provide product record details">
            <TaskListTask
              id="names-and-identifiers-3"
              title="Names and identifiers"
              href="#names-and-identifiers-3"
              status="completed"
            />
          </TaskListSection>

          <TaskListSection title="Provide indication and development information">
            <TaskListTask
              id="indication-details-3"
              title="Indication details"
              href="#indication-details-3"
              status="not-started"
            />
          </TaskListSection>

          <TaskListSection title="Provide special designation information">
            <TaskListTask
              title="Promising Innovative Medicine (PIM) and Early Access to Medicines Scheme (EAMS)"
              description="Complete the indication details section first."
              status="not-applicable"
            />
          </TaskListSection>
        </TaskList>
      </Example>
    </>
  )
}
