'use client'

import { useEffect } from 'react'

import { bannerSearchParams } from './bannerSearchParams'

/**
 * Removes the banner search parameters from the address bar once a success Alert has been shown,
 * so Back, Forward and reload do not show the banner again. Renders nothing.
 */
export function ClearBannerParams() {
  // No dependency array: this runs after every render, so a second banner on the same page (the
  // Alert stays mounted) is cleared too. It is cheap and does nothing when no parameter is present.
  useEffect(() => {
    const url = new URL(window.location.href)
    const present = bannerSearchParams.filter((name) => url.searchParams.has(name))
    if (present.length === 0) {
      return
    }
    present.forEach((name) => url.searchParams.delete(name))
    window.history.replaceState(null, '', url)
  })

  return null
}
