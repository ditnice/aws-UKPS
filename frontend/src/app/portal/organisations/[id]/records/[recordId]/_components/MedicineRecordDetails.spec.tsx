import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it } from 'vitest'

import { PublishedRecordDtoPublishedMedicineRecordDto } from '@/client/generated'

import { MedicineRecordDetails } from './MedicineRecordDetails'

afterEach(cleanup)

const record: PublishedRecordDtoPublishedMedicineRecordDto = {
  recordType: 'Medicine',
  recordId: 42,
  organisationId: 7,
  recordStatus: 'Active',
  revisionId: 3,
  recordClinicalTrialInformation: { recruitingInUk: 'Yes' },
  recordClinicalTrials: [
    {
      studyName: 'Study A',
      clinicalTrialsGovNumber: 'NCT00000001',
      otherClinicalTrialNumbers: ['ISRCTN12345678'],
    },
  ],
}

// Accordions start collapsed, so their content is hidden from accessibility queries.
const rowValue = (label: string) =>
  within(screen.getByText(label).closest('div')!).getAllByRole('definition', { hidden: true })[0]
    .textContent

describe('MedicineRecordDetails', () => {
  it('shows each clinical trial and whether any are recruiting in the UK', () => {
    render(<MedicineRecordDetails record={record} />)

    expect(screen.getByRole('heading', { name: 'Trial 1', hidden: true })).toBeDefined()
    expect(rowValue('Study name')).toBe('Study A')
    expect(rowValue('ClinicalTrials.gov number')).toBe('NCT00000001')
    expect(rowValue('Trial number from other registry (optional)')).toBe('ISRCTN12345678')
    expect(rowValue('Any trials recruiting in the UK')).toBe('Yes')
  })

  it('does not show the vaccine-only trial questions', () => {
    render(<MedicineRecordDetails record={record} />)

    expect(screen.queryByText('Trial phase')).toBeNull()
    expect(screen.queryByText('Brief description')).toBeNull()
  })

  it('shows not provided when there are no trials or recruitment answer', () => {
    render(
      <MedicineRecordDetails
        record={{ ...record, recordClinicalTrials: [], recordClinicalTrialInformation: null }}
      />,
    )

    expect(rowValue('Clinical trials')).toBe('Not provided')
    expect(rowValue('Any trials recruiting in the UK')).toBe('Not provided')
  })
})
