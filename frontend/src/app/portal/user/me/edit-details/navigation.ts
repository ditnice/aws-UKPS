export type EditDetailsProps = {
  sessionKey?: string
}

export const buildEditDetailsHref = (props?: EditDetailsProps): string => {
  const params = new URLSearchParams()
  const { sessionKey } = props ?? {}
  if (sessionKey) {
    params.set('sessionKey', sessionKey)
  }

  const query = params.toString()

  return `/portal/user/me/edit-details${query ? `?${query}` : ''}`
}
