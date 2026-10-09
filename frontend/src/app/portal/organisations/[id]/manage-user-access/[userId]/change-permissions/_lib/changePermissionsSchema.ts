import { z } from 'zod'

import { positiveIdSchema } from '@/lib/validation/positiveId'

// Super users are managed through a separate flow.
export const changePermissionsSchema = z.object({
  organisationId: positiveIdSchema,
  userId: positiveIdSchema,
  membershipId: positiveIdSchema,
  userRole: z.enum(['Standard', 'Champion']),
})
