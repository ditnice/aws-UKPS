'use client'

import { Alphabet, Letter } from '@nice-digital/nds-alphabet'

import { Example } from '../../_components/Example'

const allLetters = 'abcdefghijklmnopqrstuvwxyz'.split('')

export function Examples() {
  return (
    <>
      <Example title="Default alphabet">
        <Alphabet>
          {allLetters.map((letter) => (
            <Letter key={letter} to={`#${letter}`} label={`Letter ${letter.toUpperCase()}`}>
              {letter.toUpperCase()}
            </Letter>
          ))}
        </Alphabet>
      </Example>
      <Example title="Chunky alphabet">
        <Alphabet chunky id="a-to-z-chunky">
          {allLetters.map((letter) => (
            <Letter key={letter} to={`#${letter}`} label={`Letter ${letter.toUpperCase()}`}>
              {letter.toUpperCase()}
            </Letter>
          ))}
        </Alphabet>
      </Example>
    </>
  )
}
