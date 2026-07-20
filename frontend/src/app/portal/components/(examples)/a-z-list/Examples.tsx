'use client'

import { AZList, AZListItem } from '@nice-digital/nds-a-z-list'
import { Alphabet, Letter } from '@nice-digital/nds-alphabet'

import { Example } from '../../_components/Example'

const allLetters = 'abcdefghijklmnopqrstuvwxyz'.split('')

function AlphabetComponent() {
  return (
    <Alphabet>
      {allLetters.map((letter) => (
        <Letter key={letter} to={`#${letter}`} label={`Letter ${letter.toUpperCase()}`}>
          {letter.toUpperCase()}
        </Letter>
      ))}
    </Alphabet>
  )
}

export function Examples() {
  return (
    <Example title="A to Z list">
      <AZList alphabet={AlphabetComponent}>
        <AZListItem title="A">
          <p className="test-class">A: lorem ipsum dolor sit amet</p>
        </AZListItem>
        <AZListItem title="B">
          <p className="test-class">B: lorem ipsum dolor sit amet</p>
        </AZListItem>
      </AZList>
    </Example>
  )
}
