import { getOrganisationCompanyCodes } from '@/client/generated'
import type { GetSuggestions } from '@/components/Autocomplete/Autocomplete'

/** How many company codes to suggest at once. */
export const companyCodeSuggestionLimit = 10

/**
 * Suggests the company codes already used on the organisation's records that contain the query.
 * Suggestions are optional, so a failed request suggests nothing rather than showing an error.
 */
export const getCompanyCodeSuggestions =
  (organisationId: number): GetSuggestions =>
  async (query, signal) => {
    const { data } = await getOrganisationCompanyCodes({
      path: { organisationId },
      query: { Search: query, Limit: companyCodeSuggestionLimit },
      signal,
    })
    return data?.items ?? []
  }
