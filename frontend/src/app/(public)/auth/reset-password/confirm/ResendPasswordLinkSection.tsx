'use client'

import { useCallback, useRef } from 'react'

import ResetPasswordForm from '../ResetPasswordForm'

import { useCountdown } from './countdown'

type ResendPasswordLinkSectionProps = {
  email: string
}

const ResendPasswordLinkSection = ({ email }: ResendPasswordLinkSectionProps) => {
  const { seconds, restart } = useCountdown(60)
  const isCountingDown = seconds > 0

  const messageRef = useRef<HTMLParagraphElement>(null)

  const handleSuccess = useCallback(() => {
    restart()
    messageRef.current?.focus()
  }, [restart, messageRef])

  return (
    <>
      <div ref={messageRef} tabIndex={-1}>
        <p>An email containing a confirmation code has been sent to {email}.</p>
        <p>
          If you cannot find the email, check your spam or junk folder. If you still do not receive
          it, you can request another link
          {isCountingDown && <strong> in {seconds} seconds</strong>}.
        </p>
      </div>

      {/* Always mounted so changes are reliably announced */}
      <div role="status" className="visually-hidden">
        {!isCountingDown && 'You can now request another link.'}
      </div>

      <div className={isCountingDown ? 'invisible' : undefined} inert={isCountingDown || undefined}>
        <ResetPasswordForm
          email={email}
          buttonContentOverride="Resend link"
          onSuccess={handleSuccess}
        />
      </div>
    </>
  )
}

export default ResendPasswordLinkSection
