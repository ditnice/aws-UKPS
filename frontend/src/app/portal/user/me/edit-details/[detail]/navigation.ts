export type UserDetail = (typeof userDetails)[number]

const userDetails = ['fullName', 'contactNumber', 'email'] as const

export type PageProps = {
  params: Promise<{
    detail: string
  }>
}

export type TypedPageProps = {
  params: {
    detail: UserDetail | undefined
  }
}

export const findTypedPageProps = async (props: PageProps): Promise<TypedPageProps> => {
  const params = await props.params
  return {
    params: {
      detail: userDetails.find((x) => x === params.detail),
    },
  }
}

type buildUpdateUserDetailsHrefProps = {
  detail: UserDetail
}
export const buildUpdateUserDetailsHref = ({ detail }: buildUpdateUserDetailsHrefProps) =>
  `/portal/user/me/edit-details/${detail}`
