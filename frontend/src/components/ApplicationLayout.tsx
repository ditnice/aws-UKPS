'use client'

import { Footer, Header, Main } from '@nice-digital/global-nav'
import { Container } from '@nice-digital/nds-container'

import type { ReactNode } from 'react'

export function ApplicationLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <Header auth={false} search={false} skipLinkId="content-start" />
      <Main id="content-start">
        <Container>{children}</Container>
      </Main>
      <Footer />
    </>
  )
}
