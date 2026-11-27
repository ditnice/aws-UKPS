'use client'

import { startTransition, useActionState } from 'react'

import { Button } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

import { cancelPendingEmailChangeAction } from './serverActions'

type Props = {
  userId: number
  onSuccessRedirect: string
}

type State = { errors: string[] } | undefined

const CancelPendingEmailChange = ({ userId, onSuccessRedirect: redirectOnSuccess }: Props) => {
  const boundAction = cancelPendingEmailChangeAction
    .bind(null, redirectOnSuccess)
    .bind(null, userId)
  const [state, action, isPending] = useActionState<State, FormData>(
    async () => boundAction(),
    undefined,
  )

  return (
    <form
      action={action}
      onSubmit={(e) => {
        e.preventDefault()
        startTransition(() => {
          action(new FormData())
        })
      }}
    >
      {state?.errors?.length ? <ErrorState>{state.errors[0]}</ErrorState> : null}
      <Button type="submit" variant="secondary" disabled={isPending}>
        Cancel email change
      </Button>
    </form>
  )
}

export default CancelPendingEmailChange
