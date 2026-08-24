'use client'

import { Button } from '@/components/Button/Button'

export function PrintPageLink() {
  return (
    <Button variant="link" onClick={() => window.print()}>
      Print page
    </Button>
  )
}
