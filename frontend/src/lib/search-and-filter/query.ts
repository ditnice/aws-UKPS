export function parseMulti<T extends string>(
  param: string | string[] | undefined,
  validValues: readonly T[],
): T[] {
  const values = Array.isArray(param) ? param : param ? [param] : []

  return values.filter((value): value is T => validValues.includes(value as T))
}
