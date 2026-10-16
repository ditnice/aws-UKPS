import nextEnv from '@next/env'
import { build } from 'esbuild'

// Mirror Next's production .env loading when pnpm build is used outside Docker.
nextEnv.loadEnvConfig(process.cwd())

await build({
  entryPoints: ['scripts/start.ts'],
  outfile: 'build/start.mjs',
  bundle: true,
  platform: 'node',
  target: 'node24',
  format: 'esm',
  // Match Next's public build-time value; ECS runtime overrides cannot change it.
  define: {
    'process.env.NEXT_PUBLIC_QA_SUPPORT_EMAIL':
      JSON.stringify(process.env.NEXT_PUBLIC_QA_SUPPORT_EMAIL) ?? 'undefined',
  },
})
