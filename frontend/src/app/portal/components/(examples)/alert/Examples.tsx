'use client'

import { Alert } from '@nice-digital/nds-alert'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Information (default, assertive)">
        <Alert>
          <h3>Before you start</h3>
          <p>Have the person&apos;s current medicines and recent test results available.</p>
        </Alert>
      </Example>
      <Example title="Success (non-intrusive)">
        <Alert nonIntrusive type="success">
          <h3>Changes saved</h3>
          <p>The draft has been saved and can be reviewed before publication.</p>
        </Alert>
      </Example>
      <Example title="Caution (non-intrusive)">
        <Alert nonIntrusive type="caution">
          <h3>Check the dose</h3>
          <p>Review renal function before prescribing this medicine.</p>
        </Alert>
      </Example>
      <Example title="Error (non-intrusive)">
        <Alert nonIntrusive type="error">
          <h3>There is a problem</h3>
          <p>Enter a review date before continuing.</p>
        </Alert>
      </Example>
    </>
  )
}
