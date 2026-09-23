export const parsePositiveInteger = (value: string): number | null => {
  if (!/^[1-9]\d*$/.test(value)) {
    return null
  }

  const num = Number(value)

  if (!Number.isSafeInteger(num)) {
    return null
  }

  return num
}
