import { defineConfig } from '@hey-api/openapi-ts'

export default defineConfig({
  input: '../backend/openapi/UKPS.Api.json',
  output: 'src/client/generated',
  plugins: [
    {
      name: '@hey-api/client-next',
      runtimeConfigPath: './src/client/hey-api',
    },
    '@hey-api/sdk',
    { name: '@hey-api/typescript', enums: 'javascript' },
    { name: '@faker-js/faker', locale: 'en_GB' },
  ],
})
