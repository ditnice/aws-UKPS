'use client'

import { Container } from '@nice-digital/nds-container'

import styles from './ApplicationLayout.module.scss'
import { Footer } from './Footer/Footer'
import { Header } from './Header/Header'
import { NavLinkItem } from './Header/Nav/NavLinks/NavLinks'
import { Main } from './Main/Main'

import type { ReactElement, ReactNode } from 'react'

export function ApplicationLayout({
  links,
  headerExtension,
  children,
}: {
  links: NavLinkItem[]
  headerExtension?: ReactElement
  children: ReactNode
}) {
  return (
    <>
      <div className={styles.contentWrapper}>
        <Header skipLinkId="content-start" links={links} headerExtension={headerExtension} />
        <Main className={styles.mainContent} id="content-start">
          <Container>{children}</Container>
        </Main>
        <Footer />
      </div>
    </>
  )
}
