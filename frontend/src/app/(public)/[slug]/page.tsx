import { notFound } from 'next/navigation'

interface Props {
  params: Promise<{ slug: string }>
}

export default async function CmsPage({ params }: Props) {
  const { slug } = await params
  // TODO: Query Payload for the requested public page slug and call notFound() only when no CMS page exists.
  void slug
  notFound()
}
