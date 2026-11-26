import { notFound } from 'next/navigation'

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

const parseId = (value: string | undefined): number => {
  if (value === undefined || value.trim() === '') notFound()

  const parsed = Number(value)

  if (!Number.isInteger(parsed) || parsed < 0) notFound()

  return parsed
}

export const parseTypedParams = async (pageParams: PageParams): Promise<TypedParams> => {
  const { id, userId } = await pageParams.params
  const { updated, requestedNewEmail } = await pageParams.searchParams

  return {
    organisationId: parseId(id),
    userId: parseId(userId),
    updated: convertStringToFlag(updated),
    requestedNewEmail: convertStringToFlag(requestedNewEmail),
  }
}

export const buildViewUserDetailsHref = ({ userId, organisationId }: TypedParams) => {
  return `/portal/organisations/${organisationId}/users/${userId}/details`
}
