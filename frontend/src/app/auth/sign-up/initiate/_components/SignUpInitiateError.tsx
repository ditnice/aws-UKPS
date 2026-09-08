import { PageHeader } from '@/components/PageHeader/PageHeader'

import type { ReactNode } from 'react'

type SignUpInitiateErrorProps = {
  detail: ReactNode
  title?: string
}

export function SignUpInitiateError({
  detail,
  title = 'There is a problem with your sign-up link',
}: SignUpInitiateErrorProps) {
  return (
    <>
      <PageHeader heading={title}></PageHeader>
      <p>{detail}</p>
    </>
  )
}
