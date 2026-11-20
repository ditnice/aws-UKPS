import type { GetSuggestions } from '@/components/Autocomplete/Autocomplete'
import { getCompanyCodeSuggestions } from '@/lib/form/companyCodeSuggestions'

/**
 * The prefix of the display hint that asks for a text question to suggest values as the user
 * types. The rest of the hint names the source, as in `autocomplete:company-codes`.
 */
const autocompleteDisplayPrefix = 'autocomplete:'

/** Where autocomplete questions get their suggestions, keyed by the source named in the hint. */
const suggestionSources: Partial<Record<string, (organisationId: number) => GetSuggestions>> = {
  'company-codes': getCompanyCodeSuggestions,
}

/**
 * The suggestions for a question whose display hint names a known autocomplete source, or
 * undefined otherwise, so it is shown as a plain text input.
 */
export const getSuggestionSource = (
  display: string | null | undefined,
  organisationId: number,
): GetSuggestions | undefined => {
  if (!display?.startsWith(autocompleteDisplayPrefix)) {
    return undefined
  }
  const source = display.slice(autocompleteDisplayPrefix.length)
  return suggestionSources[source]?.(organisationId)
}
