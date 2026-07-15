import 'server-only'

import type { CreateClientConfig } from './generated/client.gen'

export const createClientConfig: CreateClientConfig = (config) => ({
  ...config,
  baseUrl: process.env.BACKEND_API_BASE_URL ?? 'http://localhost:5016',
})
