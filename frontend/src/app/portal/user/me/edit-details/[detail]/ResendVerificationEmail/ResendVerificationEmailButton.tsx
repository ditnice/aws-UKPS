'use client'

import { useActionState } from 'react'

import { Button } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { resendWorkEmailVerification } from './actions'

type ResendVerificationEmailButtonProps = {
  userId: number
}
const ResendVerificationEmailButton = ({ userId }: ResendVerificationEmailButtonProps) => {
  const boundAction = resendWorkEmailVerification.bind(null, userId)
  const [state, formAction, isPending] = useActionState(boundAction, { status: 'idle' })
  return (
    <form action={formAction}>
      <Button type="submit" disabled={isPending}>
        Didn&apos;t get it? Resend email
      </Button>

      {state.status === 'error' && <ErrorState>{state.message}</ErrorState>}
      {state.status === 'success' && (
        <p role="status">Verification email sent. Check your inbox.</p>
      )}
    </form>
  )
}

export default ResendVerificationEmailButton
