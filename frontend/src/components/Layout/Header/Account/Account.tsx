'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useSyncExternalStore } from 'react'

import { postAuthSignOut } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { csrfCookieName, csrfHeaderName, getCookie } from '@/lib/auth/cookies'
import { buildSignInHref } from '@/lib/auth/routing'

// csrf_token is set alongside access_token on login and is the browser-readable session signal.
function hasSessionCookie(): boolean {
  return getCookie(csrfCookieName) !== null
}

// Cookies have no native change event, so listeners are notified when this component changes them.
const sessionListeners = new Set<() => void>()

function subscribe(listener: () => void) {
  sessionListeners.add(listener)
  return () => {
    sessionListeners.delete(listener)
  }
}

function getServerSnapshot(): boolean {
  return false
}

// Sign-out goes through the /backend-api proxy so the browser sends the refresh_token cookie
// (scoped to /backend-api/auth) and receives the backend's cookie-clearing Set-Cookie headers.
async function signOut(): Promise<boolean> {
  try {
    const { error } = await postAuthSignOut({
      credentials: 'include',
      headers: { [csrfHeaderName]: getCookie(csrfCookieName) ?? '' },
    })

    if (error) {
      console.error('Sign-out request was rejected', { error })
      return false
    }
  } catch (error) {
    console.error('Sign-out request failed', {
      error: error instanceof Error ? error.message : error,
    })
    return false
  }

  for (const listener of sessionListeners) listener()
  return true
}

export function Account() {
  const router = useRouter()
  const isLoggedIn = useSyncExternalStore(subscribe, hasSessionCookie, getServerSnapshot)

  if (!isLoggedIn) {
    return (
      <Button elementType={Link} href={buildSignInHref(undefined)} variant="inverse">
        Sign in
      </Button>
    )
  }

  return (
    <Button
      variant="inverse"
      onClick={async () => {
        if (await signOut()) router.push('/')
      }}
    >
      Sign out
    </Button>
  )
}
