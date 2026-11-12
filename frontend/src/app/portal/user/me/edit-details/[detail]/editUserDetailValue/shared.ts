import { formOptions } from '@tanstack/react-form'
import { createServerValidate } from '@tanstack/react-form-nextjs'
import { isValidPhoneNumber } from 'libphonenumber-js/max'
import z from 'zod'

import { EditableUserDetail, UpdateUserDetailsCommand } from '@/client/generated'
import { errorMessages } from '@/lib/form/errorMessages'

export const formSchema = z.discriminatedUnion('property', [
  z.object({
    property: z.literal(EditableUserDetail.FULL_NAME),
    value: z
      .string()
      .trim()
      .min(1, errorMessages.personalFullNameRequired)
      .max(255, 'Value is too long.'),
  }),
  z.object({
    property: z.literal(EditableUserDetail.WORK_PHONE),
    value: z
      .string()
      .trim()
      .min(1, errorMessages.personalPhoneRequired)
      .refine((value) => isValidPhoneNumber(value, 'GB'), errorMessages.phoneFormat),
  }),
])

const defaultValues: UpdateUserDetailsCommand = {
  property: 'FullName',
  value: '',
}

export const updateUserDetailOptions = formOptions({
  defaultValues: defaultValues,
  validators: {
    onDynamic: formSchema,
  },
})

export const serverValidate = createServerValidate({
  ...updateUserDetailOptions,
  onServerValidate: formSchema,
})
