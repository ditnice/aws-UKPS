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
  id?: number
  layout: SitePageBlock[]
  path: string
  slug: string
  title: string
}

// TODO Add Read More Section. Not added as the link block doesn't exist

export const defaultPages: SitePage[] = [
  {
    layout: [
      {
        blockType: 'textSection',
        body: `
UK PharmaScan is a database of information on new medicines, indications, and formulations in the pharmaceutical pipeline. It is the primary source of information used by all of the UK's national horizon scanning organisations, NHS England and NHS Improvement to enable early engagement in planning and preparing the NHS for the introduction of new medicines, and to support faster NHS adoption. It is populated by companies on a confidential and secure platform.

Early engagement and NHS planning have been shown to improve the speed of medicines uptake. By regularly entering comprehensive information on pipeline medicines into UK PharmaScan, product developers ensure the NHS horizon scanning organisations have the information they need.

There is no charge for pharmaceutical companies to use UK PharmaScan.`,
        heading: 'What is UK PharmaScan?',
        variant: 'homeStandard',
      },
    ],
    path: '/',
    slug: 'home',
    title: 'UK PharmaScan',
  },
]
