'use client'

import { Main } from '@nice-digital/global-nav'
import { Container } from '@nice-digital/nds-container'

import { Footer } from './Footer/Footer'
import { Header } from './Header/Header'

import type { ReactNode } from 'react'

export function ApplicationLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <Header skipLinkId="content-start"></Header>
      <Main id="content-start">
        <Container>{children}</Container>
      </Main>
      <Footer />
    </>
  )
}
