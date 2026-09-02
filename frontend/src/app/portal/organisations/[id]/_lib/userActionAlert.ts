export const userActions = ['invited', 'permissions-updated'] as const

export type UserAction = (typeof userActions)[number]

export interface UserActionSearchParams {
  action?: string
  userId?: string
}

export interface UserActionResult {
  action: UserAction
  userId: number
}

function isUserAction(value: string | undefined): value is UserAction {
  return userActions.includes(value as UserAction)
}

export function parseUserAction({
  action,
  userId,
}: UserActionSearchParams): UserActionResult | undefined {
  const parsedUserId = Number(userId)

  return isUserAction(action) && Number.isInteger(parsedUserId) && parsedUserId > 0
    ? { action, userId: parsedUserId }
    : undefined
}

export function buildUserActionHref(
  organisationId: number,
  action: UserAction,
  userId: number,
): string {
  return `/portal/organisations/${organisationId}?action=${action}&userId=${userId}`
}
