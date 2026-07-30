'use client'

import { Footer, Main } from '@nice-digital/global-nav'
import { Container } from '@nice-digital/nds-container'

import { Header as MyHeader } from './Header/Header'

import type { ReactNode } from 'react'

export function ApplicationLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <MyHeader skipLinkId="content-start"></MyHeader>
      <Main id="content-start">
        <Container>{children}</Container>
      </Main>
      <Footer />
    </>
  )
}
