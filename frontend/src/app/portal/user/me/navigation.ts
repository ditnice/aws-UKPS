export type SearchParams = { updated?: string; mfaUpdated?: string }
export type TypedParams = { updated?: true; mfaUpdated?: 'primary' | 'secondary' }

const mfaUpdateValues = ['primary', 'secondary'] as const
const myDetailsPath = '/portal/user/me'

export const buildMyDetailsHref = (params?: TypedParams): string => {
  const query = new URLSearchParams()

  if (params?.updated) query.set('updated', params.updated.toString())
  if (params?.mfaUpdated) query.set('mfaUpdated', params.mfaUpdated)

  const qs = query.toString()
  return qs ? `${myDetailsPath}?${qs}` : myDetailsPath
}

const isMfaUpdated = (value: unknown): value is NonNullable<TypedParams['mfaUpdated']> =>
  mfaUpdateValues.includes(value as (typeof mfaUpdateValues)[number])

export const parseTypedParamsFromSearchParams = (searchParams: SearchParams): TypedParams => {
  const { updated, mfaUpdated } = searchParams

  return {
    updated:
      typeof updated === 'string' && updated?.toLocaleLowerCase() === `${true}` ? true : undefined,
    mfaUpdated: isMfaUpdated(mfaUpdated) ? mfaUpdated : undefined,
  }
}
