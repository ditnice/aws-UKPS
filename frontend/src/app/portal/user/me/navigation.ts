export type SearchParams = { updated?: string; requestedNewEmail?: string; mfaUpdated?: string }
export type TypedParams = {
  updated?: true
  requestedNewEmail?: true
  mfaUpdated?: 'primary' | 'secondary'
}

const mfaUpdateValues = ['primary', 'secondary'] as const
const myDetailsPath = '/portal/user/me'

export const buildMyDetailsHref = (params?: TypedParams): string => {
  const query = new URLSearchParams()

  if (params?.updated) query.set('updated', params.updated.toString())
  if (params?.mfaUpdated) query.set('mfaUpdated', params.mfaUpdated)
  if (params?.requestedNewEmail) query.set('requestedNewEmail', params.requestedNewEmail.toString())

  const qs = query.toString()
  return qs ? `${myDetailsPath}?${qs}` : myDetailsPath
}

const isMfaUpdated = (value: unknown): value is NonNullable<TypedParams['mfaUpdated']> =>
  mfaUpdateValues.includes(value as (typeof mfaUpdateValues)[number])

const convertStringToFlag = (input: string | undefined): true | undefined => {
  return typeof input === 'string' && input?.toLocaleLowerCase() === `${true}` ? true : undefined
}

export const parseTypedParamsFromSearchParams = (searchParams: SearchParams): TypedParams => {
  const { updated, requestedNewEmail, mfaUpdated } = searchParams

  return {
    updated: convertStringToFlag(updated),
    requestedNewEmail: convertStringToFlag(requestedNewEmail),
    mfaUpdated: isMfaUpdated(mfaUpdated) ? mfaUpdated : undefined,
  }
}
