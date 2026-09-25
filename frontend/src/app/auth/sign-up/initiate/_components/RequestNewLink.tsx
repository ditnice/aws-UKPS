'use client'

import { useEffect, useState } from 'react'

import { postAuthResendSetupToken } from '@/client/generated'
import { Button } from '@/components/Button/Button'

import styles from '../page.module.scss'

import { SignUpInitiateError } from './SignUpInitiateError'

import type { ReactNode } from 'react'

const resendCooldownSeconds = 60

type RequestNewLinkProps = {
  setupToken: string
}

type Status = 'idle' | 'sent' | 'tooManyAttempts' | 'notFound' | 'genericError'

type PageContent = {
  detail: ReactNode
  title: string
}

function getPageContent(
  status: Status,
  remainingSeconds: number,
  countdownRole: 'timer' | 'alert',
): PageContent {
  const supportEmail = process.env.NEXT_PUBLIC_QA_SUPPORT_EMAIL
  const contactSupport = (
    <>
      Please contact the UKPS support team for assistance{' '}
      <a href={`mailto:${supportEmail}`} data-testid="support-email-link">
        {supportEmail}
      </a>
      .
    </>
  )
  switch (status) {
    case 'idle':
      return {
        title: 'This link has expired',
        detail:
          'Request a new link to continue setting up your account. A new link will be sent to your registered email address.',
      }
    case 'sent':
      return {
        title: 'Check your email',
        detail: (
          <>
            <p>
              We&apos;ve sent a new link to your email address. It may take a few minutes to arrive.
            </p>
            <p>
              If you cannot find the email, check your spam or junk folder. If you still do not
              receive it, you can request another link in{' '}
              <span role={countdownRole} aria-atomic="true">
                <strong>
                  {remainingSeconds} second{remainingSeconds === 1 ? '' : 's'}
                </strong>
              </span>
              .
            </p>
          </>
        ),
      }
    case 'tooManyAttempts':
      return {
        title: 'Contact the support team',
        detail: (
          <>
            <p>You have reached the maximum number of attempts to request a new link.</p>
            {contactSupport}
          </>
        ),
      }
    case 'notFound':
    case 'genericError':
      return { title: 'Contact the support team', detail: contactSupport }
  }
}

export function RequestNewLink({ setupToken }: RequestNewLinkProps) {
  const [status, setStatus] = useState<Status>('idle')
  const [coolingDown, setCoolingDown] = useState(false)
  const [remainingSeconds, setRemainingSeconds] = useState(resendCooldownSeconds)
  const [correlationId, setCorrelationId] = useState<string | null>(null)
  const [countdownRole, setCountdownRole] = useState<'timer' | 'alert'>('timer')

  useEffect(() => {
    if (!coolingDown) return

    let alertRevertTimeout: ReturnType<typeof setTimeout> | undefined

    const interval = setInterval(() => {
      setRemainingSeconds((seconds) => {
        if (seconds <= 1) {
          setCoolingDown(false)
          return resendCooldownSeconds
        }

        const nextSeconds = seconds - 1
        if (nextSeconds === 10) {
          // Briefly switch the countdown to an assertive alert at the 10-second
          // mark so screen reader users get one heads-up, instead of an
          // announcement every second.
          setCountdownRole('alert')
          alertRevertTimeout = setTimeout(() => setCountdownRole('timer'), 1000)
        }

        return nextSeconds
      })
    }, 1000)

    return () => {
      clearInterval(interval)
      clearTimeout(alertRevertTimeout)
    }
  }, [coolingDown])

  async function handleClick() {
    setCoolingDown(true)
    setRemainingSeconds(resendCooldownSeconds)

    try {
      const result = await postAuthResendSetupToken({
        body: correlationId ? { correlationId } : { setupToken },
      })

      if (!result.error) {
        setCorrelationId(result.data?.correlationId ?? null)
        setStatus('sent')
      } else if (result.response?.status === 403) {
        setStatus('tooManyAttempts')
      } else if (result.response?.status === 404) {
        setStatus('notFound')
      } else {
        setStatus('genericError')
      }
    } catch {
      setStatus('genericError')
    }
  }

  const showResendButton = status === 'idle' || status === 'sent'

  return (
    <>
      <SignUpInitiateError {...getPageContent(status, remainingSeconds, countdownRole)} />
      {showResendButton && (
        <div className={styles.actions}>
          <Button
            disabled={coolingDown}
            onClick={handleClick}
            variant={coolingDown ? 'secondary' : 'cta'}
          >
            Send a new link
          </Button>
        </div>
      )}
    </>
  )
}
