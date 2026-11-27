import { formOptions, revalidateLogic } from '@tanstack/react-form'
import { createServerValidate } from '@tanstack/react-form-nextjs'
import z from 'zod'

import { InitiateForgottenPasswordProcedureCommand } from '@/client/generated'

export const resetEmailCookieName = 'reset-email'

export const formSchema = z.object({
  email: z.email('Enter a valid email address'),
})

export const passwordRestFormOptions = formOptions({
  defaultValues: {
    email: '',
  } satisfies InitiateForgottenPasswordProcedureCommand,
  validationLogic: revalidateLogic({
    mode: 'submit',
    modeAfterSubmission: 'blur',
  }),
  validators: {
    onDynamic: formSchema,
  },
})

export const serverValidate = createServerValidate({
  ...passwordRestFormOptions,
  onServerValidate: formSchema,
})
