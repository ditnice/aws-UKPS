'use client'

import { Container } from '@nice-digital/nds-container'

import styles from './ApplicationLayout.module.scss'
import { Footer } from './Footer/Footer'
import { Header } from './Header/Header'
import { Main } from './Main/Main'

import type { UserOrganisationContext } from './Header/Header'
import type { ReactNode } from 'react'

export type ApplicationLayoutProps = {
  children: ReactNode
  organisationContext?: UserOrganisationContext
}

export function ApplicationLayout({ children, organisationContext }: ApplicationLayoutProps) {
  return (
    <>
      <div className={styles.contentWrapper}>
        <Header skipLinkId="content-start" organisationContext={organisationContext} />
        <Main className={styles.mainContent} id="content-start">
          <Container>{children}</Container>
        </Main>
        <Footer />
      </div>
    </>
  )
}
