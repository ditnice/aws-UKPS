'use client'

import { revalidateLogic, useForm } from '@tanstack/react-form'
import Link from 'next/link'
import { QRCodeSVG } from 'qrcode.react'
import { z } from 'zod'

import { Button } from '@nice-digital/nds-button'

import { getFieldErrorMessage } from '@/app/common/form/getFieldErrorMessage'
import { Input } from '@/components/Input/Input'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import styles from './page.module.scss'

import type { ChangeEvent } from 'react'

const otpAuthUri =
  'otpauth://totp/NICE%20UKPS:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=NICE%20UKPS&algorithm=SHA1&digits=6&period=30'

const manualSetupKey = new URL(otpAuthUri).searchParams.get('secret') ?? ''

function normaliseSecurityCode(value: string) {
  return value.replace(/[\s-]/g, '')
}

const signUpSetMfaSchema = z.object({
  securityCode: z
    .string()
    .trim()
    .min(1, 'Enter your security code')
    .refine(
      (value) => /^\d{6}$/.test(normaliseSecurityCode(value)),
      'Enter a 6-digit security code',
    ),
})

type SignUpSetMfaFormValues = z.input<typeof signUpSetMfaSchema>

export default function SignUpSetMfa() {
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
    onSubmit: ({ value }) => {
      const { securityCode } = signUpSetMfaSchema.parse(value)
      console.log(normaliseSecurityCode(securityCode))
      // MFA setup verification will be wired once the submit target is confirmed.
    },
  })

  return (
    <>
      <PageHeader heading="Set up two-factor authentication"></PageHeader>
      <p>
        To help protect your account, we use two-factor authentication. To enable two-factor
        authentication scan the QR code below using your chosen authenticator app.
      </p>
      <QRCodeSVG
        aria-label="QR code for authenticator app setup"
        value={otpAuthUri}
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

        <div className={styles.actions}>
          <Button type="submit" variant="cta">
            Continue
          </Button>
        </div>
      </form>
    </>
  )
}
