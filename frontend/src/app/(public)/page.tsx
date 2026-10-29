import { notFound } from 'next/navigation'

import RenderPageLayout from '@/components/RenderPageLayout'
import { getPageByPath } from '@/payload/PayloadContent'

export const dynamic = 'force-dynamic'

export default async function HomePage() {
  const page = await getPageByPath('/')
  if (!page) notFound()
  return <RenderPageLayout blocks={page.layout} title={page.title} />
}
