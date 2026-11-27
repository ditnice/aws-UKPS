import { notFound } from 'next/navigation'

import { parsePositiveInteger } from '@/lib/valueParsing'

export const updatedFlag = Object.keys({ updated: 'true' } satisfies SearchParams)[0]
export const requestedNewEmailFlag = Object.keys({
  requestedNewEmail: 'true',
} satisfies SearchParams)[0]

type SearchParams = { updated?: string; requestedNewEmail?: string }
export type PageParams = {
  // Next.js route params arrive as strings at runtime, whatever the type says
  params: Promise<{ id: string; userId: string }>
  searchParams: Promise<{ updated?: string; requestedNewEmail?: string }>
}

export type TypedParams = {
  userId: number
  organisationId: number
  updated?: true
  requestedNewEmail?: true
}

const convertStringToFlag = (input: string | undefined): true | undefined => {
  return typeof input === 'string' && input?.toLocaleLowerCase() === `${true}` ? true : undefined
}

export const parseTypedParams = async (pageParams: PageParams): Promise<TypedParams> => {
  const { id, userId } = await pageParams.params
  const { updated, requestedNewEmail } = await pageParams.searchParams

  return {
    organisationId: parsePositiveInteger(id) ?? notFound(),
    userId: parsePositiveInteger(userId) ?? notFound(),
    updated: convertStringToFlag(updated),
    requestedNewEmail: convertStringToFlag(requestedNewEmail),
  }
}

export const buildViewUserDetailsHref = ({ userId, organisationId }: TypedParams) => {
  return `/portal/organisations/${organisationId}/users/${userId}/details`
}
