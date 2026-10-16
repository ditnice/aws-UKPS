'use client'

import { Container } from '@nice-digital/nds-container'

import styles from './ApplicationLayout.module.scss'
import { Footer } from './Footer/Footer'
import { Header } from './Header/Header'
import { Main } from './Main/Main'

import type { CurrentOrganisation } from './Header/Header'
import type { ReactNode } from 'react'

export type ApplicationLayoutProps = {
  children: ReactNode
  currentOrganisation?: CurrentOrganisation
}

export function ApplicationLayout({ children, currentOrganisation }: ApplicationLayoutProps) {
  return (
    <>
      <div className={styles.contentWrapper}>
        <Header skipLinkId="content-start" currentOrganisation={currentOrganisation} />
        <Main className={styles.mainContent} id="content-start">
          <Container>{children}</Container>
        </Main>
        <Footer />
      </div>
    </>
  )
}
