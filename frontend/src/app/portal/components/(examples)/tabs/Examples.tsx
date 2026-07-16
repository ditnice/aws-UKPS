'use client'

import { Tab, Tabs } from '@nice-digital/nds-tabs'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <Example title="Tabs">
      <Tabs>
        <Tab title="Tab 1">
          <p>Here is some content for the first tab</p>
        </Tab>
        <Tab title="Tab 2">
          <p>Here is some content for the second tab</p>
        </Tab>
        <Tab title="Tab 3">
          <p>Here is some content for the third tab</p>
        </Tab>
      </Tabs>
    </Example>
  )
}
