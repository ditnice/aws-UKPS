import { cleanup, fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { FormQuestionDto, RecordPageDto } from '@/client/generated'
import { NextLinkMock } from '@/test-utils/nextMocks'

import { FormPage } from './FormPage'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  saveRecordPage: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mocks.push }),
}))

vi.mock('next/link', () => ({ default: NextLinkMock }))

vi.mock('@/client/generated', async (importOriginal) => {
  const original = await importOriginal<object>()
  return { ...original, saveRecordPage: mocks.saveRecordPage }
})

const indication: FormQuestionDto = {
  id: 'medicines_product_detail.indication',
  type: 'Textarea',
  label: 'What is the indication this product is seeking a licence for?',
  hint: 'First paragraph.\n\nSecond paragraph.',
  rules: [
    { kind: 'Required', message: 'Enter the indication' },
    { kind: 'MaxLength', value: 10, message: 'Indication must be 10 characters or fewer' },
  ],
}

const bnfChapter: FormQuestionDto = {
  id: 'medicines_product_detail.bnf_chapter_id',
  type: 'Select',
  label: 'Select the BNF chapter for this product',
  rules: [{ kind: 'Required', message: 'Select a BNF chapter' }],
  options: [
    { value: '1', label: '1: Gastro-intestinal system' },
    { value: '2', label: '2: Cardiovascular system' },
  ],
}

const cancer: FormQuestionDto = {
  id: 'medicines_product_detail.indication_is_cancer',
  type: 'Radio',
  label: 'Is this a product intended to treat cancer?',
  rules: [{ kind: 'Required', message: 'Select whether this product is intended to treat cancer' }],
  options: [
    { value: 'Yes', label: 'Yes' },
    { value: 'No', label: 'No' },
    { value: 'Unknown', label: 'Unknown at this stage' },
  ],
}

const therapeuticArea: FormQuestionDto = {
  id: 'medicines_product_detail_therapeutic_area',
  type: 'Checkbox',
  display: 'combobox',
  label: 'Select the most appropriate therapeutic area for this product (optional)',
  rules: [{ kind: 'MaxItems', value: 1, message: 'Select up to 1 therapeutic area' }],
  options: [
    { value: '4', label: '1: Addiction' },
    { value: '7', label: '2: Allergy' },
  ],
}

const createPage = (overrides: Partial<RecordPageDto> = {}): RecordPageDto => ({
  formVersion: '2026.10.1',
  revisionVersion: 4711,
  readOnly: false,
  organisationId: 3,
  section: { id: 'indication-details', title: 'Indication details' },
  page: { id: 'indication', title: 'Indication' },
  previousPageId: null,
  questions: [indication],
  answers: { [indication.id]: null },
  context: {},
  ...overrides,
})

const renderPage = (page: RecordPageDto = createPage()) =>
  render(<FormPage page={page} recordId={5} revisionId={7} />)

const submit = () => fireEvent.click(screen.getByRole('button', { name: 'Save and continue' }))

const savedBody = () =>
  (mocks.saveRecordPage.mock.calls[0]?.[0] as { body: unknown; path: unknown }) ?? {}

beforeEach(() => {
  mocks.saveRecordPage.mockResolvedValue({
    data: { nextPageId: 'bnf-chapter' },
    response: { ok: true, status: 200 },
  })
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('FormPage', () => {
  describe('layout', () => {
    it('uses the question as the heading and the section as the caption', () => {
      renderPage()

      // NDS renders the caption inside the h1.
      const heading = screen.getByRole('heading', { level: 1 })
      expect(heading.textContent).toBe(`Indication details${indication.label}`)
      expect(within(heading).getByText('Indication details')).toBeTruthy()
    })

    it('renders hint paragraphs', () => {
      renderPage()

      expect(screen.getByText('First paragraph.')).toBeTruthy()
      expect(screen.getByText('Second paragraph.')).toBeTruthy()
    })

    it('links Back and Return to tasklist to the records list on the first page', () => {
      renderPage()

      expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
        '/portal/organisations/3/records',
      )
      expect(screen.getByRole('link', { name: 'Return to tasklist' }).getAttribute('href')).toBe(
        '/portal/organisations/3/records',
      )
    })

    it('links Back to the previous page', () => {
      renderPage(createPage({ previousPageId: 'paediatric' }))

      expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
        '/portal/records/5/revisions/7/paediatric',
      )
    })
  })

  describe('question types', () => {
    it('renders a textarea with its saved answer', () => {
      renderPage(createPage({ answers: { [indication.id]: 'Hepatitis C' } }))

      expect((screen.getByLabelText(indication.label) as HTMLTextAreaElement).value).toBe(
        'Hepatitis C',
      )
    })

    it('renders a select with its options and saved answer', () => {
      renderPage(createPage({ questions: [bnfChapter], answers: { [bnfChapter.id]: '2' } }))

      const select = screen.getByLabelText(bnfChapter.label) as HTMLSelectElement
      expect(select.value).toBe('2')
      expect(
        within(select)
          .getAllByRole('option')
          .map((o) => o.textContent),
      ).toEqual(['Choose an option', '1: Gastro-intestinal system', '2: Cardiovascular system'])
    })

    it('renders radios with the saved answer checked', () => {
      renderPage(createPage({ questions: [cancer], answers: { [cancer.id]: 'No' } }))

      expect((screen.getByLabelText('No') as HTMLInputElement).checked).toBe(true)
      expect((screen.getByLabelText('Yes') as HTMLInputElement).checked).toBe(false)
    })

    it('renders a combobox question as checkboxes until the combobox is built', () => {
      renderPage(
        createPage({ questions: [therapeuticArea], answers: { [therapeuticArea.id]: ['7'] } }),
      )

      expect((screen.getByLabelText('2: Allergy') as HTMLInputElement).checked).toBe(true)
      expect((screen.getByLabelText('1: Addiction') as HTMLInputElement).checked).toBe(false)
    })
  })

  describe('validation', () => {
    it('shows the first failing rule in the field and the error summary, and does not save', async () => {
      renderPage()

      submit()

      await waitFor(() => {
        expect(screen.getByRole('alert').textContent).toContain('There is a problem')
      })
      const summaryLink = within(screen.getByRole('alert')).getByRole('link', {
        name: 'Enter the indication',
      })
      expect(summaryLink.getAttribute('href')).toBe('#medicines_product_detail.indication')
      expect(screen.getAllByText('Enter the indication')).toHaveLength(2)
      expect(mocks.saveRecordPage).not.toHaveBeenCalled()
    })

    it('applies rules after trimming', async () => {
      renderPage()
      fireEvent.change(screen.getByLabelText(indication.label), {
        target: { value: '   abcdefghij   ' },
      })

      submit()

      await waitFor(() => expect(mocks.saveRecordPage).toHaveBeenCalled())
    })

    it('links radio errors to the first option', async () => {
      renderPage(createPage({ questions: [cancer], answers: { [cancer.id]: null } }))

      submit()

      await waitFor(() => {
        expect(
          within(screen.getByRole('alert'))
            .getByRole('link', { name: cancer.rules[0]!.message })
            .getAttribute('href'),
        ).toBe('#medicines_product_detail.indication_is_cancer_Yes')
      })
    })

    it('enforces max items on checkboxes', async () => {
      renderPage(
        createPage({ questions: [therapeuticArea], answers: { [therapeuticArea.id]: ['4'] } }),
      )
      fireEvent.click(screen.getByLabelText('2: Allergy'))

      submit()

      await waitFor(() => {
        expect(screen.getAllByText('Select up to 1 therapeutic area').length).toBeGreaterThan(0)
      })
      expect(mocks.saveRecordPage).not.toHaveBeenCalled()
    })
  })

  describe('saving', () => {
    it('sends every answer keyed by question ID with the loaded versions', async () => {
      renderPage(
        createPage({
          questions: [indication, cancer, therapeuticArea],
          answers: { [indication.id]: 'Old', [cancer.id]: null, [therapeuticArea.id]: [] },
        }),
      )
      fireEvent.change(screen.getByLabelText(indication.label), {
        target: { value: '  Hep C  ' },
      })
      fireEvent.click(screen.getByLabelText('Yes'))

      submit()

      await waitFor(() => expect(mocks.saveRecordPage).toHaveBeenCalled())
      expect(savedBody()).toEqual({
        path: { recordId: 5, revisionId: 7, pageId: 'indication' },
        body: {
          formVersion: '2026.10.1',
          revisionVersion: 4711,
          answers: {
            [indication.id]: 'Hep C',
            [cancer.id]: 'Yes',
            [therapeuticArea.id]: [],
          },
        },
      })
    })

    it('goes to the next page after saving', async () => {
      renderPage(createPage({ answers: { [indication.id]: 'Hep C' } }))

      submit()

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledWith('/portal/records/5/revisions/7/bnf-chapter')
      })
    })

    it('goes to the records list after the last page', async () => {
      mocks.saveRecordPage.mockResolvedValue({
        data: { nextPageId: null },
        response: { ok: true, status: 200 },
      })
      renderPage(createPage({ answers: { [indication.id]: 'Hep C' } }))

      submit()

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledWith('/portal/organisations/3/records')
      })
    })

    it('shows server validation errors against their fields', async () => {
      mocks.saveRecordPage.mockResolvedValue({
        error: { errors: { [indication.id]: ['Server says no'] } },
        response: { ok: false, status: 400 },
      })
      renderPage(createPage({ answers: { [indication.id]: 'Hep C' } }))

      submit()

      await waitFor(() => {
        expect(screen.getAllByText('Server says no')).toHaveLength(2)
      })
      expect(mocks.push).not.toHaveBeenCalled()
    })

    it('asks the user to reload when the page has changed', async () => {
      mocks.saveRecordPage.mockResolvedValue({
        error: { code: 'revision_changed' },
        response: { ok: false, status: 409 },
      })
      renderPage(createPage({ answers: { [indication.id]: 'Hep C' } }))

      submit()

      await waitFor(() => {
        expect(screen.getByText(/This page has changed since you opened it/)).toBeTruthy()
      })
      expect(screen.getByRole('link', { name: 'Reload the page' }).getAttribute('href')).toBe(
        '/portal/records/5/revisions/7/indication',
      )
      expect(mocks.saveRecordPage).toHaveBeenCalledTimes(1)
    })

    it('shows a generic error when saving fails', async () => {
      mocks.saveRecordPage.mockResolvedValue({ response: { ok: false, status: 500 } })
      renderPage(createPage({ answers: { [indication.id]: 'Hep C' } }))

      submit()

      await waitFor(() => {
        expect(
          screen.getByText('There was a problem saving your answers. Please try again.'),
        ).toBeTruthy()
      })
    })
  })

  describe('read only', () => {
    it('disables inputs and hides Save', () => {
      renderPage(createPage({ readOnly: true }))

      expect((screen.getByLabelText(indication.label) as HTMLTextAreaElement).disabled).toBe(true)
      expect(screen.queryByRole('button', { name: 'Save and continue' })).toBeNull()
      expect(screen.getByText('You can view this record but you cannot change it.')).toBeTruthy()
    })
  })
})
