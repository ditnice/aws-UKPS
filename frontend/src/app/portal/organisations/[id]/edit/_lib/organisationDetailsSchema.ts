import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { z } from 'zod'

import { errorMessages } from '@/lib/form/errorMessages'

export const editOrganisationDetailsSchema = z.object({
  organisationName: z.string().trim().min(1, errorMessages.organisationNameRequired),
  headOfficeAddress: z.string().trim().min(1, errorMessages.addressRequired),
  headOfficeEmail: z
    .string()
    .trim()
    .min(1, errorMessages.organisationEmailRequired)
    .pipe(z.email(errorMessages.emailFormat)),
  headOfficeTelephone: z
    .string()
    .trim()
    .min(1, errorMessages.phoneRequired)
    .refine((value) => isValidPhoneNumber(value, 'GB'), errorMessages.phoneFormat),
})

export type EditOrganisationDetailsFormValues = z.input<typeof editOrganisationDetailsSchema>
