import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { CreateRecordCommand } from '@/client/generated'

import CreateMedicineRecordForm from './CreateMedicineRecordForm'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  createRecord: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({
    push: mocks.push,
  }),
}))

vi.mock('@/client/generated', async (importOriginal) => {
  const original = await importOriginal
  return { ...original, createRecord: mocks.createRecord }
})

const renderComponent = (organisationId = 1) => {
  render(<CreateMedicineRecordForm organisationId={organisationId} />)
}

type FormValues = Omit<CreateRecordCommand, 'organisationId'>

const validFormValues: FormValues = {
  developmentNames: ['dn1', 'dn2', 'dn3'],
  brandedName: 'test',
  genericNames: ['gn1', 'gn2', 'gn3'],
  recordTitle: 'record-title',
}

const setFieldValue = (label: string, value: string) => {
  fireEvent.change(screen.getByLabelText(label), {
    target: { value },
  })
}

const fillArrayField = (addButtonLabel: string, labelPrefix: string, values: string[]) => {
  values.forEach((value, index) => {
    const label = index === 0 ? labelPrefix : `${labelPrefix} ${index + 1}`

    if (!screen.queryByLabelText(label)) {
      fireEvent.click(screen.getByText(addButtonLabel))
    }

    setFieldValue(label, value)
  })
}

const fillInForm = (formValues: FormValues) => {
  fillArrayField('Add additional development name', 'Development name', formValues.developmentNames)

  setFieldValue('Branded name (Optional)', formValues.brandedName ?? '')

  fillArrayField('Add additional active substance', 'Generic name', formValues.genericNames)

  setFieldValue('Record title', formValues.recordTitle)
}

const clickSubmitButton = () => {
  fireEvent.click(screen.getByRole('button', { name: 'Save and continue' }))
}

beforeEach(() => {
  mocks.createRecord.mockResolvedValue({
    error: undefined,
    response: { ok: true },
  })
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('CreateMedicineRecordForm', () => {
  describe('rendering', () => {
    it('renders the form', () => {
      renderComponent()

      expect(screen.getByRole('button', { name: 'Save and continue' })).toBeDefined()
    })

    it('renders the default development name field', () => {
      renderComponent()

      expect(screen.getByLabelText('Development name')).toBeDefined()
    })

    it('renders the default generic name field', () => {
      renderComponent()

      expect(screen.getByLabelText('Generic name')).toBeDefined()
    })

    it('renders the branded name field', () => {
      renderComponent()

      expect(screen.getByLabelText('Branded name (Optional)')).toBeDefined()
    })

    it('renders the record title field', () => {
      renderComponent()

      expect(screen.getByLabelText('Record title')).toBeDefined()
    })

    it('does not show an error initially', () => {
      renderComponent()

      expect(screen.queryByText(/unable to create/i)).toBeNull()
    })
  })

  describe('development names', () => {
    it('allows additional development names to be added', () => {
      renderComponent()

      fireEvent.click(screen.getByText('Add additional development name'))

      expect(screen.getByLabelText('Development name')).toBeDefined()
      expect(screen.getByLabelText('Development name 2')).toBeDefined()
    })

    it('allows additional development names to be removed', () => {
      renderComponent()

      fireEvent.click(screen.getByText('Add additional development name'))

      expect(screen.getByLabelText('Development name 2')).toBeDefined()

      fireEvent.click(screen.getByText('Remove Development Name'))

      expect(screen.queryByLabelText('Development name 2')).toBeNull()
    })

    it('requires a development name', async () => {
      renderComponent()

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText('Enter development name')).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })

    it('rejects duplicate development names', async () => {
      renderComponent()

      fillArrayField('Add additional development name', 'Development name', [
        'duplicate',
        'duplicate',
      ])

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText('Development names must be distinct')).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })
  })

  describe('generic names', () => {
    it('allows additional generic names to be added', () => {
      renderComponent()

      fireEvent.click(screen.getByText('Add additional active substance'))

      expect(screen.getByLabelText('Generic name')).toBeDefined()
      expect(screen.getByLabelText('Generic name 2')).toBeDefined()
    })

    it('allows additional generic names to be removed', () => {
      renderComponent()

      fireEvent.click(screen.getByText('Add additional active substance'))

      expect(screen.getByLabelText('Generic name 2')).toBeDefined()

      fireEvent.click(screen.getByText('Remove active substance'))

      expect(screen.queryByLabelText('Generic name 2')).toBeNull()
    })

    it('requires a generic name', async () => {
      renderComponent()

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText('Enter generic name')).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })

    it('rejects duplicate generic names', async () => {
      renderComponent()

      fillArrayField('Add additional active substance', 'Generic name', ['duplicate', 'duplicate'])

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText('Generic names must be distinct')).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })
  })

  describe('branded name', () => {
    it('allows the branded name to be empty', async () => {
      renderComponent()

      fillInForm({
        ...validFormValues,
        brandedName: '',
      })

      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.createRecord).toHaveBeenCalled()
      })
    })
  })

  describe('record title', () => {
    it('requires a record title', async () => {
      renderComponent()

      fillInForm({
        ...validFormValues,
        recordTitle: '',
      })

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText('Enter record title')).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })

    it('rejects a record title longer than 100 characters', async () => {
      renderComponent()

      fillInForm({
        ...validFormValues,
        recordTitle: 'a'.repeat(101),
      })

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText('Record title cannot be greater than 100 characters')).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })

    it('accepts a record title of exactly 100 characters', async () => {
      renderComponent()

      fillInForm({
        ...validFormValues,
        recordTitle: 'a'.repeat(100),
      })

      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.createRecord).toHaveBeenCalled()
      })
    })
  })

  describe('submission', () => {
    it('sends the entered values to the API', async () => {
      renderComponent(123)

      fillInForm(validFormValues)
      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.createRecord).toHaveBeenCalled()
      })

      expect(mocks.createRecord).toHaveBeenCalledWith({
        body: {
          organisationId: 123,
          ...validFormValues,
        },
      })
    })

    it('navigates to the records page after successful submission', async () => {
      renderComponent(123)

      fillInForm(validFormValues)
      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/123/records')
      })
    })

    it('does not navigate when the API request fails', async () => {
      mocks.createRecord.mockResolvedValue({
        error: {},
        response: { ok: false },
      })

      renderComponent(123)

      fillInForm(validFormValues)
      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.createRecord).toHaveBeenCalled()
      })

      expect(mocks.push).not.toHaveBeenCalled()
    })

    it('shows an error when the API request fails', async () => {
      mocks.createRecord.mockResolvedValue({
        error: {},
        response: { ok: false },
      })

      renderComponent()

      fillInForm(validFormValues)
      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText(/error/i)).toBeDefined()
      })
    })
  })
})
