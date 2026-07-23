'use client'

import { Tag, type TagColour } from '@/components/Tag/Tag'

import { Example } from '../../_components/Example'

const tagColours: TagColour[] = [
  'grey',
  'green',
  'teal',
  'blue',
  'purple',
  'magenta',
  'red',
  'orange',
  'yellow',
]

export function Examples() {
  return (
    <>
      <Example title="Demo tags">
        <>
          <Tag>I am a tag!</Tag> <Tag colour="green">I am also a tag!</Tag>
        </>
      </Example>
      <Example title="Standard">
        <Tag>Tag</Tag>
      </Example>
      <Example title="Custom colours">
        <>
          {tagColours.map((colour) => (
            <div key={colour}>
              <Tag colour={colour}>{colour}</Tag>
            </div>
          ))}
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
