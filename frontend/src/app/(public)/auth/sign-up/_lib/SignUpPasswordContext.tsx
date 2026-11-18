'use client'

import { createContext, useCallback, useContext, useState } from 'react'

import type { ReactNode } from 'react'

type SignUpPasswordContextValue = {
  clearPassword: () => void
  password: null | string
  setPassword: (password: string) => void
}

const SignUpPasswordContext = createContext<SignUpPasswordContextValue | undefined>(undefined)

export function SignUpPasswordProvider({
  children,
  initialPassword = null,
}: {
  children: ReactNode
  initialPassword?: null | string
}) {
  const [password, setPasswordState] = useState<null | string>(initialPassword)
  const setPassword = useCallback((newPassword: string) => setPasswordState(newPassword), [])
  const clearPassword = useCallback(() => setPasswordState(null), [])

  return (
    <SignUpPasswordContext.Provider value={{ clearPassword, password, setPassword }}>
      {children}
    </SignUpPasswordContext.Provider>
  )
}

export function useSignUpPassword() {
  const context = useContext(SignUpPasswordContext)
  if (!context) {
    throw new Error('useSignUpPassword must be used within SignUpPasswordProvider')
  }
  return context
}
