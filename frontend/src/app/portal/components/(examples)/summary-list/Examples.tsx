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
      <Example title="Without actions">
        <SummaryList variant="two-column">
          <SummaryListRow label="Name" value="Sarah Philips" />
          <SummaryListRow label="Date of birth" value="5 January 1978" />
          <SummaryListRow
            label="Address"
            value={
              <>
                72 Guild Street
                <br />
                London
                <br />
                SE23 6FH
              </>
            }
          />
        </SummaryList>
      </Example>

      <Example title="With actions">
        <SummaryList>
          <SummaryListRow label="Name" value="Sarah Philips">
            <SummaryListAction href="#change-name" visuallyHiddenText="name">
              Change
            </SummaryListAction>
          </SummaryListRow>
          <SummaryListRow label="Date of birth" value="5 January 1978">
            <SummaryListAction href="#change-date-of-birth" visuallyHiddenText="date of birth">
              Change
            </SummaryListAction>
          </SummaryListRow>
        </SummaryList>
      </Example>

      <Example title="Mixed and multiple actions">
        <SummaryList>
          <SummaryListRow label="Name" value="Sarah Philips" />
          <SummaryListRow
            label="Contact details"
            value={
              <>
                <p>07700 900457</p>
                <p>sarah.philips@example.com</p>
              </>
            }
          >
            <SummaryListAction href="#add-contact-details" visuallyHiddenText="contact details">
              Add
            </SummaryListAction>
            <SummaryListAction href="#change-contact-details" visuallyHiddenText="contact details">
              Change
            </SummaryListAction>
          </SummaryListRow>
        </SummaryList>
      </Example>
      <Example title="Card with a section action">
        <SummaryList
          actions={
            <SummaryListAction href="#edit-section" visuallyHiddenText="names and identifiers">
              Edit section
            </SummaryListAction>
          }
          card
          title="Names and identifiers"
          titleHeadingLevel={2}
        >
          <SummaryListRow label="Company code" value="GC22">
            <SummaryListAction href="#change-company-code" visuallyHiddenText="company code">
              Change
            </SummaryListAction>
          </SummaryListRow>
          <SummaryListRow
            label="Record title"
            tag={
              <Tag colour="purple" flush>
                Hidden
              </Tag>
            }
            value="In combination with methotrexate"
          >
            <SummaryListAction href="#change-record-title" visuallyHiddenText="record title">
              Change
            </SummaryListAction>
          </SummaryListRow>
        </SummaryList>
      </Example>

      <Example title="Card with text instead of rows">
        <SummaryList
          actions={
            <SummaryListAction href="#edit-pricing" visuallyHiddenText="pricing and budget impact">
              Edit section
            </SummaryListAction>
          }
          card
          intro={<p>Not yet provided</p>}
          title="Pricing and budget impact (optional)"
          titleHeadingLevel={2}
        />
      </Example>

      <Example title="Card with intro text above rows">
        <SummaryList
          card
          intro={
            <p>This task becomes mandatory within 12 months of the market authorisation date.</p>
          }
          title="Laboratory testing (optional)"
          titleHeadingLevel={2}
        >
          <SummaryListRow label="Does the product require a specific biomarker?" value="No">
            <SummaryListAction href="#change-biomarker" visuallyHiddenText="biomarker">
              Change
            </SummaryListAction>
          </SummaryListRow>
        </SummaryList>
      </Example>
    </>
  )
}
