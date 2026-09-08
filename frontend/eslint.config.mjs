import coreWebVitals from 'eslint-config-next/core-web-vitals'
import typescript from 'eslint-config-next/typescript'

// Local wrappers around NDS components that everything else should import
// instead of the raw @nice-digital package. Each entry needs a matching
// `no-restricted-imports` "off ramp" for its own directory below, since a
// wrapper has to import the package it wraps.
const ndsWrappers = [
  { dir: 'Table', pkg: '@nice-digital/nds-table', component: 'Table' },
  { dir: 'Button', pkg: '@nice-digital/nds-button', component: 'Button' },
  { dir: 'Input', pkg: '@nice-digital/nds-input', component: 'Input' },
  { dir: 'Textarea', pkg: '@nice-digital/nds-textarea', component: 'Textarea' },
  { dir: 'Tag', pkg: '@nice-digital/nds-tag', component: 'Tag' },
  { dir: 'PageHeader', pkg: '@nice-digital/nds-page-header', component: 'PageHeader' },
  { dir: 'Alert', pkg: '@nice-digital/nds-alert', component: 'Alert' },
]

const restrictedImportPaths = ndsWrappers.map(({ pkg, dir, component }) => ({
  name: pkg,
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
  ...coreWebVitals,
  ...typescript,
  {
    settings: {
      'import/parsers': {
        '@typescript-eslint/parser': ['.ts', '.tsx'],
      },
      'import/resolver': {
        typescript: {
          project: ['./tsconfig.json'],
        },
      },
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
      'import/first': 'error',
      'import/newline-after-import': 'error',
      'import/no-unresolved': 'error',
      'import/order': [
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
