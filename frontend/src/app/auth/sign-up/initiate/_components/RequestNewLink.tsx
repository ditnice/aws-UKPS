'use client'

import Link from 'next/link'
import { useEffect, useState } from 'react'

import { postAuthResendSetupToken } from '@/client/generated'
import { Button } from '@/components/Button/Button'

import { SignUpInitiateError } from './SignUpInitiateError'

const resendCooldownSeconds = 60

type RequestNewLinkProps = {
  setupToken: string
}

type Status = 'idle' | 'sent' | 'tooManyAttempts' | 'notFound' | 'genericError'

export function RequestNewLink({ setupToken }: RequestNewLinkProps) {
  const [status, setStatus] = useState<Status>('idle')
  const [coolingDown, setCoolingDown] = useState(false)
  const [remainingSeconds, setRemainingSeconds] = useState(resendCooldownSeconds)

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
      const result = await postAuthResendSetupToken({ body: { setupToken } })

      if (!result.error) {
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

  if (status === 'sent') {
    return (
      <>
        <SignUpInitiateError
          title="Check your email"
          detail={
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
          }
        />
        <Button
          disabled={coolingDown}
          onClick={handleClick}
          variant={coolingDown ? 'secondary' : 'cta'}
        >
          Send a new link
        </Button>
      </>
    )
  }

  if (status === 'tooManyAttempts') {
    return (
      <SignUpInitiateError
        title="Check your email"
        detail={
          <>
            You have reached the maximum number of attempts to request a new link.
            <br />
            Contact <Link href="/">UKPS support</Link> for help completing your registration.
          </>
        }
      />
    )
  }

  if (status === 'notFound') {
    return (
      <SignUpInitiateError
        title="We could not find this sign-up link"
        detail={
          <>
            You may have already requested a new link. Check your email for the most recent one,
            including your spam or junk folder.
            <br />
            If you still need help, contact <Link href="/">UKPS support</Link> for help completing
            your registration.
          </>
        }
      />
    )
  }

  if (status === 'genericError') {
    return (
      <SignUpInitiateError
        title="We could not send a new link"
        detail={
          <>
            Something went wrong and we could not send you a new link.
            <br />
            Contact <Link href="/">UKPS support</Link> for help completing your registration.
          </>
        }
      />
    )
  }

  return (
    <>
      <SignUpInitiateError
        title="This link has expired"
        detail="Request a new link to continue setting up your account. A new link will be sent to your registered email address."
      />
      <Button
        disabled={coolingDown}
        onClick={handleClick}
        variant={coolingDown ? 'secondary' : 'cta'}
      >
        Send a new link
      </Button>
    </>
  )
}
