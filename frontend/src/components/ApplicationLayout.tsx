'use client'

import { Footer, Header, Main } from '@nice-digital/global-nav'

import type { ReactNode } from 'react'

export function ApplicationLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <Header auth={false} search={false} skipLinkId="content-start" />
      <Main id="content-start">{children}</Main>
      <Footer />
    </>
  )
}
