'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { QRCodeSVG } from 'qrcode.react'
import { ChangeEvent, useState } from 'react'
import z from 'zod'

import { postAuthVerifyMfa, verifyMultiFactorAuthenticationUpdate } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { Input } from '@/components/Input/Input'
import { errorMessages } from '@/lib/form/errorMessages'
import { getFieldErrorMessage } from '@/lib/form/getFieldErrorMessage'

import styles from './SetupMfaForm.module.scss'

function normaliseSecurityCode(value: string) {
  return value.replace(/[\s-]/g, '')
}

const signUpSetMfaSchema = z.object({
  securityCode: z
    .string()
    .trim()
    .min(1, errorMessages.securityCodeRequired)
    .refine(
      (value) => /^\d{6}$/.test(normaliseSecurityCode(value)),
      errorMessages.securityCodeFormat,
    ),
})

export const signUpMfaSetupSchema = z.object({
  authenticationSession: z.string().min(1),
  otpAuthUri: z.string().min(1),
  setupToken: z.string().min(1),
})

type CommonMfaSetupParameters = {
  otpAuthUri: string
}
type MfaSetupParameters = (
  { type: 'update' } | { type: 'initialise'; setupToken: string; authenticationSession: string }
) &
  CommonMfaSetupParameters

type SignUpSetMfaFormValues = z.input<typeof signUpSetMfaSchema>
export type SignUpMfaSetup = z.infer<typeof signUpMfaSetupSchema>
type Setup2FAFormProps = {
  setup: MfaSetupParameters
  hideAlternativeOptions?: boolean
  onSuccess?: () => void
}
const SetupMfaForm = ({ setup, hideAlternativeOptions, onSuccess }: Setup2FAFormProps) => {
  const [submitError, setSubmitError] = useState<string | null>(null)
  const sendRequest = async (securityCode: string) => {
    const normalisedSecurityToken = normaliseSecurityCode(securityCode)
    if (setup.type === 'initialise') {
      return await postAuthVerifyMfa({
        body: {
          authenticationSession: setup.authenticationSession,
          code: normalisedSecurityToken,
          setupToken: setup.setupToken,
        },
        credentials: 'include',
      })
    }
    return await verifyMultiFactorAuthenticationUpdate({
      body: { code: normalisedSecurityToken },
      credentials: 'include',
    })
  }
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
      const { securityCode } = signUpSetMfaSchema.parse(value)
      setSubmitError(null)

      try {
        const result = await sendRequest(securityCode)

        if (!result.error) {
          if (onSuccess) onSuccess()
          return
        }

        if (result.response?.status === 400) {
          formApi.setErrorMap({
            onSubmit: {
              fields: {
                securityCode: errorMessages.incorrectMfaCode,
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
  const manualSetupKey = new URL(setup.otpAuthUri).searchParams.get('secret') ?? ''
  return (
    <>
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
                label="Enter your authentication code"
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

        {hideAlternativeOptions ? <></> : <Link href="/">I want to use a different method</Link>}

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

export default SetupMfaForm
