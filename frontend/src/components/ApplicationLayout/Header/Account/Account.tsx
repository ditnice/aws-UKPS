'use client'

import { useSyncExternalStore } from 'react'

import { Button } from '@nice-digital/nds-button'

import { buildSignInHref } from '@/app/auth/constants'

import styles from './Account.module.scss'

// csrf_token is set alongside access_token on login and isn't HttpOnly, so it's the only auth
// cookie readable from the browser - the same signal frontend/src/client/hey-api.ts already reads.
function hasSessionCookie(): boolean {
  return document.cookie.split(';').some((cookie) => cookie.trim().startsWith('csrf_token='))
}

// There's no native change event for cookies, so there's nothing to subscribe to - this just lets
// useSyncExternalStore read the cookie once the DOM is available, matching `false` on the server.
function subscribe() {
  return () => {}
}

function getServerSnapshot(): boolean {
  return false
}

export function Account() {
  const isLoggedIn = useSyncExternalStore(subscribe, hasSessionCookie, getServerSnapshot)

  if (!isLoggedIn) {
    return (
      <Button to={buildSignInHref(undefined)} variant="inverse">
        Sign in
      </Button>
    )
  }

  return (
    <form action="/auth/sign-out" method="POST" className={styles.signOutForm}>
      <Button type="submit" variant="inverse">
        Sign out
      </Button>
    </form>
  )
}
