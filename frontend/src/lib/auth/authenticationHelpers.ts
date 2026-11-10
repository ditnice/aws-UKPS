import { AuthenticationProblemDetails } from '@/client/generated'

interface WwwAuthenticateChallenge {
  scheme: string
  params: Record<string, string>
}

const parseWwwAuthenticate = (headerValue: string | null): WwwAuthenticateChallenge | null => {
  if (!headerValue) return null

  // Split off the scheme (e.g. "Bearer")
  const match = headerValue.match(/^(\w+)\s+(.*)$/)
  if (!match) return null

  const [, scheme, paramString] = match
  const params: Record<string, string> = {}

  // Matches key="quoted value" or key=token
  const paramRegex = /([\w-]+)\s*=\s*(?:"((?:[^"\\]|\\.)*)"|([^\s,]+))/g
  let m: RegExpExecArray | null

  while ((m = paramRegex.exec(paramString)) !== null) {
    const [, key, quoted, token] = m
    params[key] = quoted !== undefined ? quoted.replace(/\\(.)/g, '$1') : token
  }

  return { scheme, params }
}

const getWwwAuthenticateFromResponse = (response: Response): WwwAuthenticateChallenge | null => {
  if (response.status !== 401) return null
  return parseWwwAuthenticate(response.headers.get('WWW-Authenticate'))
}

export const getStepUpMaxAge = (response: Response): number | null => {
  const parsed = getWwwAuthenticateFromResponse(response)
  if (parsed?.params['error'] !== 'insufficient_user_authentication') {
    return null
  }
  const maxAge = Number.parseInt(parsed.params['max_age'] ?? '', 10)
  return Number.isNaN(maxAge) ? null : maxAge
}

export const isReauthenticationRequiredResponse = (response: Response) => {
  return getStepUpMaxAge(response) !== null
}

type ReauthenticationRequiredError = AuthenticationProblemDetails & {
  code: 'ReauthenticationRequired'
}
export const isReauthenticationRequiredError = (
  error: unknown,
): error is ReauthenticationRequiredError => {
  return (
    typeof error === 'object' &&
    error !== null &&
    'code' in error &&
    error.code === 'ReauthenticationRequired'
  )
}
