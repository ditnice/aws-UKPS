import type { User } from '@/payload-types'

import type { AccessArgs, PayloadRequest } from 'payload'

export const payloadUser: User = {
  id: 1,
  email: 'editor@example.com',
  collection: 'users',
  createdAt: '2026-01-01T12:00:00Z',
  updatedAt: '2026-01-01T12:00:00Z',
}

export function accessArgs(user: User | null | undefined): AccessArgs {
  // Access-unit tests exercise req.user only, without creating a Payload runtime or database.
  return { req: { user } as PayloadRequest }
}
