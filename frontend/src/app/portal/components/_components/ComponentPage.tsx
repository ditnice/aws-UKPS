import Link from 'next/link'

import { Container } from '@nice-digital/nds-container'
import { PageHeader } from '@nice-digital/nds-page-header'

import styles from '../page.module.scss'

import type { ReactNode } from 'react'

type ComponentPageProps = {
  children: ReactNode
  title: string
}

export function ComponentPage({ children, title }: ComponentPageProps) {
  return (
    <Container>
      <Link href="/portal/components" prefetch={false}>
        Back to components
      </Link>
      <hr></hr>
      <PageHeader heading={title} preheading="NICE Design System component" />
      <div className={styles.exampleList}>{children}</div>
    </Container>
  )
}
