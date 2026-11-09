import { createServerValidate, formOptions } from '@tanstack/react-form-nextjs'
import z from 'zod'

import { ConfirmNewPasswordCommand } from '@/client/generated'

export const formSchema = z.object({
  email: z.email(),
  confirmationCode: z.string().length(6, 'Confirmation code must be 6 characters long'),
  newPassword: z.string().min(8, 'Password must be at least 8 characters long').max(256),
})

export const confirmPasswordResetFormOptions = formOptions({
  defaultValues: {
    email: '',
    confirmationCode: '',
    newPassword: '',
  } satisfies ConfirmNewPasswordCommand,
  validators: {
    onDynamic: formSchema,
  },
})

export const serverValidate = createServerValidate({
  ...confirmPasswordResetFormOptions,
  onServerValidate: formSchema,
})
