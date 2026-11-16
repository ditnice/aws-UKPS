import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { PharmaceuticalEntity } from '@/client/generated'

import { CreateRecordActionBanner } from './CreateRecordActionBanner'

describe('CreateMedicineRecordActionBanner', () => {
  const organisationId = '4'
  it('shows only the medicine button when the user has medicines permission', () => {
    render(
      <CreateRecordActionBanner
        organisationId={organisationId}
        allowedPharmaceuticalEntity={PharmaceuticalEntity.MEDICINES}
      />,
    )
    expect(screen.getByRole('link', { name: 'Create medicine record' })).toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Create vaccine record' })).not.toBeInTheDocument()
  })

  it('shows only the vaccine button when the user has vaccines permission', () => {
    render(
      <CreateRecordActionBanner
        organisationId={organisationId}
        allowedPharmaceuticalEntity={PharmaceuticalEntity.VACCINES}
      />,
    )
    expect(screen.queryByRole('link', { name: 'Create medicine record' })).not.toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Create vaccine record' })).toBeInTheDocument()
  })

  it('shows both buttons when the user has both permission', () => {
    render(
      <CreateRecordActionBanner
        organisationId={organisationId}
        allowedPharmaceuticalEntity={PharmaceuticalEntity.BOTH}
      />,
    )
    expect(screen.getByRole('link', { name: 'Create medicine record' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Create vaccine record' })).toBeInTheDocument()
  })

  it('shows neither button when the permission is undefined', () => {
    render(
      <CreateRecordActionBanner
        organisationId={organisationId}
        allowedPharmaceuticalEntity={undefined}
      />,
    )
    expect(screen.queryByRole('link', { name: 'Create medicine record' })).not.toBeInTheDocument()
    expect(screen.queryByRole('link', { name: 'Create vaccine record' })).not.toBeInTheDocument()
  })
})
