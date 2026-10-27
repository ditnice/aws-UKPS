import { fileURLToPath } from 'node:url'

import { includeIgnoreFile } from '@eslint/config-helpers'
import coreWebVitals from 'eslint-config-next/core-web-vitals'
import typescript from 'eslint-config-next/typescript'
import { createTypeScriptImportResolver } from 'eslint-import-resolver-typescript'
import importX from 'eslint-plugin-import-x'

const gitignorePath = fileURLToPath(new URL('../.gitignore', import.meta.url))

// Local wrappers around NDS components that everything else should import
// instead of the raw @nice-digital package. Each entry needs a matching
// `no-restricted-imports` "off ramp" for its own directory below, since a
// wrapper has to import the package it wraps. `componentOnly` restricts just
// the wrapped component, for packages whose other exports are still used.
const ndsWrappers = [
  { dir: 'Table', pkg: '@nice-digital/nds-table', component: 'Table' },
  { dir: 'Button', pkg: '@nice-digital/nds-button', component: 'Button' },
  { dir: 'Input', pkg: '@nice-digital/nds-input', component: 'Input' },
  { dir: 'Textarea', pkg: '@nice-digital/nds-textarea', component: 'Textarea' },
  { dir: 'Tag', pkg: '@nice-digital/nds-tag', component: 'Tag' },
  { dir: 'PageHeader', pkg: '@nice-digital/nds-page-header', component: 'PageHeader' },
  { dir: 'Alert', pkg: '@nice-digital/nds-alert', component: 'Alert' },
  { dir: 'ActionBanner', pkg: '@nice-digital/nds-action-banner', component: 'ActionBanner' },
  {
    dir: 'FilterByInput',
    pkg: '@nice-digital/nds-filters',
    component: 'FilterByInput',
    componentOnly: true,
  },
]

const restrictedImportPaths = ndsWrappers.map(({ pkg, dir, component, componentOnly }) => ({
  name: pkg,
  ...(componentOnly && { importNames: [component] }),
  message: `Use the local wrapper @/components/${dir}/${component} instead.`,
}))

const ndsWrapperRestrictions = [
  {
    files: ['**/*.{ts,tsx}'],
    rules: {
      'no-restricted-imports': ['error', { paths: restrictedImportPaths }],
    },
  },
  ...ndsWrappers.map(({ dir, pkg }) => ({
    files: [`src/components/${dir}/**/*.{ts,tsx}`],
    rules: {
      'no-restricted-imports': [
        'error',
        { paths: restrictedImportPaths.filter((path) => path.name !== pkg) },
      ],
    },
  })),
]

const eslintConfig = [
  includeIgnoreFile(gitignorePath, { gitignoreResolution: true }),
  ...coreWebVitals,
  ...typescript,
  {
    // eslint-plugin-import's `order` rule crashes on ESLint 10 (it calls removed SourceCode
    // APIs), so our import rules use the maintained import-x fork instead.
    plugins: {
      'import-x': importX,
    },
    settings: {
      'import-x/resolver-next': [createTypeScriptImportResolver({ project: './tsconfig.json' })],
    },
    rules: {
      '@typescript-eslint/ban-ts-comment': 'warn',
      '@typescript-eslint/no-empty-object-type': 'warn',
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/no-unused-vars': [
        'warn',
        {
          vars: 'all',
          args: 'after-used',
          ignoreRestSiblings: false,
          argsIgnorePattern: '^_',
          varsIgnorePattern: '^_',
          destructuredArrayIgnorePattern: '^_',
          caughtErrorsIgnorePattern: '^(_|ignore)',
        },
      ],
      'import-x/first': 'error',
      'import-x/newline-after-import': 'error',
      'import-x/no-unresolved': 'error',
      'import-x/order': [
        'error',
        {
          'newlines-between': 'always',
          groups: [
            'builtin',
            'external',
            'internal',
            'unknown',
            'parent',
            'sibling',
            'index',
            'object',
            'type',
          ],
          alphabetize: { order: 'asc', caseInsensitive: true },
          pathGroupsExcludedImportTypes: ['builtin'],
          warnOnUnassignedImports: true,
          pathGroups: [
            {
              pattern: '@nice-digital/**',
              group: 'external',
              position: 'after',
            },
            {
              pattern: '@/**',
              group: 'internal',
              position: 'before',
            },
          ],
        },
      ],
    },
  },
  ...ndsWrapperRestrictions,
  {
    ignores: [
      '.next/',
      'next-env.d.ts',
      'src/payload-types.ts',
      'src/payload-generated-schema.ts',
      'src/client/generated/',
      'src/app/(payload)/',
    ],
  },
]

export default eslintConfig
