'use client'

import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { useRouter } from 'next/navigation'
import { useState } from 'react'

import { initiateUpdateSmsMultiFactorAuthentication } from '@/client/generated/sdk.gen'
import { BackLink } from '@/components/BackLink/BackLink'
import { Button } from '@/components/Button/Button'
import { Details } from '@/components/Details/Details'
import { Input } from '@/components/Input/Input'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { errorMessages } from '@/lib/form/errorMessages'

export default function RequestNewCode() {
  const router = useRouter()
  const [phoneNumber, setPhoneNumber] = useState(() => {
    try {
      const stored = sessionStorage.getItem('changeSmsMfa')
      return stored ? (JSON.parse(stored).phoneNumber ?? '') : ''
    } catch {
      return ''
    }
  })
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function requestNewCode() {
    if (!phoneNumber.trim() || !isValidPhoneNumber(phoneNumber.trim(), 'GB')) {
      setError(errorMessages.phoneFormat)
      return
    }

    setError('')
    setIsSubmitting(true)

    try {
      const result = await initiateUpdateSmsMultiFactorAuthentication({
        body: { phoneNumber: phoneNumber.trim() },
        credentials: 'include',
      })

      if (result.error) {
        if (result.response?.status === 400) {
          setError(errorMessages.phoneFormat)
          return
        }

        setError(
          result.error.detail ?? 'We could not request a new security code. Try again later.',
        )
        return
      }

      try {
        sessionStorage.setItem('changeSmsMfa', JSON.stringify({ phoneNumber: phoneNumber.trim() }))
      } catch {}

      router.push('/portal/user/me/update-sms-mfa/check-your-phone')
    } catch {
      setError('We could not request a new security code. Try again later.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <PageHeader
        backLink={<BackLink href="/portal/user/me/update-sms-mfa/check-your-phone">Back</BackLink>}
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
          onChange={(event) => setPhoneNumber(event.target.value)}
          error={Boolean(error)}
          errorMessage={error}
        />
      </Details>
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
