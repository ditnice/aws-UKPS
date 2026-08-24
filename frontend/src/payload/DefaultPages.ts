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

export type SitePage = {
  id?: string
  layout: SitePageBlock[]
  navigationGroup?: string
  navigationLabel?: string
  navigationOrder?: number
  path: string
  slug: string
  title: string
}

export const defaultPages: SitePage[] = [
  {
    layout: [
      {
        blockType: 'textSection',
        body: 'Home Test',
        heading: 'Home',
        variant: 'homeStandard',
      },
    ],
    path: '/',
    slug: 'home',
    title: 'Home',
  },
]
