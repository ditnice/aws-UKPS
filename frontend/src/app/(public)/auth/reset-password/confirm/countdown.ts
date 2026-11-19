'use client'

import { useCallback, useEffect, useState } from 'react'

export const useCountdown = (from: number) => {
  const [seconds, setSeconds] = useState<number>(from)

  useEffect(() => {
    if (seconds === null || seconds <= 0) return

    const timeout = setTimeout(() => setSeconds(seconds - 1), 1000)
    return () => clearTimeout(timeout)
  }, [seconds])

  const restart = useCallback(() => setSeconds(from), [from])

  return { seconds, restart }
}
