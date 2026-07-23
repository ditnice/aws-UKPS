import Link from 'next/link'

import { PageHeader } from '@nice-digital/nds-page-header'

import { Tag } from '@/components/Tag/Tag'

import styles from '../page.module.scss'

import type { ReactNode } from 'react'

type ComponentPageProps = {
  children: ReactNode
  custom?: boolean
  title: string
}

export function ComponentPage({ children, custom = false, title }: ComponentPageProps) {
  return (
    <>
      <Link href="/portal/components" prefetch={false}>
        Back to components
      </Link>
      <hr></hr>
      <PageHeader
        heading={title}
        preheading={
          custom ? (
            <>
              <Tag>Custom</Tag> UKPS component
            </>
          ) : (
            'NICE Design System component'
          )
        }
      />
      <div className={styles['example-list']}>{children}</div>
    </>
  )
}
