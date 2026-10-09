import type { ProblemDetails } from '@/client/generated/types.gen'

type ErrorContent = {
  detail: string
  title: string
}

export function getSetupTokenErrorContent(error: ProblemDetails, status?: number): ErrorContent {
  const title = error.title ?? 'There is a problem with your sign-up link'
  let fallbackDetail: string

  switch (status) {
    case 404:
      fallbackDetail = 'This sign-up link could not be found.'
      break
    case 409:
      fallbackDetail = 'This sign-up link has already been used.'
      break
    case 400:
      fallbackDetail = 'This sign-up link is not valid.'
      break
    default:
      fallbackDetail = 'We could not check your sign-up link. Try again later.'
  }

  return { title, detail: error.detail ?? fallbackDetail }
}
