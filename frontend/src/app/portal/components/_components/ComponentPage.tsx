import Link from 'next/link'

import { PageHeader } from '@/components/PageHeader/PageHeader'
import { Tag } from '@/components/Tag/Tag'

import styles from '../page.module.scss'

import type { ReactNode } from 'react'

type ComponentPageMarker = 'custom' | 'wrapper'

type ComponentPageProps = {
  children: ReactNode
  marker?: ComponentPageMarker
  title: string
}

export function ComponentPage({ children, marker, title }: ComponentPageProps) {
  const preheading = marker ? (
    <>
      <Tag>{marker === 'custom' ? 'Custom' : 'Wrapper'}</Tag> UKPS component
    </>
  ) : (
    'NICE Design System component'
  )

  return (
    <>
      <Link href="/portal/components" prefetch={false}>
        Back to components
      </Link>
      <hr></hr>
      <PageHeader heading={title} preheading={preheading} />
      <div className={styles['example-list']}>{children}</div>
    </>
  )
}
