import { getPayload } from 'payload'
import { cache } from 'react'
import 'server-only'

import { defaultPages, type SitePage } from '@/payload/DefaultPages'
import { mapPage } from '@/payload/SitePageMapper'

import config from '@payload-config'

const getPayloadClient = cache(async () => getPayload({ config }))

function getDefaultPageByPath(path: string) {
  return defaultPages.find((page) => page.path === path) ?? null
}

function splitPathIntoSlugs(path: string): string[] {
  return path.split('/').filter(Boolean)
}

export type HeaderNavItem = {
  label: string
  path: string
}

export const getHeaderNav = cache(async (): Promise<HeaderNavItem[]> => {
  try {
    const payload = await getPayloadClient()
    // depth: 1 is enough — the `destination` relationship is restricted to top-level pages,
    // so never has a parent.
    const header = await payload.findGlobal({ slug: 'header', depth: 1 })

    return (header.headerLinks ?? [])
      .map((item) => {
        const page = item.destination
        if (!page || typeof page !== 'object' || !page.slug) {
          return null
        }
        return { label: item.label, path: `/${page.slug}` }
      })
      .filter((item): item is HeaderNavItem => Boolean(item))
  } catch (error) {
    console.error('Failed to load header navigation from Payload:', error)
    return []
  }
})

export const getPageByPath = cache(async (path: string): Promise<SitePage | null> => {
  try {
    const payload = await getPayloadClient()
    const slugs = splitPathIntoSlugs(path)

    if (!slugs.length) {
      return getDefaultPageByPath(path)
    }

    let parentId: number | undefined
    let doc = null

    // Slugs are not necessarily unique - you may have vaccines/resources and medicines/resources
    // so we need to start at the top and find the right resources page with the correct parent
    for (const slug of slugs) {
      const result = await payload.find({
        collection: 'pages',
        limit: 1,
        pagination: false,
        where: {
          parent: parentId ? { equals: parentId } : { exists: false },
          slug: { equals: slug },
        },
      })

      doc = result.docs[0] ?? null
      if (!doc) break
      parentId = doc.id
    }

    const page = doc ? mapPage(doc, `/${slugs.join('/')}`) : null

    return page ?? getDefaultPageByPath(path)
  } catch (error) {
    console.error(
      `Failed to load page "${path}" from Payload, falling back to default page:`,
      error,
    )
    return getDefaultPageByPath(path)
  }
})
