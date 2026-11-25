import { useSyncExternalStore } from 'react'

export type StoredState<T> =
  { status: 'ready'; data: T } | { status: 'error' } | { status: 'loading' }

type StoreOptions<T> = {
  /** Validate/narrow the parsed JSON. Return false to treat the value as an error. */
  validate?: (value: unknown) => value is T
  /** Defaults to sessionStorage. Pass a getter so SSR never touches window. */
  getStorage?: () => Storage
}

const loadingState = { status: 'loading' } as const
const errorState = { status: 'error' } as const

export const createSessionStorageStore = <T>(key: string, options: StoreOptions<T> = {}) => {
  const { validate, getStorage = () => sessionStorage } = options

  // Per-store cache so getSnapshot returns a stable reference between changes.
  let cachedRaw: string | null | undefined
  let cachedState: StoredState<T> | undefined
  const listeners = new Set<() => void>()

  const notify = () => listeners.forEach((listener) => listener())

  const resetCache = () => {
    cachedRaw = undefined
    cachedState = undefined
  }

  const getSnapshot = (): StoredState<T> => {
    let raw: string | null
    try {
      raw = getStorage().getItem(key)
    } catch {
      return errorState
    }

    if (cachedState && raw === cachedRaw) return cachedState
    cachedRaw = raw

    if (!raw) return (cachedState = errorState)

    try {
      const parsed: unknown = JSON.parse(raw)
      if (validate && !validate(parsed)) return (cachedState = errorState)
      return (cachedState = { status: 'ready', data: parsed as T })
    } catch {
      return (cachedState = errorState)
    }
  }

  const subscribe = (listener: () => void) => {
    listeners.add(listener)
    return () => {
      listeners.delete(listener)
    }
  }

  const set = (data: T): boolean => {
    try {
      getStorage().setItem(key, JSON.stringify(data))
    } catch {
      return false // storage unavailable or quota exceeded
    }
    notify()
    return true
  }

  const clear = () => {
    try {
      getStorage().removeItem(key)
    } catch {
      // storage unavailable – nothing to clear
    }
    resetCache()
    notify()
  }

  const getServerSnapshot = () => loadingState

  const useStore = () => {
    const state = useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot)
    return { ...state, set, clear }
  }

  return { useStore, set, clear, getSnapshot }
}
