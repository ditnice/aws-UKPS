import { z } from 'zod'

const requestGuidSchema = z.guid()

export function isRequestGuid(value: string | undefined): value is string {
  return requestGuidSchema.safeParse(value).success
}
