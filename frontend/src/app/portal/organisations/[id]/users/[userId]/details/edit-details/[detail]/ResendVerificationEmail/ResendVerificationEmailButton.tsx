'use client'

import { useActionState } from 'react'

import { Button } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { Navigations, resendWorkEmailVerification } from './actions'

type ResendVerificationEmailButtonProps = {
  userId: number
  navigation: Navigations
}
const ResendVerificationEmailButton = ({
  userId,
  navigation,
}: ResendVerificationEmailButtonProps) => {
  const boundAction = resendWorkEmailVerification.bind(null, navigation).bind(null, userId)
  const [state, formAction, isPending] = useActionState(boundAction, { status: 'idle' })
  return (
    <form action={formAction}>
      <Button type="submit" disabled={isPending}>
        Resend email
      </Button>

      {state.status === 'error' && <ErrorState>{state.message}</ErrorState>}
    </form>
  )
}

export default ResendVerificationEmailButton
