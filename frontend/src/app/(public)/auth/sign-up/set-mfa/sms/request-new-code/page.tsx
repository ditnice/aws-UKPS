'use client'

import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { setupSmsMfa } from '@/client/generated/sdk.gen'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { Details } from '@/components/Details/Details'
import { Input } from '@/components/Input/Input'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { PasswordInput } from '@/components/PasswordInput/PasswordInput'
import { errorMessages } from '@/lib/form/errorMessages'

import { useMfaSetupStore } from '../../../_lib/mfaSetupStorage'
import { useSmsMfaSetupStore } from '../../../_lib/mfaSmsSetupStorage'
import { useSignUpPassword } from '../../../_lib/SignUpPasswordContext'

export default function RequestNewCode() {
  const router = useRouter()
  const mfaSetup = useMfaSetupStore()
  const smsMfaSetup = useSmsMfaSetupStore()
  const { clearPassword, password, setPassword } = useSignUpPassword()

  const setupToken = mfaSetup.status === 'ready' ? (mfaSetup.data.setupToken ?? '') : ''
  const storedPhoneNumber = smsMfaSetup.status === 'ready' ? smsMfaSetup.data.phoneNumber : ''

  // null until the user edits the field, so it shows the stored number once the store has loaded.
  const [phoneNumberInput, setPhoneNumberInput] = useState<string | null>(null)
  const phoneNumber = phoneNumberInput ?? storedPhoneNumber

  const [enteredPassword, setEnteredPassword] = useState('')
  const [error, setError] = useState('')
  const [passwordError, setPasswordError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function requestNewCode() {
    if (!phoneNumber.trim() || !isValidPhoneNumber(phoneNumber.trim(), 'GB')) {
      setError(errorMessages.phoneFormat)
      return
    }

    if (!setupToken) {
      setError(
        'We could not find your multi-factor authentication setup details. Return to your sign-up link and try again.',
      )
      return
    }

    const submittedPassword = password ?? enteredPassword
    if (!submittedPassword) {
      setPasswordError(errorMessages.passwordRequired)
      return
    }

    setError('')
    setPasswordError('')
    setIsSubmitting(true)

    try {
      const result = await setupSmsMfa({
        body: { setupToken, phoneNumber: phoneNumber.trim(), password: submittedPassword },
        credentials: 'include',
      })

      if (result.error) {
        if (result.response?.status === 401) {
          clearPassword()
          setEnteredPassword('')
          setPasswordError('The password you entered is incorrect.')
          return
        }

        setError(
          result.error.detail ?? 'We could not request a new security code. Try again later.',
        )
        return
      }

      smsMfaSetup.set({
        phoneNumber: phoneNumber.trim(),
        authenticationSession: result.data.authenticationSession,
      })

      setPassword(submittedPassword)
      router.push('/auth/sign-up/set-mfa/sms/check-your-phone')
    } catch {
      setError('We could not request a new security code. Try again later.')
    } finally {
      setIsSubmitting(false)
    }
  }

  if (mfaSetup.status === 'loading' || smsMfaSetup.status === 'loading') {
    return null
  }

  return (
    <>
      <PageHeader
        backLink={<BackLink href="/auth/sign-up/set-mfa/sms/check-your-phone">Back</BackLink>}
        heading="Request a new security code"
      />
      <p>
        Text messages sometimes take a few minutes to arrive. If you do not receive the text
        message, you can request a new one.
      </p>
      <Details summary="Change where the text message is sent">
        <Input
          name="phoneNumber"
          label="Mobile number"
          width="one-third"
          value={phoneNumber}
          onChange={(event) => setPhoneNumberInput(event.target.value)}
          error={Boolean(error)}
          errorMessage={error}
        />
      </Details>
      {password === null ? (
        <PasswordInput
          name="password"
          label="Password"
          width="one-third"
          value={enteredPassword}
          onChange={(event) => setEnteredPassword(event.target.value)}
          error={Boolean(passwordError)}
          errorMessage={passwordError}
        />
      ) : null}
      <Button
        variant="cta"
        buttonType="submit"
        disabled={isSubmitting}
        onClick={() => void requestNewCode()}
      >
        Request a new code
      </Button>
    </>
  )
}
