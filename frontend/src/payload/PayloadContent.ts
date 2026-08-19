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

function getDefaultPagesByNavigationGroup(group: string) {
  return defaultPages
    .filter((page) => page.navigationGroup === group)
    .sort((left, right) => (left.navigationOrder ?? 0) - (right.navigationOrder ?? 0))
}

export const getAllPages = cache(async (): Promise<SitePage[]> => {
  try {
    const payload = await getPayloadClient()
    const result = await payload.find({
      collection: 'pages',
      limit: 0,
      pagination: false,
      sort: 'path',
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
        path: {
          equals: path,
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

export const getPagesByNavigationGroup = cache(async (group: string): Promise<SitePage[]> => {
  try {
    const payload = await getPayloadClient()
    const result = await payload.find({
      collection: 'pages',
      limit: 0,
      pagination: false,
      sort: 'navigationOrder',
      where: {
        navigationGroup: {
          equals: group,
        },
      },
    })

    const pages = result.docs
      .map((doc) => mapPage(doc))
      .filter((doc): doc is SitePage => Boolean(doc))

    return pages.length ? pages : getDefaultPagesByNavigationGroup(group)
  } catch (error) {
    console.error(
      `Failed to load pages for navigation group "${group}" from Payload, falling back to default pages:`,
      error,
    )
    return getDefaultPagesByNavigationGroup(group)
  }
})
