import { describe, expect, it } from 'vitest'

import type { Page } from '@/payload-types'

import { mapPage } from './SitePageMapper'

type Block = Page['layout'][number]

// Payload returns layout blocks as untyped JSON at runtime, so tests build malformed blocks
// the generated type would otherwise reject.
const makePage = (layout: unknown[], overrides: Partial<Page> = {}): Page => ({
  id: 1,
  title: 'About us',
  slug: 'about-us',
  layout: layout as Block[],
  createdAt: '2026-01-01T00:00:00.000Z',
  updatedAt: '2026-01-01T00:00:00.000Z',
  ...overrides,
})

const textSection = { blockType: 'textSection', id: 'b1', heading: 'Heading', body: 'Body' }

describe('mapPage', () => {
  it('maps a page with valid blocks to a site page at the given path', () => {
    expect(mapPage(makePage([textSection]), '/about-us')).toEqual({
      id: 1,
      layout: [
        {
          blockType: 'textSection',
          id: 'b1',
          heading: 'Heading',
          body: 'Body',
          variant: 'default',
        },
      ],
      path: '/about-us',
      slug: 'about-us',
      title: 'About us',
    })
  })

  it.each([
    ['title', { title: '' }],
    ['slug', { slug: '' }],
  ])('returns null when the page has no %s', (_, overrides) => {
    expect(mapPage(makePage([textSection], overrides), '/about-us')).toBeNull()
  })

  it('returns null when no block in the layout is valid', () => {
    expect(mapPage(makePage([{ blockType: 'textSection', heading: 'No body' }]), '/')).toBeNull()
  })

  it('returns null when the layout is not an array', () => {
    expect(mapPage(makePage(null as unknown as unknown[]), '/')).toBeNull()
  })

  it('drops invalid blocks and keeps valid ones', () => {
    const page = mapPage(makePage([null, { blockType: 'unknown' }, textSection]), '/')

    expect(page?.layout).toHaveLength(1)
    expect(page?.layout[0]).toMatchObject({ blockType: 'textSection', id: 'b1' })
  })

  describe('textSection blocks', () => {
    it('keeps the homeStandard variant', () => {
      const page = mapPage(makePage([{ ...textSection, variant: 'homeStandard' }]), '/')

      expect(page?.layout[0]).toMatchObject({ variant: 'homeStandard' })
    })

    it('falls back to the default variant for unknown values', () => {
      const page = mapPage(makePage([{ ...textSection, variant: 'unexpected' }]), '/')

      expect(page?.layout[0]).toMatchObject({ variant: 'default' })
    })

    it('omits a non-string id', () => {
      const page = mapPage(makePage([{ ...textSection, id: 42 }]), '/')

      expect(page?.layout[0].id).toBeUndefined()
    })
  })

  describe('tabs blocks', () => {
    it('maps tabs to title and body only', () => {
      const block = { blockType: 'tabs', tabs: [{ id: 't1', title: 'Tab', body: 'Content' }] }

      expect(mapPage(makePage([block]), '/')?.layout[0]).toEqual({
        blockType: 'tabs',
        id: undefined,
        tabs: [{ title: 'Tab', body: 'Content' }],
      })
    })

    it('drops the block when any tab is malformed', () => {
      const block = { blockType: 'tabs', tabs: [{ title: 'Tab', body: 'Content' }, { title: 1 }] }

      expect(mapPage(makePage([block, textSection]), '/')?.layout).toHaveLength(1)
    })
  })

  describe('accordion blocks', () => {
    it('maps items to title and body only', () => {
      const block = { blockType: 'accordion', items: [{ id: 'i1', title: 'Item', body: 'Text' }] }

      expect(mapPage(makePage([block]), '/')?.layout[0]).toEqual({
        blockType: 'accordion',
        id: undefined,
        items: [{ title: 'Item', body: 'Text' }],
      })
    })

    it('drops the block when any item is malformed', () => {
      const block = { blockType: 'accordion', items: [null] }

      expect(mapPage(makePage([block, textSection]), '/')?.layout).toHaveLength(1)
    })
  })

  describe('columnList blocks', () => {
    const columnList = { blockType: 'columnList', heading: 'List', items: [{ text: 'One' }] }

    it.each([
      ['2', 2],
      ['3', 3],
    ])('converts the "%s" column option to the number %d', (columns, expected) => {
      const page = mapPage(makePage([{ ...columnList, columns }]), '/')

      expect(page?.layout[0]).toEqual({
        blockType: 'columnList',
        id: undefined,
        heading: 'List',
        columns: expected,
        items: [{ text: 'One' }],
      })
    })

    it('drops the block when the column option is not supported', () => {
      const page = mapPage(makePage([{ ...columnList, columns: '4' }, textSection]), '/')

      expect(page?.layout).toHaveLength(1)
    })

    it('drops the block when any item has no text', () => {
      const block = { ...columnList, columns: '2', items: [{ text: 'One' }, {}] }

      expect(mapPage(makePage([block, textSection]), '/')?.layout).toHaveLength(1)
    })
  })
})
