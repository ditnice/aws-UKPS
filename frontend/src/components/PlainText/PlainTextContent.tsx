import { Fragment } from 'react/jsx-runtime'

type PlainTextContentProps = {
  text: string
}

function renderParagraph(block: string) {
  const lines = block.split('\n')

  return (
    <p>
      {lines.map((line, index) => (
        <span key={`${line}-${index}`}>
          {index > 0 ? <br /> : null}
          {line}
        </span>
      ))}
    </p>
  )
}

export default function PlainTextContent({ text }: PlainTextContentProps) {
  const blocks = text
    .split(/\n\s*\n/)
    .map((block) => block.trim())
    .filter(Boolean)

  return blocks.map((block, index) => {
    const lines = block.split('\n').map((line) => line.trim())
    const isList = lines.every((line) => line.startsWith('- '))

    if (isList) {
      return (
        <ul key={`${block}-${index}`}>
          {lines.map((line) => (
            <li key={line}>{line.replace(/^- /, '')}</li>
          ))}
        </ul>
      )
    }

    return <Fragment key={`${block}-${index}`}>{renderParagraph(block)}</Fragment>
  })
}
