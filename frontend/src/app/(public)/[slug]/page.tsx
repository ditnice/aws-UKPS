import { notFound } from 'next/navigation'

interface Props {
  params: Promise<{ slug: string }>
}

export default async function CmsPage({ params }: Props) {
  const { slug } = await params
  // TODO: query Payload Pages collection (Step 6)
  void slug
  notFound()
}
