import type { SitePage, SitePageBlock } from '@/payload/DefaultPages'
import type { Page } from '@/payload-types'

function mapBlock(block: Record<string, unknown>): SitePageBlock | null {
  const blockType = block.blockType

  if (
    blockType === 'textSection' &&
    typeof block.heading === 'string' &&
    typeof block.body === 'string'
  ) {
    return {
      blockType,
      body: block.body,
      heading: block.heading,
      id: typeof block.id === 'string' ? block.id : undefined,
      variant: block.variant === 'homeStandard' ? 'homeStandard' : 'default',
    }
  }

  if (
    blockType === 'tabs' &&
    Array.isArray(block.tabs) &&
    block.tabs.every(
      (tab) =>
        typeof tab === 'object' &&
        tab !== null &&
        'title' in tab &&
        'body' in tab &&
        typeof tab.title === 'string' &&
        typeof tab.body === 'string',
    )
  ) {
    return {
      blockType,
      id: typeof block.id === 'string' ? block.id : undefined,
      tabs: block.tabs.map((tab) => ({
        title: tab.title,
        body: tab.body,
      })),
    }
  }

  if (
    blockType === 'accordion' &&
    Array.isArray(block.items) &&
    block.items.every(
      (item) =>
        typeof item === 'object' &&
        item !== null &&
        'title' in item &&
        'body' in item &&
        typeof item.title === 'string' &&
        typeof item.body === 'string',
    )
  ) {
    return {
      blockType,
      id: typeof block.id === 'string' ? block.id : undefined,
      items: block.items.map((item) => ({
        title: item.title,
        body: item.body,
      })),
    }
  }

  if (
    blockType === 'columnList' &&
    Array.isArray(block.items) &&
    typeof block.heading === 'string' &&
    (block.columns === '2' || block.columns === '3') &&
    block.items.every(
      (item) =>
        typeof item === 'object' &&
        item !== null &&
        'text' in item &&
        typeof item.text === 'string',
    )
  ) {
    return {
      blockType,
      id: typeof block.id === 'string' ? block.id : undefined,
      heading: block.heading,
      columns: block.columns === '2' ? 2 : 3,
      items: block.items.map((item) => ({
        text: item.text,
      })),
    }
  }

  return null
}

export function mapPage(doc: Page): SitePage | null {
  if (!(doc.title && doc.slug && doc.path)) {
    return null
  }

  const layout = Array.isArray(doc.layout)
    ? doc.layout
        .map((block) =>
          block && typeof block === 'object' ? mapBlock(block as Record<string, unknown>) : null,
        )
        .filter((block): block is SitePageBlock => Boolean(block))
    : []

  if (!layout.length) {
    return null
  }

  return {
    id: doc.id,
    layout,
    navigationGroup: doc.navigationGroup ?? undefined,
    navigationLabel: doc.navigationLabel ?? undefined,
    navigationOrder: doc.navigationOrder ?? undefined,
    path: doc.path,
    slug: doc.slug,
    title: doc.title,
  }
}
