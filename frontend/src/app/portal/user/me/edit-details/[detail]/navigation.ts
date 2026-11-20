export type UserDetail = (typeof userDetails)[number]

const userDetails = ['fullName', 'contactNumber', 'email'] as const

export type PageProps = {
  params: Promise<{
    detail: string
  }>
  searchParams: Promise<{
    verificationLinkResent?: string
  }>
}

export type TypedPageProps = {
  params: {
    detail: UserDetail | undefined
  }
  searchParams: {
    verificationLinkResent?: true
  }
}

export const findTypedPageProps = async (props: PageProps): Promise<TypedPageProps> => {
  const params = await props.params
  const searchParams = await props.searchParams
  return {
    params: {
      detail: userDetails.find((x) => x === params.detail),
    },
    searchParams: {
      verificationLinkResent:
        searchParams?.verificationLinkResent?.toLocaleLowerCase() === `${true}` ? true : undefined,
    },
  }
}

type buildUpdateUserDetailsHrefProps =
  | {
      detail: 'fullName' | 'contactNumber'
    }
  | {
      detail: 'email'
      emailLinkResent?: true
    }
export const buildUpdateUserDetailsHref = (props: buildUpdateUserDetailsHrefProps) => {
  const query = new URLSearchParams()
  if (props.detail === 'email' && props.emailLinkResent)
    query.set('verificationLinkResent', `${true}`)

  const path = `/portal/user/me/edit-details/${props.detail}`
  const qs = query.toString()
  return qs ? `${path}?${qs}` : path
}
