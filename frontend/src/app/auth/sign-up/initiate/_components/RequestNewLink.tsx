'use client'

import { useEffect, useState } from 'react'

import { postAuthResendSetupToken } from '@/client/generated'
import { Button } from '@/components/Button/Button'

import { SignUpInitiateError } from './SignUpInitiateError'

import type { ReactNode } from 'react'

const resendCooldownSeconds = 60
const supportEmail = 'QA@UKPS.com'

type RequestNewLinkProps = {
  setupToken: string
}

type Status = 'idle' | 'sent' | 'tooManyAttempts' | 'notFound' | 'genericError'

type ErrorContent = {
  detail: ReactNode
  title: string
}

const contactSupport = (
  <>
    Please contact the UKPS support team for assistance{' '}
    <a href={`mailto:${supportEmail}`}>{supportEmail}</a>.
  </>
)

function getErrorContent(status: Status, remainingSeconds: number): ErrorContent {
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
            We&apos;ve sent a new link to your email address. It may take a few minutes to arrive.
            <br />
            <br />
            If you cannot find the email, check your spam or junk folder. If you still do not
            receive it, you can request another link in{' '}
            <strong>
              {remainingSeconds} second{remainingSeconds === 1 ? '' : 's'}
            </strong>
            .
          </>
        ),
      }
    case 'tooManyAttempts':
      return {
        title: 'Contact the support team',
        detail: (
          <>
            You have reached the maximum number of attempts to request a new link.
            <br />
            <br />
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

  useEffect(() => {
    if (!coolingDown) return

    const interval = setInterval(() => {
      setRemainingSeconds((seconds) => {
        if (seconds <= 1) {
          setCoolingDown(false)
          return resendCooldownSeconds
        }
        return seconds - 1
      })
    }, 1000)

    return () => clearInterval(interval)
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
      <SignUpInitiateError {...getErrorContent(status, remainingSeconds)} />
      {showResendButton && (
        <Button
          disabled={coolingDown}
          onClick={handleClick}
          variant={coolingDown ? 'secondary' : 'cta'}
        >
          Send a new link
        </Button>
      )}
    </>
  )
}
