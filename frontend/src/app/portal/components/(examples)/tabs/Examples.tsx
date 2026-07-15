'use client'

import { useState } from 'react'

import { Tab, Tabs } from '@nice-digital/nds-tabs'

import { Example } from '../../_components/Example'

export function Examples() {
  const [activeTab, setActiveTab] = useState<string | null>(null)

  return (
    <Example title="Multiple panels and change callback">
      <Tabs aria-label="Application information" onTabChange={setActiveTab}>
        <Tab id="overview" title="Overview">
          <h3>Overview</h3>
          <p>Review the purpose and scope of the application.</p>
        </Tab>
        <Tab id="requirements" title="Requirements">
          <h3>Requirements</h3>
          <p>Prepare the required records before completing the form.</p>
        </Tab>
        <Tab id="support" title="Support">
          <h3>Support</h3>
          <p>Contact the service team if you need help with your application.</p>
        </Tab>
      </Tabs>
      <p aria-live="polite">
        Latest value from <code>onTabChange</code>:{' '}
        <strong>{activeTab ?? 'No tab change yet'}</strong>
      </p>
    </Example>
  )
}
