'use client'

import { useState } from 'react'

import { OrganisationListDto, updateCurrentOrganisation } from '@/client/generated'
import { Button } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'
import { Table } from '@/components/Table/Table'
import { getOrganisationLandingHref } from '@/lib/auth/routing'

export type SelectOrganisationTableProps = {
  organisations: OrganisationListDto[]
  returnTo?: string
}

export const SelectOrganisationTable = ({
  organisations,
  returnTo,
}: SelectOrganisationTableProps) => {
  const [hasError, setHasError] = useState(false)
  const [loading, setLoading] = useState(false)

  const selectOrganisation = async (organisationId: number) => {
    setHasError(false)
    setLoading(true)

    try {
      const result = await updateCurrentOrganisation({ body: { organisationId } })
      if (result.error) {
        setHasError(true)
        return
      }
      // A full page load so nothing from the previous organisation is kept, such as the header.
      globalThis.location.assign(getOrganisationLandingHref(organisationId, returnTo))
    } catch {
      setHasError(true)
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      {hasError && (
        <ErrorState data-testid="organisation-selection-error">
          An error occurred when attempting to set the organisation you are managing.
        </ErrorState>
      )}
      <Table columnWidth="content">
        <thead>
          <tr>
            <th scope="col">Organisation</th>
            <th scope="col">Action</th>
          </tr>
        </thead>
        <tbody>
          {organisations.map((organisation) => (
            <tr key={organisation.id}>
              <td>{organisation.organisationName}</td>
              <td>
                <Button
                  variant="link"
                  disabled={loading}
                  onClick={() => selectOrganisation(organisation.id)}
                  aria-label={`Manage Organisation - ${organisation.organisationName}`}
                >
                  Manage
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </Table>
    </>
  )
}
