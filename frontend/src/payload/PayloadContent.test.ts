import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { Page } from '@/payload-types'

import { defaultPages } from './DefaultPages'
import { getHeaderNav, getPageByPath } from './PayloadContent'

// Payload is a boundary: replace getPayload with a fake client so the tests exercise our
// lookup, mapping and fallback logic without a database.
const payload = vi.hoisted(() => ({
  find: vi.fn(),
  findGlobal: vi.fn(),
}))

vi.mock('payload', () => ({ getPayload: vi.fn(async () => payload) }))
vi.mock('@payload-config', () => ({ default: {} }))
vi.mock('server-only', () => ({}))

const makePage = (id: number, slug: string, parent?: number): Page => ({
  id,
  slug,
  title: `Title ${slug}`,
  parent: parent ?? null,
  layout: [{ blockType: 'textSection', heading: slug, body: `Body ${slug}` }],
  createdAt: '2026-01-01T00:00:00.000Z',
  updatedAt: '2026-01-01T00:00:00.000Z',
})

type FindArgs = {
  where: { slug: { equals: string }; parent: { equals: number } | { exists: false } }
}

// A small in-memory page tree that answers `payload.find` the way the pages collection would.
const usePages = (pages: Page[]) => {
  payload.find.mockImplementation(async ({ where }: FindArgs) => {
    const parentId = 'equals' in where.parent ? where.parent.equals : null
    return {
      docs: pages.filter((p) => p.slug === where.slug.equals && (p.parent ?? null) === parentId),
    }
  })
}

beforeEach(() => {
  vi.spyOn(console, 'error').mockImplementation(() => undefined)
})

describe('getPageByPath', () => {
  it('returns a top-level page by slug', async () => {
    usePages([makePage(1, 'about-us')])

    const page = await getPageByPath('/about-us')

    expect(page).toMatchObject({ id: 1, path: '/about-us', title: 'Title about-us' })
    expect(payload.find).toHaveBeenCalledExactlyOnceWith({
      collection: 'pages',
      overrideAccess: false,
      limit: 1,
      pagination: false,
      where: { parent: { exists: false }, slug: { equals: 'about-us' } },
    })
  })

  it('resolves a nested page through its parent when slugs are not unique', async () => {
    usePages([
      makePage(1, 'vaccines'),
      makePage(2, 'medicines'),
      makePage(3, 'resources', 1),
      makePage(4, 'resources', 2),
    ])

    const page = await getPageByPath('/medicines/resources')

    expect(page).toMatchObject({ id: 4, path: '/medicines/resources' })
    for (const [options] of payload.find.mock.calls) {
      expect(options).toMatchObject({ overrideAccess: false })
    }
  })

  it('only matches top-level pages for the first slug', async () => {
    usePages([makePage(1, 'about-us'), makePage(2, 'resources', 1)])

    expect(await getPageByPath('/resources')).toBeNull()
  })

  it('stops looking once a slug in the path is not found', async () => {
    usePages([makePage(1, 'about-us')])

    expect(await getPageByPath('/missing/about-us')).toBeNull()
    expect(payload.find).toHaveBeenCalledOnce()
  })

  it('serves the default home page for the root path without querying Payload', async () => {
    expect(await getPageByPath('/')).toEqual(defaultPages[0])
    expect(payload.find).not.toHaveBeenCalled()
  })

  it('returns null when the stored page has no renderable blocks', async () => {
    usePages([{ ...makePage(1, 'empty'), layout: [] }])

    expect(await getPageByPath('/empty')).toBeNull()
  })

  it('falls back to the default page and logs when Payload fails', async () => {
    const error = new Error('Database unavailable')
    payload.find.mockRejectedValue(error)

    expect(await getPageByPath('/about-us')).toBeNull()
    expect(console.error).toHaveBeenCalledWith(
      'Failed to load page "/about-us" from Payload, falling back to default page:',
      error,
    )
  })
})

describe('getHeaderNav', () => {
  it('maps header links to labels and top-level page paths', async () => {
    payload.findGlobal.mockResolvedValue({
      headerLinks: [
        { label: 'About', destination: makePage(1, 'about-us') },
        { label: 'Help', destination: makePage(2, 'help') },
      ],
    })

    expect(await getHeaderNav()).toEqual([
      { label: 'About', path: '/about-us' },
      { label: 'Help', path: '/help' },
    ])
    expect(payload.findGlobal).toHaveBeenCalledWith({
      slug: 'header',
      depth: 1,
      overrideAccess: false,
    })
  })

  it('skips links whose destination was not populated', async () => {
    payload.findGlobal.mockResolvedValue({
      headerLinks: [
        { label: 'Unpopulated', destination: 7 },
        { label: 'Deleted', destination: null },
        { label: 'About', destination: makePage(1, 'about-us') },
      ],
    })

    expect(await getHeaderNav()).toEqual([{ label: 'About', path: '/about-us' }])
  })

  it('returns no links when the header has none', async () => {
    payload.findGlobal.mockResolvedValue({ headerLinks: null })

    expect(await getHeaderNav()).toEqual([])
  })

  it('returns no links and logs when Payload fails', async () => {
    const error = new Error('Database unavailable')
    payload.findGlobal.mockRejectedValue(error)

    expect(await getHeaderNav()).toEqual([])
    expect(console.error).toHaveBeenCalledWith(
      'Failed to load header navigation from Payload:',
      error,
    )
  })
})
