'use client'

import { Badge, type BadgeColour } from '@/components/Badge/Badge'

import { Example } from '../../_components/Example'

const badgeColours: BadgeColour[] = [
  'blue',
  'purple',
  'bright-purple',
  'red',
  'green',
  'black',
  'grey',
]

export function Examples() {
  return (
    <>
      <Example title="Default">
        <Badge>Recently added</Badge>
      </Example>
      <Example title="Green">
        <Badge colour="green">Complete</Badge>
      </Example>
      <Example title="Red">
        <Badge colour="red">Urgent</Badge>
      </Example>
      <Example title="All colours">
        <>
          {badgeColours.map((colour) => (
            <span key={colour}>
              <Badge colour={colour}>{colour}</Badge>{' '}
            </span>
          ))}
        </>
      </Example>
      <Example title="Large">
        <>
          {badgeColours.map((colour) => (
            <span key={colour}>
              <Badge colour={colour} large>
                {colour}
              </Badge>{' '}
            </span>
          ))}
        </>
      </Example>
      <Example title="Inline with a heading">
        <h3>
          Case 12345 <Badge colour="red">Urgent</Badge>
        </h3>
      </Example>
      <Example title="With a screen reader label">
        <Badge colour="red" label="Case priority:">
          Urgent
        </Badge>
      </Example>
    </>
  )
}
