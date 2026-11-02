export type TextSectionLayoutBlock = {
  blockType: 'textSection'
  id?: string
  body: string
  heading: string
  variant?: 'default' | 'homeStandard'
}

export type TabsLayoutBlock = {
  blockType: 'tabs'
  id?: string
  tabs: {
    title: string
    body: string
  }[]
}

export type AccordionLayoutBlock = {
  blockType: 'accordion'
  id?: string
  items: {
    title: string
    body: string
  }[]
}

export type ListLayoutBlock = {
  blockType: 'columnList'
  id?: string
  heading: string
  columns: 2 | 3
  items: {
    text: string
  }[]
}

export type MediaAsset = {
  alt: string
  filename?: string
  height: number
  id?: number | string
  url: string
  width: number
}

export type SitePageBlock =
  TextSectionLayoutBlock | TabsLayoutBlock | AccordionLayoutBlock | ListLayoutBlock

export type SitePageHeader = {
  preheading?: string
  lead?: string
  description?: string
}

export type SitePage = {
  id?: number
  layout: SitePageBlock[]
  pageHeader?: SitePageHeader
  path: string
  slug: string
  title: string
}

export const defaultPages: SitePage[] = [
  {
    layout: [],
    pageHeader: {
      lead: 'Home Test',
    },
    path: '/',
    slug: 'home',
    title: 'UK PharmaScan',
  },
]
