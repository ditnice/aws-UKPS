/**
 * Search parameters that trigger a one-off success banner. An Alert with `showOnce` is shown only
 * when one of these is in the URL on arrival, and removes them from the address bar once shown.
 * Add a name here, and set `showOnce` on the Alert, when a new banner is driven by a search
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
