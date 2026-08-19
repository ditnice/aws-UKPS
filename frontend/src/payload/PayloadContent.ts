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

// Current design is that a path is always `/${slug}`, i.e. no nested routes like /about-us/what-is-ukps
function pathToSlug(path: string): string {
  return path.replace(/^\//, '')
}

export const getAllPages = cache(async (): Promise<SitePage[]> => {
  try {
    const payload = await getPayloadClient()
    const result = await payload.find({
      collection: 'pages',
      limit: 0,
      pagination: false,
      sort: 'slug',
    })

    const pages = result.docs
      .map((doc) => mapPage(doc))
      .filter((doc): doc is SitePage => Boolean(doc))

    return pages.length ? pages : defaultPages
  } catch (error) {
    console.error('Failed to load pages from Payload, falling back to default pages:', error)
    return defaultPages
  }
})

export const getPageByPath = cache(async (path: string): Promise<SitePage | null> => {
  try {
    const payload = await getPayloadClient()
    const result = await payload.find({
      collection: 'pages',
      limit: 1,
      pagination: false,
      where: {
        slug: {
          equals: pathToSlug(path),
        },
      },
    })

    const page = result.docs.map((doc) => mapPage(doc)).find((doc): doc is SitePage => Boolean(doc))

    return page ?? getDefaultPageByPath(path)
  } catch (error) {
    console.error(
      `Failed to load page "${path}" from Payload, falling back to default page:`,
      error,
    )
    return getDefaultPageByPath(path)
  }
})
