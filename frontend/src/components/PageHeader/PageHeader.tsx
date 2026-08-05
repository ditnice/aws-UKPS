import clsx from 'clsx'

import {
  PageHeader as NdsPageHeader,
  type PageHeaderProps as NdsPageHeaderProps,
} from '@nice-digital/nds-page-header'

import './PageHeader.scss'

import type { ReactNode } from 'react'

type PageHeaderVerticalPadding = NdsPageHeaderProps['verticalPadding'] | 'top-only'

export interface PageHeaderProps extends Omit<
  NdsPageHeaderProps,
  'breadcrumbs' | 'className' | 'verticalPadding'
> {
  backLink?: ReactNode
  breadcrumbs?: ReactNode
  className?: string
  verticalPadding?: PageHeaderVerticalPadding
}

export function PageHeader({ backLink, className, verticalPadding, ...props }: PageHeaderProps) {
  const isTopOnly = verticalPadding === 'top-only'
  const wrapperClassName = clsx(className, isTopOnly && 'page-header--vertical-padding-top-only')

  return (
    <div className={wrapperClassName || undefined}>
      <NdsPageHeader
        {...(props as NdsPageHeaderProps)}
        breadcrumbs={backLink ?? props.breadcrumbs}
        verticalPadding={isTopOnly ? undefined : verticalPadding}
      />
    </div>
  )
}
