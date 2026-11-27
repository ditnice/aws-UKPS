import { notFound } from 'next/navigation'

import { parsePositiveInteger } from '@/lib/valueParsing'

const userDetails = ['fullName', 'contactNumber', 'email'] as const
export type UserDetail = (typeof userDetails)[number]

const DEFAULT_NAVIGATE_TO_ON_SUCCESS = '/portal'

export type PageProps = {
  params: Promise<{
    id: string
    userId: string
    detail: string
  }>
  searchParams?: Promise<{
    returnLink?: string
    onSuccessFlag?: string
    verificationLinkResent?: string
    error?: string
  }>
}

type CommonPageProps = {
  userId: number
  organisationId: number
  returnLink: string
  onSuccessFlag: string
}

export type TypedPageProps = (
  | {
      detail: Exclude<UserDetail, 'email'>
    }
  | {
      detail: 'email'
      verificationLinkResent?: true
      error?: true
    }
) &
  CommonPageProps

export type StaticPageProps = {
  userId: number
  organisationId: number
  detail: UserDetail
  returnLink: string
  onSuccessFlag: string
}

const isUserDetail = (value: string): value is UserDetail =>
  (userDetails as readonly string[]).includes(value)

const parseTrueFlag = (value: string | undefined): true | undefined =>
  value?.toLowerCase() === 'true' ? true : undefined

// Only allow same-origin relative paths, to avoid an open redirect (e.g. ?returnLink=https://evil.com).
// Resolving against a dummy origin catches tricks a prefix check misses, such as
// "/\evil.com" or "/<tab>/evil.com", which browsers turn into "//evil.com".
const SAFE_REDIRECT_BASE = 'http://safe-location'

const parseSafeRedirect = (value: string | undefined): string => {
  if (!value || !value.startsWith('/')) return DEFAULT_NAVIGATE_TO_ON_SUCCESS

  try {
    const url = new URL(value, SAFE_REDIRECT_BASE)
    if (url.origin !== SAFE_REDIRECT_BASE) return DEFAULT_NAVIGATE_TO_ON_SUCCESS
    return `${url.pathname}${url.search}${url.hash}`
  } catch {
    return DEFAULT_NAVIGATE_TO_ON_SUCCESS
  }
}

export const getStaticProps = ({
  detail,
  userId,
  organisationId,
  returnLink,
  onSuccessFlag,
}: TypedPageProps): StaticPageProps => {
  return { detail, userId, organisationId, returnLink, onSuccessFlag }
}

export const findTypedPageProps = async (props: PageProps): Promise<TypedPageProps> => {
  const params = await props.params
  const searchParams = await props.searchParams

  const detail = isUserDetail(params.detail) ? params.detail : notFound()

  const common: CommonPageProps = {
    organisationId: parsePositiveInteger(params.id) ?? notFound(),
    userId: parsePositiveInteger(params.userId) ?? notFound(),
    returnLink: parseSafeRedirect(searchParams?.returnLink),
    onSuccessFlag: searchParams?.onSuccessFlag ?? '',
  }

  if (detail === 'email') {
    return {
      ...common,
      detail,
      verificationLinkResent: parseTrueFlag(searchParams?.verificationLinkResent),
      error: parseTrueFlag(searchParams?.error),
    }
  }

  return { ...common, detail }
}

export const buildUpdateUserDetailsHref = (props: TypedPageProps): string => {
  const query = new URLSearchParams()

  if (props.detail === 'email') {
    if (props.verificationLinkResent) query.set('verificationLinkResent', 'true')
    if (props.error) query.set('error', 'true')
  }

  query.set('returnLink', props.returnLink)
  if (props.onSuccessFlag) query.set('onSuccessFlag', props.onSuccessFlag)

  return `/portal/organisations/${props.organisationId}/users/${props.userId}/details/edit-details/${props.detail}?${query.toString()}`
}
