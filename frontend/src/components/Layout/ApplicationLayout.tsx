'use client'

import { Container } from '@nice-digital/nds-container'

import styles from './ApplicationLayout.module.scss'
import { Footer } from './Footer/Footer'
import { Header } from './Header/Header'
import { Main } from './Main/Main'

import type { ReactNode } from 'react'

export function ApplicationLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <div className={styles.contentWrapper}>
        <Header skipLinkId="content-start" />
        <Main className={styles.mainContent} id="content-start">
          <Container>{children}</Container>
        </Main>
        <Footer />
      </div>
    </>
  )
}
