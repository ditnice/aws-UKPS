'use client'

import { useSearchParams } from 'next/navigation'
import { useEffect, useState, type ReactNode } from 'react'

import { bannerSearchParams } from './bannerSearchParams'

/**
 * Shows its children only when one of the `bannerSearchParams` is in the URL as the page mounts,
 * then removes those parameters from the address bar. The server keeps rendering the banner; this
 * decides whether it is visible. When Back or Forward restores the cached page, the component
 * mounts again with the clean URL and renders nothing, so the banner is not repeated.
 */
export function ShowOnce({ children }: { children: ReactNode }) {
  const searchParams = useSearchParams()
  const hasBannerParam = bannerSearchParams.some((name) => searchParams.has(name))
  const [show, setShow] = useState(hasBannerParam)

  // The page can stay mounted when a new action adds a parameter, so switch the banner on then
  if (hasBannerParam && !show) {
    setShow(true)
  }

  useEffect(() => {
    const url = new URL(window.location.href)
    const present = bannerSearchParams.filter((name) => url.searchParams.has(name))
    if (present.length === 0) {
      return
    }
    present.forEach((name) => url.searchParams.delete(name))
    window.history.replaceState(null, '', url)
  }, [hasBannerParam])

  return show ? children : null
}
