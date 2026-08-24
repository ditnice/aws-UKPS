'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { QRCodeSVG } from 'qrcode.react'
import { useState, type ChangeEvent } from 'react'
import { z } from 'zod'

import { postAuthVerifyMfa } from '@/client/generated/sdk.gen'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { routeOnSuccessfulAuth } from '@/lib/auth/routing'
import {
  SECURITY_CODE_FORMAT_ERROR_MESSAGE,
  SECURITY_CODE_REQUIRED_ERROR_MESSAGE,
} from '@/lib/form/errorMessages'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import { signUpMfaSetupStorageKey } from '../_lib/mfaSetupStorage'

import styles from './page.module.scss'

function normaliseSecurityCode(value: string) {
  return value.replace(/[\s-]/g, '')
}

const signUpMfaSetupSchema = z.object({
  authenticationSession: z.string().min(1),
  otpAuthUri: z.string().min(1),
  setupToken: z.string().min(1),
})

const signUpSetMfaSchema = z.object({
  securityCode: z
    .string()
    .trim()
    .min(1, SECURITY_CODE_REQUIRED_ERROR_MESSAGE)
    .refine(
      (value) => /^\d{6}$/.test(normaliseSecurityCode(value)),
      SECURITY_CODE_FORMAT_ERROR_MESSAGE,
    ),
})

type SignUpSetMfaFormValues = z.input<typeof signUpSetMfaSchema>
type SignUpMfaSetup = z.infer<typeof signUpMfaSetupSchema>
type SetupState = { setup: SignUpMfaSetup; status: 'ready' } | { status: 'error' }

function loadSignUpMfaSetup(): SetupState {
  let storedSetup: string | null

  try {
    storedSetup = sessionStorage.getItem(signUpMfaSetupStorageKey)
  } catch {
    return { status: 'error' }
  }

  if (!storedSetup) {
    return { status: 'error' }
  }

  try {
    const parsedSetup = signUpMfaSetupSchema.parse(JSON.parse(storedSetup))
    new URL(parsedSetup.otpAuthUri)

    return { setup: parsedSetup, status: 'ready' }
  } catch {
    return { status: 'error' }
  }
}

export default function SignUpSetMfa() {
  const router = useRouter()
  const [setupState, setSetupState] = useState<SetupState>(loadSignUpMfaSetup)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const form = useForm({
    defaultValues: {
      securityCode: '',
    } satisfies SignUpSetMfaFormValues,
    validationLogic: revalidateLogic({
      mode: 'submit',
      modeAfterSubmission: 'blur',
    }),
    validators: {
      onDynamic: signUpSetMfaSchema,
    },
    onSubmit: async ({ value, formApi }) => {
      if (setupState.status !== 'ready') {
        setSetupState({ status: 'error' })
        return
      }

      const { securityCode } = signUpSetMfaSchema.parse(value)
      const { setup } = setupState
      setSubmitError(null)

      try {
        const result = await postAuthVerifyMfa({
          body: {
            authenticationSession: setup.authenticationSession,
            code: normaliseSecurityCode(securityCode),
            setupToken: setup.setupToken,
          },
          credentials: 'include',
        })

        if (!result.error) {
          try {
            sessionStorage.removeItem(signUpMfaSetupStorageKey)
          } catch {}

          router.push(routeOnSuccessfulAuth)
          return
        }

        if (result.response?.status === 400) {
          formApi.setErrorMap({
            onSubmit: {
              fields: {
                securityCode: 'Invalid authentication code.',
              },
            },
          })
          return
        }

        setSubmitError(
          result.error.detail ?? 'We could not verify your authentication code. Try again later.',
        )
      } catch {
        setSubmitError('We could not verify your authentication code. Try again later.')
      }
    },
  })

  if (setupState.status === 'error') {
    return (
      <>
        <PageHeader heading="There is a problem setting up two-factor authentication"></PageHeader>
        <p>
          We could not find your multi-factor authentication setup details. Return to your sign-up
          link and try again.
        </p>
      </>
    )
  }

  const { setup } = setupState
  const manualSetupKey = new URL(setup.otpAuthUri).searchParams.get('secret') ?? ''

  return (
    <>
      <PageHeader heading="Set up two-factor authentication"></PageHeader>
      <p>
        To help protect your account, we use two-factor authentication. To enable two-factor
        authentication scan the QR code below using your chosen authenticator app.
      </p>
      <QRCodeSVG
        aria-label="QR code for authenticator app setup"
        value={setup.otpAuthUri}
        size={192}
        level="M"
      />
      <p>
        If you cannot scan the QR code, enter this set up key in your authenticator app manually:
      </p>
      <p>
        <strong>{manualSetupKey}</strong>
      </p>
      <hr></hr>
      <form
        noValidate
        onSubmit={(event) => {
          event.preventDefault()
          event.stopPropagation()
          void form.handleSubmit()
        }}
      >
        <form.Field name="securityCode">
          {(field) => {
            const errorMessage = getFieldErrorMessage(field.state.meta.errors)

            return (
              <Input
                autoComplete="one-time-code"
                error={Boolean(errorMessage)}
                errorMessage={errorMessage}
                hint="Enter the 6-digit authentication code shown in your authenticator app."
                inputMode="numeric"
                label="Authentication code"
                name={field.name}
                onBlur={field.handleBlur}
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  field.handleChange(event.target.value)
                }
                type="text"
                value={field.state.value}
                width="one-quarter"
              />
            )
          }}
        </form.Field>

        <Link href="/">I want to use a different method</Link>

        {submitError ? <p>{submitError}</p> : null}

        <div className={styles.actions}>
          <Button type="submit" variant="cta">
            Continue
          </Button>
        </div>
      </form>
    </>
  )
}
