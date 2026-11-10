import { isValidPhoneNumber } from 'libphonenumber-js/max'
import { useMemo, useSyncExternalStore } from 'react'
import z from 'zod'

import { UpdateUserDetailsCommand } from '@/client/generated'
import { errorMessages } from '@/lib/form/errorMessages'

export const EditDetails = z.object({
  fullName: z.string().trim().min(1, errorMessages.personalFullNameRequired),
  workEmail: z
    .string()
    .trim()
    .min(1, errorMessages.workEmailRequired)
    .pipe(z.email(errorMessages.emailFormat)),
  workTelephone: z
    .string()
    .trim()
    .min(1, errorMessages.personalPhoneRequired)
    .refine((value) => isValidPhoneNumber(value, 'GB'), errorMessages.phoneFormat),
})

const consumed = new Map<string, string | null>()

const readAndClear = (key: string | undefined): string | null => {
  if (!key) return null
  if (consumed.has(key)) return consumed.get(key) ?? null

  let raw: string | null = null
  try {
    raw = sessionStorage.getItem(key)
    sessionStorage.removeItem(key)
  } catch {}

  consumed.set(key, raw)
  return raw
}

const subscribe = () => () => {}

const getServerSnapshot = () => null

const parseDetails = (raw: string | null): UpdateUserDetailsCommand | undefined => {
  if (!raw) return undefined
  try {
    const parsed = EditDetails.safeParse(JSON.parse(raw))
    return parsed.success ? parsed.data : undefined
  } catch {
    return undefined
  }
}

// `state` is a snapshot taken at render time. Writes via `store`/`clear`
// deliberately don't update it, so the form doesn't reset mid-submit.
export const useStoredUserDetails = (
  key: string | undefined,
): UpdateUserDetailsCommand | undefined => {
  const raw = useSyncExternalStore(subscribe, () => readAndClear(key), getServerSnapshot)
  const state = useMemo(() => parseDetails(raw), [raw])
  return state
}

export const storeUserDetails = (details: UpdateUserDetailsCommand) => {
  const newKey = crypto.randomUUID()
  try {
    sessionStorage.setItem(newKey, JSON.stringify(details))
  } catch {}
  return newKey
}
