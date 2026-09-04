'use client'

import { useSyncExternalStore } from 'react'

import { Button } from '@/components/Button/Button'
import { buildSignInHref } from '@/lib/auth/routing'

import styles from './Account.module.scss'

// csrf_token is set alongside access_token on login and is the browser-readable session signal.
function hasSessionCookie(): boolean {
  return document.cookie.split(';').some((cookie) => cookie.trim().startsWith('csrf_token='))
}

// Cookies have no native change event; this reads them once hydration makes the DOM available.
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
      <Button buttonType="submit" variant="inverse">
        Sign out
      </Button>
    </form>
  )
}
