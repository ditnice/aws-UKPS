export type UserDetail = (typeof userDetails)[number]

const userDetails = ['fullName', 'contactNumber', 'email'] as const

export type PageProps = {
  params: Promise<{
    sku: string
  }>
}

export type TypedPageProps = {
  params: {
    sku: UserDetail | undefined
  }
}

export const findTypedPageProps = async (props: PageProps): Promise<TypedPageProps> => {
  const params = await props.params
  return {
    params: {
      sku: userDetails.find((x) => x === params.sku),
    },
  }
}

type buildUpdateUserDetailsHrefProps = {
  sku: UserDetail
}
export const buildUpdateUserDetailsHref = ({ sku }: buildUpdateUserDetailsHrefProps) =>
  `/portal/user/me/edit-details/${sku}`
