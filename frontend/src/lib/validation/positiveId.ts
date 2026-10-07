import { z } from 'zod'

export const positiveIdSchema = z.number().int().positive()
