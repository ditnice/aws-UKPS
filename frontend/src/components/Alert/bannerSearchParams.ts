/**
 * Search parameters that trigger a one-off success banner. A success Alert removes these from the
 * address bar once it has been shown. Add a name here when a new banner is driven by a search
 * parameter.
 */
export const bannerSearchParams = [
  'action',
  'userId',
  'userRequestId',
  'updated',
  'requestedNewEmail',
  'mfaUpdated',
] as const
