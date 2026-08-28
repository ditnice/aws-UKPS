'use client'

import { useRouter } from 'next/navigation'

import { BackLink } from '@/components/BackLink/BackLink'

export function BackLinkBrowser() {
  const router = useRouter()

  return (
    <BackLink
      href="#"
      onClick={(event) => {
        event.preventDefault()
        router.back()
      }}
    />
  )
}
