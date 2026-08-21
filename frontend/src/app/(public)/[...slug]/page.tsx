// src/app/(public)/[...slug]/page.tsx
import { notFound } from 'next/navigation'

import RenderPageLayout from '@/components/RenderPageLayout'
import { getPageByPath } from '@/payload/PayloadContent'

interface Props {
  params: Promise<{ slug: string[] }>
}

export default async function CmsPage({ params }: Props) {
  const { slug } = await params
  const path = `/${slug.join('/')}`
  const page = await getPageByPath(path)

  if (!page) notFound()

  return <RenderPageLayout blocks={page.layout} />
}
