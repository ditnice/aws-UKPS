import { getPayload } from 'payload'

import config from '@payload-config'

export const dynamic = 'force-dynamic'

export async function GET() {
  try {
    await getPayload({ config })

    return Response.json(
      { status: 'ok' },
      {
        status: 200,
        headers: {
          'Cache-Control': 'no-store',
        },
      },
    )
  } catch (error) {
    console.error('Payload initialization failed', error)

    return Response.json(
      { status: 'unavailable' },
      {
        status: 503,
        headers: {
          'Cache-Control': 'no-store',
        },
      },
    )
  }
}
