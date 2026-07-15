'use client'

import { AZList, AZListItem } from '@nice-digital/nds-a-z-list'
import { Alphabet, Letter } from '@nice-digital/nds-alphabet'

import { Example } from '../../_components/Example'

const listLetters = ['a', 'c', 'd', 'h', 's'] as const

function ListAlphabet({ className }: { className?: string }) {
  return (
    <Alphabet className={className} id="conditions-a-to-z">
      {listLetters.map((letter) => (
        <Letter
          key={letter}
          label={`Conditions beginning with ${letter.toUpperCase()}`}
          to={`#az-${letter}`}
        >
          {letter.toUpperCase()}
        </Letter>
      ))}
    </Alphabet>
  )
}

export function Examples() {
  return (
    <Example title="Condition topics with an alphabet index">
      <AZList alphabet={ListAlphabet} aria-label="Conditions by initial letter">
        <AZListItem id="az-a" title="A">
          <ul>
            <li>
              <a href="#az-a">Asthma</a>
            </li>
            <li>
              <a href="#az-a">Atrial fibrillation</a>
            </li>
          </ul>
        </AZListItem>
        <AZListItem id="az-c" title="C">
          <p>
            <a href="#az-c">Chronic kidney disease</a>
          </p>
        </AZListItem>
        <AZListItem id="az-d" title="D">
          <p>
            <a href="#az-d">Diabetes</a>
          </p>
        </AZListItem>
        <AZListItem id="az-h" title="H">
          <p>
            <a href="#az-h">Hypertension</a>
          </p>
        </AZListItem>
        <AZListItem id="az-s" title="S">
          <p>
            <a href="#az-s">Stroke and transient ischaemic attack</a>
          </p>
        </AZListItem>
      </AZList>
    </Example>
  )
}
