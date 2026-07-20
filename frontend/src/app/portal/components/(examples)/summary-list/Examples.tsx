import {
  SummaryList,
  SummaryListAction,
  SummaryListRow,
} from '@/components/SummaryList/SummaryList'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Without actions">
        <SummaryList>
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
    </>
  )
}
