import { SignUpPasswordProvider } from './_lib/SignUpPasswordContext'

import type { ReactNode } from 'react'

export default function SignUpLayout({ children }: { children: ReactNode }) {
  return <SignUpPasswordProvider>{children}</SignUpPasswordProvider>
}
