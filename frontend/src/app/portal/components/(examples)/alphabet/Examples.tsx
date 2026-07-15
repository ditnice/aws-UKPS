'use client'

import { Alphabet, Letter } from '@nice-digital/nds-alphabet'

import { Example } from '../../_components/Example'

const alphabetLetters = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'] as const

export function Examples() {
  return (
    <>
      <Example title="Standard with linked and unavailable letters">
        <Alphabet id="standard-alphabet">
          {alphabetLetters.map((letter) => (
            <Letter
              key={letter}
              label={`${letter.toUpperCase()}${letter === 'e' || letter === 'f' ? ', no entries' : ''}`}
              to={letter === 'e' || letter === 'f' ? false : '/portal/components/a-z-list'}
            >
              {letter.toUpperCase()}
            </Letter>
          ))}
        </Alphabet>
      </Example>
      <Example title="Chunky">
        <Alphabet chunky id="chunky-alphabet">
          {alphabetLetters.map((letter) => (
            <Letter
              key={letter}
              label={`Browse ${letter.toUpperCase()}`}
              to="/portal/components/alphabet"
            >
              {letter.toUpperCase()}
            </Letter>
          ))}
        </Alphabet>
      </Example>
    </>
  )
}
