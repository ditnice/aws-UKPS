import clsx from 'clsx'

import {
  PageHeader as NdsPageHeader,
  type PageHeaderProps as NdsPageHeaderProps,
} from '@nice-digital/nds-page-header'

import styles from './PageHeader.module.scss'

import type { ReactNode } from 'react'

export interface PageHeaderProps extends NdsPageHeaderProps {
  backLink?: ReactNode
  className?: string
}

export function PageHeader({ backLink, breadcrumbs, className, ...props }: PageHeaderProps) {
  const pageHeaderProps: NdsPageHeaderProps = {
    ...props,
    breadcrumbs: backLink ?? breadcrumbs,
  }

  return (
    <div className={clsx(styles['page-header-wrapper'], className)}>
      <NdsPageHeader {...pageHeaderProps} />
    </div>
  )
}
