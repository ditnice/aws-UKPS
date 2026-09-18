'use client'

import { Accordion, AccordionGroup } from '@nice-digital/nds-accordion'
import { ColumnList } from '@nice-digital/nds-column-list'
import { Tab, Tabs } from '@nice-digital/nds-tabs'

import PlainTextContent from '@/components/PlainText/PlainTextContent'
import type { SitePageBlock } from '@/payload/DefaultPages'

type RenderPageLayoutProps = {
  blocks: SitePageBlock[]
  title: string
}

export default function RenderPageLayout({ blocks, title }: RenderPageLayoutProps) {
  return (
    <>
      <h1>{title}</h1>
      {blocks.map((block, index) => {
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

          case 'accordion':
            return (
              <AccordionGroup key={key}>
                {block.items.map((item, i) => (
                  <Accordion key={`${key}-item-${i}`} title={item.title}>
                    <p>{item.body}</p>
                  </Accordion>
                ))}
              </AccordionGroup>
            )

          case 'columnList':
            return (
              <section aria-labelledby={`${key}-heading`} key={key}>
                <h2 id={`${key}-heading`}>{block.heading}</h2>
                <ColumnList columns={block.columns}>
                  {block.items.map((item, i) => (
                    <li key={i}>{item.text}</li>
                  ))}
                </ColumnList>
              </section>
            )

          default:
            return null
        }
      })}
    </>
  )
}
