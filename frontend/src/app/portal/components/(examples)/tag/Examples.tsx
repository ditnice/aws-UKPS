'use client'

import { Tag } from '@nice-digital/nds-tag'

import { Example } from '../../_components/Example'

export function Examples() {
  return (
    <>
      <Example title="Demo tags">
        <>
          <Tag>I am a tag!</Tag> <Tag success>I am also a tag!</Tag>
        </>
      </Example>
      <Example title="Standard">
        <Tag>Tag</Tag>
      </Example>
      <Example title="Phase">
        <>
          <Tag alpha>Alpha</Tag> <Tag beta>Beta</Tag> <Tag>Live</Tag>
        </>
      </Example>
      <Example title="Guidance">
        <>
          <Tag>New</Tag> <Tag updated>Updated</Tag> <Tag consultation>Consultation</Tag>
        </>
      </Example>
      <Example title="Impact">
        <Tag impact>Tag</Tag>
      </Example>
      <Example title="Flush">
        <Tag flush>Tag</Tag>
      </Example>
      <Example title="Outline">
        <Tag outline>Tag</Tag>
      </Example>
    </>
  )
}
