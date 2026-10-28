'use client'

import Link from 'next/link'
import { useState } from 'react'

import { updateCurrentOrganisation } from '@/client/generated'
import { Button, ButtonGroup } from '@/components/Button/Button'
import { ErrorState } from '@/components/Placeholder/ErrorState'

export type SwitchOrganisationControlsProps = {
  organisationId: number
  confirmHref: string
  cancelHref: string
}

export const SwitchOrganisationControls = ({
  organisationId,
  confirmHref,
  cancelHref,
}: SwitchOrganisationControlsProps) => {
  const [hasError, setHasError] = useState(false)
  const [loading, setLoading] = useState(false)

  const switchOrganisation = async () => {
    setHasError(false)
    setLoading(true)

    try {
      const result = await updateCurrentOrganisation({ body: { organisationId } })
      if (result.error) {
        setHasError(true)
        return
      }
      // A full page load so nothing from the previous organisation is kept, such as the header.
      globalThis.location.assign(confirmHref)
    } catch {
      setHasError(true)
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      {hasError && (
        <ErrorState data-testid="switch-organisation-error">
          An error occurred when attempting to switch organisation.
        </ErrorState>
      )}
      <ButtonGroup>
        <Button variant="cta" disabled={loading} onClick={switchOrganisation}>
          Confirm
        </Button>
        <Button elementType={Link} href={cancelHref} variant="secondary">
          Cancel
        </Button>
      </ButtonGroup>
    </>
  )
}
