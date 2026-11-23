import { Fragment } from 'react'

// Only https links are recognised, so any other scheme stays as literal text.
const linkPattern = /\[([^[\]]+)\]\((https:\/\/[^\s()]+)\)/g

type HintTextProps = {
  text: string
}

/** Hint text in which `[link text](https://...)` is shown as a link that opens in a new tab. */
export function HintText({ text }: HintTextProps) {
  // Splitting on two capture groups repeats: plain text, link text, URL.
  const parts = text.split(linkPattern)

  return parts.map((part, index) => {
    if (index % 3 === 2) return null
    if (index % 3 === 0) return <Fragment key={index}>{part}</Fragment>
    return (
      <a key={index} href={parts[index + 1]} target="_blank" rel="noopener noreferrer">
        {part} (opens in a new tab)
      </a>
    )
  })
}
