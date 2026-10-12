import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { CreateRecordCommand } from '@/client/generated'
import { errorMessages } from '@/lib/form/errorMessages'

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
  companyCode: 'cc1',
  brandedName: 'test',
  genericNames: ['gn1', 'gn2', 'gn3'],
  otherIdentifiers: ['oi1', 'oi2'],
  recordTitle: 'record-title',
}

const otherIdentifiersLabel = 'Other names and identifiers (optional)'

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
  setFieldValue('Company code', formValues.companyCode)

  setFieldValue('Branded name (optional)', formValues.brandedName ?? '')

  fillArrayField('Add another active substance', 'Generic name', formValues.genericNames)

  fillArrayField('Add another identifier', otherIdentifiersLabel, formValues.otherIdentifiers)

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

    it('renders the default company code field', () => {
      renderComponent()

      expect(screen.getByLabelText('Company code')).toBeDefined()
    })

    it('renders the default generic name field', () => {
      renderComponent()

      expect(screen.getByLabelText('Generic name')).toBeDefined()
    })

    it('renders the branded name field', () => {
      renderComponent()

      expect(screen.getByLabelText('Branded name (optional)')).toBeDefined()
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

  describe('company code', () => {
    it('requires a company code', async () => {
      renderComponent()

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText(errorMessages.companyCodeRequired)).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })

    it('rejects a whitespace-only company code', async () => {
      renderComponent()
      fillInForm({ ...validFormValues, companyCode: '   ' })

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText(errorMessages.companyCodeRequired)).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })
  })

  describe('generic names', () => {
    it('allows additional generic names to be added', () => {
      renderComponent()

      fireEvent.click(screen.getByText('Add another active substance'))

      expect(screen.getByLabelText('Generic name')).toBeDefined()
      expect(screen.getByLabelText('Generic name 2')).toBeDefined()
    })

    it('allows additional generic names to be removed', () => {
      renderComponent()

      fireEvent.click(screen.getByText('Add another active substance'))

      expect(screen.getByLabelText('Generic name 2')).toBeDefined()

      fireEvent.click(screen.getByText('Remove active substance'))

      expect(screen.queryByLabelText('Generic name 2')).toBeNull()
    })

    it('requires a generic name', async () => {
      renderComponent()

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText(errorMessages.genericNameRequired)).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })

    it('rejects duplicate generic names that differ only by case', async () => {
      renderComponent()

      fillArrayField('Add another active substance', 'Generic name', ['duplicate', 'DUPLICATE'])

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText(errorMessages.genericNamesDistinct)).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })

    it('rejects duplicate generic names', async () => {
      renderComponent()

      fillArrayField('Add another active substance', 'Generic name', ['duplicate', 'duplicate'])

      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText(errorMessages.genericNamesDistinct)).toBeDefined()
      })

      expect(mocks.createRecord).not.toHaveBeenCalled()
    })
  })

  describe('other names and identifiers', () => {
    it('renders an empty other names and identifiers field', () => {
      renderComponent()

      expect((screen.getByLabelText(otherIdentifiersLabel) as HTMLInputElement).value).toBe('')
    })

    it('allows additional other names and identifiers to be added and removed', () => {
      renderComponent()

      fireEvent.click(screen.getByText('Add another identifier'))

      expect(screen.getByLabelText(`${otherIdentifiersLabel} 2`)).toBeDefined()

      fireEvent.click(screen.getByText('Remove identifier'))

      expect(screen.queryByLabelText(`${otherIdentifiersLabel} 2`)).toBeNull()
    })

    it('sends no other names and identifiers when left blank', async () => {
      renderComponent(123)

      fillInForm({ ...validFormValues, otherIdentifiers: [] })
      fireEvent.click(screen.getByText('Add another identifier'))
      setFieldValue(`${otherIdentifiersLabel} 2`, '   ')
      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.createRecord).toHaveBeenCalled()
      })

      expect(mocks.createRecord).toHaveBeenCalledWith({
        body: { organisationId: 123, ...validFormValues, otherIdentifiers: [] },
      })
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
        expect(screen.getByText(errorMessages.recordTitleRequired)).toBeDefined()
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
        expect(screen.getByText(errorMessages.recordTitleTooLong)).toBeDefined()
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

    it('sends a null branded name when the branded name is left empty', async () => {
      renderComponent(123)

      fillInForm({ ...validFormValues, brandedName: '   ' })
      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.createRecord).toHaveBeenCalled()
      })

      expect(mocks.createRecord).toHaveBeenCalledWith({
        body: {
          organisationId: 123,
          ...validFormValues,
          brandedName: null,
        },
      })
    })

    it('sends trimmed values to the API', async () => {
      renderComponent(123)

      fillInForm({
        companyCode: '  cc1  ',
        otherIdentifiers: ['  oi1  '],
        brandedName: '  branded  ',
        genericNames: ['  gn1  '],
        recordTitle: '  record-title  ',
      })
      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.createRecord).toHaveBeenCalled()
      })

      expect(mocks.createRecord).toHaveBeenCalledWith({
        body: {
          organisationId: 123,
          companyCode: 'cc1',
          otherIdentifiers: ['oi1'],
          brandedName: 'branded',
          genericNames: ['gn1'],
          recordTitle: 'record-title',
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

    it('does not show an error after successful submission', async () => {
      renderComponent(123)

      fillInForm(validFormValues)
      clickSubmitButton()

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalled()
      })

      expect(screen.queryByText(errorMessages.creatingNewRecordError)).toBeNull()
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

    it('disables the submit button while the request is in progress', async () => {
      let resolveRequest: (value: unknown) => void = () => {}
      mocks.createRecord.mockReturnValue(
        new Promise((resolve) => {
          resolveRequest = resolve
        }),
      )

      renderComponent()

      fillInForm(validFormValues)
      clickSubmitButton()

      await waitFor(() => {
        expect(
          screen.getByRole('button', { name: 'Save and continue' }).hasAttribute('disabled'),
        ).toBe(true)
      })

      resolveRequest({ error: undefined, response: { ok: true } })

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalled()
      })
    })

    it('re-enables the submit button when the API request fails', async () => {
      mocks.createRecord.mockResolvedValue({
        error: {},
        response: { ok: false },
      })

      renderComponent()

      fillInForm(validFormValues)
      clickSubmitButton()

      await waitFor(() => {
        expect(screen.getByText(errorMessages.creatingNewRecordError)).toBeDefined()
      })

      expect(
        screen.getByRole('button', { name: 'Save and continue' }).hasAttribute('disabled'),
      ).toBe(false)
    })

    it.each([
      ['company code', 'CompanyCode', 'Company code'],
      ['generic names', 'GenericNames', 'Generic name'],
      ['other names and identifiers', 'OtherIdentifiers', otherIdentifiersLabel],
    ])(
      'allows resubmission after correcting %s rejected by the API',
      async (_, errorKey, label) => {
        const apiErrorMessage = 'The API rejected these names'
        mocks.createRecord.mockResolvedValueOnce({
          error: { title: 'Validation failed', errors: { [errorKey]: [apiErrorMessage] } },
          response: { ok: false },
        })

        renderComponent()

        fillInForm(validFormValues)
        clickSubmitButton()

        await waitFor(() => {
          expect(screen.getByText(apiErrorMessage)).toBeDefined()
        })

        setFieldValue(label, 'corrected')

        await waitFor(() => {
          expect(screen.queryByText(apiErrorMessage)).toBeNull()
        })

        clickSubmitButton()

        await waitFor(() => {
          expect(mocks.createRecord).toHaveBeenCalledTimes(2)
        })
      },
    )

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
