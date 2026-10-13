import type userEvent from '@testing-library/user-event'

export async function fillInput(
  user: ReturnType<typeof userEvent.setup>,
  input: HTMLElement,
  value: string,
) {
  await user.clear(input)
  if (value) {
    // Treat fixture values as literal text, not user-event keyboard descriptors.
    await user.type(input, value.replace(/[{[]/g, '$&$&'))
  }
}
