'use client'

import { Tab, Tabs } from '@nice-digital/nds-tabs'

import PlainTextContent from '@/components/PlainText/PlainTextContent'
import type { SitePageBlock } from '@/payload/DefaultPages'

type RenderPageLayoutProps = {
  blocks: SitePageBlock[]
}

export default function RenderPageLayout({ blocks }: RenderPageLayoutProps) {
  return blocks.map((block, index) => {
    const key = block.id ?? `${block.blockType}-${index}`

    switch (block.blockType) {
      case 'textSection':
        return (
          <section
            aria-labelledby={`${key}-heading`}
            className={
              block.variant === 'homeStandard'
                ? 'content-block home-standard thin-border--bottom pb--c'
                : 'content-block'
            }
            key={key}
          >
            <h2 id={`${key}-heading`}>{block.heading}</h2>
            <PlainTextContent text={block.body} />
          </section>
        )

      case 'tabs':
        return (
          <Tabs key={key}>
            {block.tabs.map((tab, index) => (
              <Tab key={`${key}-tab-${index}`} title={tab.title}>
                <p>{tab.body}</p>
              </Tab>
            ))}
          </Tabs>
        )

      default:
        return null
    }
  })
}
