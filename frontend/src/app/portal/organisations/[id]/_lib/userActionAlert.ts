export const userActions = ['invited', 'permissions-updated', 'deactivated', 'reactivated'] as const
export type UserAction = (typeof userActions)[number]

export const userRequestActions = ['approved-request', 'rejected-request'] as const
export type UserRequestAction = (typeof userRequestActions)[number]

export interface UserActionSearchParams {
  action?: string
  userId?: string
  userRequestId?: string
}

export type RegisteredUserResult = {
  type: 'user'
  action: UserAction
  userId: number
}
export type RequestResult = {
  type: 'request'
  action: UserRequestAction
  userRequestId: number
}
export type UserActionResult = RegisteredUserResult | RequestResult

function isUserAction(value: string | undefined): value is UserAction {
  return userActions.includes(value as UserAction)
}

function isUserRequestAction(value: string | undefined): value is UserRequestAction {
  return userRequestActions.includes(value as UserRequestAction)
}

export function parseUserAction({
  action,
  userId,
  userRequestId,
}: UserActionSearchParams): UserActionResult | undefined {
  const parsedUserId = Number(userId)
  const parsedUserRequestId = Number(userRequestId)

  if (isUserAction(action) && Number.isInteger(parsedUserId) && parsedUserId > 0) {
    return { type: 'user', action, userId: parsedUserId }
  }
  if (
    isUserRequestAction(action) &&
    Number.isInteger(parsedUserRequestId) &&
    parsedUserRequestId > 0
  ) {
    return { type: 'request', action, userRequestId: parsedUserRequestId }
  }
  return undefined
}

type UserHrefArgs =
  | {
      action: UserAction
      userId: number
    }
  | { action: UserRequestAction; userRequestId: number }
export function buildUserActionHref(organisationId: number, args: UserHrefArgs): string {
  return 'userId' in args
    ? `/portal/organisations/${organisationId}?action=${args.action}&userId=${args.userId}`
    : `/portal/organisations/${organisationId}?action=${args.action}&userRequestId=${args.userRequestId}`
}
