'use client'

import { FormGroup } from '@nice-digital/nds-form-group'
import { Textarea } from '@nice-digital/nds-textarea'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Overview and how to use">
        <Textarea
          label="Tell us more about your request or issue."
          name="issue_description"
        ></Textarea>
      </Example>
      <Example title="Grouped textareas">
        <FormGroup legend="Feedback Details">
          <Textarea label="What could be improved?" name="improvement_feedback"></Textarea>
          <Textarea label="Additional Comments" name="additional_comments"></Textarea>
        </FormGroup>
      </Example>
      <Example title="Individual input hint text">
        <Textarea
          label="Tell us more about your request or issue."
          name="issue_description_hint"
          hint="Please describe the issue in detail, including any error messages you have received."
        ></Textarea>
      </Example>
      <Example title="Grouped inputs hint text">
        <FormGroup
          legend="Feedback Details"
          hint="Please answer all fields clearly and avoid including any personal or sensitive information."
        >
          <Textarea
            label="What could be improved?"
            name="improvement_feedback_group_hint"
          ></Textarea>
          <Textarea label="Additional Comments" name="additional_comments_group_hint"></Textarea>
        </FormGroup>
      </Example>
      <Example title="Error messages">
        <Textarea
          label="Tell us more about your request or issue."
          name="issue_description_error"
          error={true}
          errorMessage="Please describe your request or issue."
        ></Textarea>
      </Example>
    </>
  )
}
