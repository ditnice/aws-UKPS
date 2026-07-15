'use client'

import { Panel } from '@nice-digital/nds-panel'

import { Example } from '../../_components/Example'

const panelVariants = [
  [
    'supporting',
    'Supporting information',
    'Useful context that complements the main page content.',
  ],
  [
    'primary',
    'Key recommendation',
    'Offer clear information in a format the person can understand.',
  ],
  ['impact', 'Expected impact', 'This change should improve access to timely assessment.'],
  ['inverse', 'Further support', 'Contact the local medicines information service for advice.'],
  ['impact-alt', 'Evidence update', 'New surveillance evidence was considered in June 2026.'],
] as const

export function Examples() {
  return (
    <>
      <Example title="Default">
        <Panel>
          <h3>About this guidance</h3>
          <p>This guideline updates and replaces recommendations published previously.</p>
        </Panel>
      </Example>
      {panelVariants.map(([variant, title, body]) => (
        <Example dark={variant === 'inverse'} key={variant} title={`${variant} variant`}>
          <Panel variant={variant}>
            <h3>{title}</h3>
            <p>{body}</p>
          </Panel>
        </Example>
      ))}
    </>
  )
}
