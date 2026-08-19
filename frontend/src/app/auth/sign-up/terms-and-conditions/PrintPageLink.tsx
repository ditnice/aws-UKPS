'use client'

export function PrintPageLink() {
  return (
    <a
      href="#"
      onClick={(event) => {
        event.preventDefault()
        window.print()
      }}
    >
      Print page
    </a>
  )
}
