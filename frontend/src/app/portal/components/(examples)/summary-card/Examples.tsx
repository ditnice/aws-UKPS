import { SummaryCard } from '@/components/SummaryCard/SummaryCard'
import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'
import { Tag } from '@/components/Tag/Tag'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Summary list with a section action">
        <SummaryCard
          actions={
            <SummaryListAction href="#edit-section" visuallyHiddenText="names and identifiers">
              Edit section
            </SummaryListAction>
          }
          title="Names and identifiers"
        >
          <SummaryList variant="wide-value">
            <SummaryListRow label="Company code" value="GC22">
              <SummaryListAction href="#change-company-code" visuallyHiddenText="company code">
                Change
              </SummaryListAction>
            </SummaryListRow>
            <SummaryListRow label="Generic name" value="Lorem ipsum dolor sit amet">
              <SummaryListAction href="#change-generic-name" visuallyHiddenText="generic name">
                Change
              </SummaryListAction>
            </SummaryListRow>
          </SummaryList>
        </SummaryCard>
      </Example>

      <Example title="Several section actions">
        <SummaryCard
          actions={[
            <SummaryListAction href="#edit-trial" key="edit" visuallyHiddenText="trial 1">
              Edit
            </SummaryListAction>,
            <SummaryListAction href="#remove-trial" key="remove" visuallyHiddenText="trial 1">
              Remove
            </SummaryListAction>,
          ]}
          title="Trial 1"
          titleHeadingLevel={3}
        >
          <SummaryList variant="wide-value">
            <SummaryListRow label="Study name" value="Lorem ipsum dolor sit amet" />
          </SummaryList>
        </SummaryCard>
      </Example>

      <Example title="Tag beside an action">
        <SummaryCard
          actions={
            <SummaryListAction href="#edit-empty-trial" visuallyHiddenText="trial 1">
              Edit
            </SummaryListAction>
          }
          tag={<Tag colour="red">There is a problem</Tag>}
          title="Trial 1"
          titleHeadingLevel={3}
        >
          <SummaryList variant="wide-value">
            <SummaryListRow label="Not yet provided" value="" />
          </SummaryList>
        </SummaryCard>
      </Example>

      <Example title="Text instead of a list">
        <SummaryCard
          actions={
            <SummaryListAction href="#edit-pricing" visuallyHiddenText="pricing and budget impact">
              Edit section
            </SummaryListAction>
          }
          title="Pricing and budget impact (optional)"
        >
          <p>Not yet provided</p>
        </SummaryCard>
      </Example>

      <Example title="Intro text above a summary list">
        <SummaryCard title="Laboratory testing (optional)">
          <p>This task becomes mandatory within 12 months of the market authorisation date.</p>
          <SummaryList variant="wide-value">
            <SummaryListRow label="Does the product require a specific biomarker?" value="No">
              <SummaryListAction href="#change-biomarker" visuallyHiddenText="biomarker">
                Change
              </SummaryListAction>
            </SummaryListRow>
          </SummaryList>
        </SummaryCard>
      </Example>
    </>
  )
}
