import Link from 'next/link'

export default function NotFound() {
  return (
    <div>
      {/* TODO: Replace this static public 404 with Payload-managed content once the CMS page/global model exists. */}
      <h2>Not Found</h2>
      <p>Could not find requested resource</p>
      <Link href="/">Return Home</Link>
    </div>
  )
}
