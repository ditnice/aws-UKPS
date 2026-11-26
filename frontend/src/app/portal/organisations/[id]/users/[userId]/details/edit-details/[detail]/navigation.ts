import { notFound } from 'next/navigation'

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

export type TypedPageProps = (
  | {
      detail: 'fullName' | 'contactNumber'
    }
  | {
      detail: 'email'
      verificationLinkResent?: true
      error?: true
    }
) & { userId: number; organisationId: number; returnLink: string; onSuccessFlag: string }

export type StaticPageProps = {
  userId: number
  organisationId: number
  detail: 'fullName' | 'contactNumber' | 'email'
  returnLink: string
  onSuccessFlag: string
}

const isUserDetail = (value: string): value is UserDetail =>
  (userDetails as readonly string[]).includes(value)

const parseTrueFlag = (value: string | undefined): true | undefined =>
  value?.toLowerCase() === 'true' ? true : undefined

// Only allow same-site relative paths, to avoid an open redirect (e.g. ?navigateToOnSuccess=https://evil.com)
const parseSafeRedirect = (value: string | undefined): string =>
  value && value.startsWith('/') && !value.startsWith('//') && !value.startsWith('/\\')
    ? value
    : DEFAULT_NAVIGATE_TO_ON_SUCCESS

export const getStaticProps = ({
  detail,
  userId,
  organisationId,
  returnLink,
  onSuccessFlag,
}: TypedPageProps): StaticPageProps => {
  return { detail, userId, organisationId, returnLink, onSuccessFlag }
}

const parseId = (value: string | undefined): number => {
  if (value === undefined || value.trim() === '') notFound()

  const parsed = Number(value)

  if (!Number.isInteger(parsed) || parsed < 0) notFound()

  return parsed
}

export const findTypedPageProps = async (props: PageProps): Promise<TypedPageProps> => {
  const params = await props.params
  const searchParams = await props.searchParams

  return {
    organisationId: parseId(params.id),
    userId: parseId(params.userId),
    detail: isUserDetail(params.detail) ? params.detail : notFound(),
    returnLink: parseSafeRedirect(searchParams?.returnLink),
    onSuccessFlag: searchParams?.onSuccessFlag ?? '',
    verificationLinkResent: parseTrueFlag(searchParams?.verificationLinkResent),
    error: parseTrueFlag(searchParams?.error),
  }
}

export const buildUpdateUserDetailsHref = (props: TypedPageProps) => {
  const query = new URLSearchParams()

  if (props.detail === 'email') {
    if (props.verificationLinkResent) query.set('verificationLinkResent', 'true')
    if (props.error) query.set('error', 'true')
  }

  query.set('returnLink', props.returnLink)
  query.set('onSuccessFlag', props.onSuccessFlag)

  return `/portal/organisations/${props.organisationId}/users/${props.userId}/details/edit-details/${props.detail}?${query.toString()}`
}
