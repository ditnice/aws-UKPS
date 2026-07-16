import path from 'path'
import { fileURLToPath } from 'url'

import { withPayload } from '@payloadcms/next/withPayload'

import type { NextConfig } from 'next'

const __filename = fileURLToPath(import.meta.url)
const dirname = path.dirname(__filename)

const niceDesignSystemPackages = [
  '@nice-digital/design-system',
  '@nice-digital/nds-a-z-list',
  '@nice-digital/nds-accordion',
  '@nice-digital/nds-action-banner',
  '@nice-digital/nds-alert',
  '@nice-digital/nds-alphabet',
  '@nice-digital/nds-breadcrumbs',
  '@nice-digital/nds-button',
  '@nice-digital/nds-card',
  '@nice-digital/nds-checkbox',
  '@nice-digital/nds-column-list',
  '@nice-digital/nds-container',
  '@nice-digital/nds-core',
  '@nice-digital/nds-enhanced-pagination',
  '@nice-digital/nds-filters',
  '@nice-digital/nds-form-group',
  '@nice-digital/nds-full-bleed',
  '@nice-digital/nds-grid',
  '@nice-digital/nds-horizontal-nav',
  '@nice-digital/nds-input',
  '@nice-digital/nds-page-header',
  '@nice-digital/nds-panel',
  '@nice-digital/nds-phase-banner',
  '@nice-digital/nds-prev-next',
  '@nice-digital/nds-radio',
  '@nice-digital/nds-simple-pagination',
  '@nice-digital/nds-stacked-nav',
  '@nice-digital/nds-table',
  '@nice-digital/nds-tabs',
  '@nice-digital/nds-tag',
  '@nice-digital/nds-textarea',
]

const nextConfig: NextConfig = {
  images: {
    localPatterns: [
      {
        pathname: '/api/media/file/**',
      },
    ],
  },
  webpack: (webpackConfig) => {
    webpackConfig.resolve.extensionAlias = {
      '.cjs': ['.cts', '.cjs'],
      '.js': ['.ts', '.tsx', '.js', '.jsx'],
      '.mjs': ['.mts', '.mjs'],
    }

    return webpackConfig
  },
  turbopack: {
    root: path.resolve(dirname),
  },
  transpilePackages: niceDesignSystemPackages,
}

export default withPayload(nextConfig, { devBundleServerPackages: false })
