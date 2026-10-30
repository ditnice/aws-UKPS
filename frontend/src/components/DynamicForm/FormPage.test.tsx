import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { RecordPageDto, RecordPageQuestionDto } from '@/client/generated'

import { FormPage } from './FormPage'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  saveRecordFormPage: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mocks.push }),
}))

vi.mock('@/client/generated', async (importOriginal) => {
  const original = await importOriginal<typeof import('@/client/generated')>()
  return { ...original, saveRecordFormPage: mocks.saveRecordFormPage }
})

const question = (overrides: Partial<RecordPageQuestionDto>): RecordPageQuestionDto => ({
  id: 'medicines_indication_detail.indication',
  type: 'Textarea',
  multiple: false,
  label: 'What is the indication this product is seeking a licence for?',
  hint: 'Describe the indication.',
  display: null,
  placeholder: null,
  childLabel: null,
  childPlaceholder: null,
  addAnotherLabel: null,
  rules: [{ kind: 'Required', value: null, message: 'Enter the indication' }],
  options: null,
  ...overrides,
})

const tree = [
  {
    value: '1',
    label: '1: Addiction',
    children: [
      { value: '11', label: '1.1: Opioid use disorder', children: null },
      { value: '13', label: '1.3: Smoking cessation', children: null },
    ],
  },
  { value: '2', label: '2: Allergy', children: null },
]

const page = (overrides: Partial<RecordPageDto> = {}): RecordPageDto => ({
  formVersion: '2026.10.1',
  revisionVersion: 4711,
  recordId: 5,
  section: { id: 'indication-details', title: 'Indication details' },
  page: { id: 'indication', title: 'Indication' },
  previousPageId: null,
  nextPageId: 'bnf-chapter',
  questions: [question({})],
  answers: { 'medicines_indication_detail.indication': null },
  ...overrides,
})

const areasQuestion = question({
  id: 'areas',
  type: 'HierarchicalSelect',
  multiple: true,
  label: 'Select the most appropriate therapeutic area for this product (optional)',
  placeholder: 'Choose a therapeutic area',
  childLabel: 'Select the disease or condition (optional)',
  childPlaceholder: 'Choose a disease or condition',
  addAnotherLabel: 'Add another therapy area',
  rules: [{ kind: 'MaxItems', value: 2, message: 'Select up to 2 therapeutic areas' }],
  options: tree,
})

const save = () => fireEvent.click(screen.getByRole('button', { name: 'Save and continue' }))

beforeEach(() => {
  mocks.saveRecordFormPage.mockResolvedValue({
    data: { nextPageId: 'bnf-chapter' },
    response: { ok: true, status: 200 },
  })
})

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})

describe('FormPage', () => {
  describe('rendering', () => {
    it('uses the question label as the heading of a single-question page', () => {
      render(<FormPage page={page()} />)

      expect(
        // The NDS page header puts the section caption inside the h1.
        screen.getByRole('heading', {
          level: 1,
          name: /What is the indication this product is seeking a licence for\?$/,
        }),
      ).toBeInTheDocument()
      expect(screen.getByText('Indication details')).toBeInTheDocument()
      expect(screen.getByRole('link', { name: 'Return to tasklist' })).toBeInTheDocument()
    })

    it('goes back to the task list from the first page of a task', () => {
      render(<FormPage page={page()} />)

      expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
        'href',
        '/portal/records/5/tasks',
      )
    })

    it('labels the button by whether the task has another page', () => {
      render(<FormPage page={page()} />)
      expect(screen.getByRole('button', { name: 'Save and continue' })).toBeInTheDocument()

      cleanup()
      render(<FormPage page={page({ nextPageId: null })} />)
      expect(
        screen.getByRole('button', { name: 'Save and return to task list' }),
      ).toBeInTheDocument()
    })

    it('keeps a hidden label on the input of a single-question page', () => {
      render(<FormPage page={page({ questions: [question({ type: 'Text' })] })} />)

      expect(
        screen.getByLabelText('What is the indication this product is seeking a licence for?'),
      ).toBe(screen.getByRole('textbox'))
    })

    it('uses the page title as the heading of a page with several questions', () => {
      render(
        <FormPage
          page={page({
            previousPageId: 'indication',
            questions: [question({}), question({ id: 'other', type: 'Text', label: 'Other' })],
          })}
        />,
      )

      expect(screen.getByRole('heading', { level: 1, name: /Indication$/ })).toBeInTheDocument()
      expect(screen.getByLabelText('Other')).toBeInTheDocument()
      expect(screen.getByRole('link', { name: 'Back' }).getAttribute('href')).toBe(
        '/portal/records/5/indication',
      )
    })

    it('fills in saved answers', () => {
      render(
        <FormPage
          page={page({ answers: { 'medicines_indication_detail.indication': 'Saved' } })}
        />,
      )

      expect(screen.getByRole('textbox')).toHaveValue('Saved')
    })
  })

  describe('saving', () => {
    it('sends every answer with the versions and goes to the next page', async () => {
      render(<FormPage page={page()} />)

      fireEvent.change(screen.getByRole('textbox'), { target: { value: 'Hepatitis C' } })
      save()

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledWith('/portal/records/5/bnf-chapter')
      })
      expect(mocks.saveRecordFormPage).toHaveBeenCalledWith({
        path: { recordId: 5, pageId: 'indication' },
        body: {
          formVersion: '2026.10.1',
          revisionVersion: 4711,
          answers: { 'medicines_indication_detail.indication': 'Hepatitis C' },
        },
      })
    })

    it('returns to the task list after the last page of a task', async () => {
      mocks.saveRecordFormPage.mockResolvedValue({
        data: { nextPageId: null },
        response: { ok: true, status: 200 },
      })
      render(
        <FormPage
          page={page({
            nextPageId: null,
            answers: { 'medicines_indication_detail.indication': 'x' },
          })}
        />,
      )

      fireEvent.click(screen.getByRole('button', { name: 'Save and return to task list' }))

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledWith('/portal/records/5/tasks')
      })
    })

    it('shows client validation errors against the field', async () => {
      render(<FormPage page={page()} />)

      save()

      expect(await screen.findByText('Enter the indication')).toBeInTheDocument()
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('shows server validation errors, including keys not on the page', async () => {
      mocks.saveRecordFormPage.mockResolvedValue({
        error: {
          errors: {
            'medicines_indication_detail.indication': ['Server says no'],
            stray: ['This is not a question on this page.'],
          },
        },
        response: { ok: false, status: 400 },
      })
      render(
        <FormPage page={page({ answers: { 'medicines_indication_detail.indication': 'x' } })} />,
      )

      save()

      expect(await screen.findByText('Server says no')).toBeInTheDocument()
      expect(screen.getByRole('alert')).toHaveTextContent('This is not a question on this page.')
      expect(mocks.push).not.toHaveBeenCalled()
    })

    it('offers a reload when the page has changed since it was loaded', async () => {
      mocks.saveRecordFormPage.mockResolvedValue({
        error: {
          code: 'revision_changed',
          detail: 'This record has changed since the page was loaded.',
        },
        response: { ok: false, status: 409 },
      })
      render(
        <FormPage page={page({ answers: { 'medicines_indication_detail.indication': 'x' } })} />,
      )

      save()

      expect(
        await screen.findByText('This record has changed since the page was loaded.'),
      ).toBeInTheDocument()
      expect(screen.getByRole('link', { name: 'Reload this page' })).toHaveAttribute(
        'href',
        '/portal/records/5/indication',
      )
    })

    it('shows a general error for other failures', async () => {
      mocks.saveRecordFormPage.mockResolvedValue({
        error: { detail: 'Forbidden' },
        response: { ok: false, status: 403 },
      })
      render(
        <FormPage page={page({ answers: { 'medicines_indication_detail.indication': 'x' } })} />,
      )

      save()

      expect(
        await screen.findByText('There was a problem saving your answers. Please try again later.'),
      ).toBeInTheDocument()
    })
  })

  describe('controls', () => {
    it('saves the chosen radio option', async () => {
      render(
        <FormPage
          page={page({
            page: { id: 'cancer', title: 'Treating cancer' },
            questions: [
              question({
                id: 'cancer',
                type: 'Radio',
                label: 'Is this a product intended to treat cancer?',
                hint: null,
                rules: [],
                options: [
                  { value: 'Yes', label: 'Yes', children: null },
                  { value: 'No', label: 'No', children: null },
                ],
              }),
            ],
            answers: { cancer: null },
          })}
        />,
      )

      fireEvent.click(screen.getByLabelText('No'))
      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalled())
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({ cancer: 'No' })
    })

    it('saves ticked checkboxes', async () => {
      render(
        <FormPage
          page={page({
            questions: [
              question({ id: 'ticks', type: 'Checkbox', multiple: true, rules: [], options: tree }),
            ],
            answers: { ticks: ['1'] },
          })}
        />,
      )

      expect(screen.getByLabelText('1: Addiction')).toBeChecked()
      fireEvent.click(screen.getByLabelText('1: Addiction'))
      fireEvent.click(screen.getByLabelText('2: Allergy'))
      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalled())
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({ ticks: ['2'] })
    })

    it('saves a plain select and a text input', async () => {
      render(
        <FormPage
          page={page({
            questions: [
              question({
                id: 'choice',
                type: 'Select',
                label: 'Choice',
                rules: [],
                options: tree,
              }),
              question({ id: 'name', type: 'Text', label: 'Name', rules: [] }),
            ],
            answers: { choice: null, name: null },
          })}
        />,
      )

      fireEvent.change(screen.getByLabelText('Choice'), { target: { value: '2' } })
      fireEvent.change(screen.getByLabelText('Name'), { target: { value: 'A name' } })
      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalled())
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({
        choice: '2',
        name: 'A name',
      })
    })

    it('shows the child select once a parent with children is chosen, and saves the child', async () => {
      render(
        <FormPage
          page={page({
            page: { id: 'bnf-chapter', title: 'BNF chapter' },
            questions: [
              question({
                id: 'bnf',
                type: 'HierarchicalSelect',
                label: 'Select the BNF chapter for this product',
                hint: null,
                placeholder: 'Choose BNF chapter',
                childLabel: 'Select the BNF subchapter (optional)',
                childPlaceholder: 'Choose BNF subchapter',
                rules: [{ kind: 'Required', value: null, message: 'Choose a BNF chapter' }],
                options: tree,
              }),
            ],
            answers: { bnf: null },
          })}
        />,
      )

      expect(screen.queryByLabelText('Select the BNF subchapter (optional)')).toBeNull()
      fireEvent.change(screen.getByLabelText('Select the BNF chapter for this product'), {
        target: { value: '1' },
      })
      fireEvent.change(screen.getByLabelText('Select the BNF subchapter (optional)'), {
        target: { value: '13' },
      })
      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalled())
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({ bnf: '13' })
    })

    it('clears a parent error before showing the child select', async () => {
      render(
        <FormPage
          page={page({
            page: { id: 'bnf-chapter', title: 'BNF chapter' },
            questions: [
              question({
                id: 'bnf',
                type: 'HierarchicalSelect',
                label: 'Select the BNF chapter for this product',
                hint: null,
                childLabel: 'Select the BNF subchapter (optional)',
                rules: [{ kind: 'Required', value: null, message: 'Choose a BNF chapter' }],
                options: tree,
              }),
            ],
            answers: { bnf: null },
          })}
        />,
      )

      save()
      expect(await screen.findByText('Choose a BNF chapter')).toBeInTheDocument()

      fireEvent.change(screen.getByLabelText('Select the BNF chapter for this product'), {
        target: { value: '1' },
      })

      await waitFor(() => expect(screen.queryByText('Choose a BNF chapter')).toBeNull())
      const child = screen.getByLabelText('Select the BNF subchapter (optional)')
      expect(child.getAttribute('aria-invalid')).toBeNull()
      expect(child.className).not.toMatch(/fieldError/)
    })

    it('describes every hierarchical select with the question hint', () => {
      render(
        <FormPage
          page={page({
            questions: [{ ...areasQuestion, hint: 'Select up to 2 therapeutic areas.' }],
            answers: { areas: ['13', '2'] },
          })}
        />,
      )

      const hint = screen.getByText('Select up to 2 therapeutic areas.')
      for (const select of screen.getAllByRole('combobox')) {
        expect(select.getAttribute('aria-describedby')?.split(' ')).toContain(hint.id)
      }
    })

    it('loads a saved child into its parent and child selects', () => {
      render(
        <FormPage
          page={page({
            questions: [areasQuestion],
            answers: { areas: ['13', '2'] },
          })}
        />,
      )

      const parents = screen.getAllByRole('combobox')
      expect(parents[0]).toHaveValue('1')
      expect(parents[1]).toHaveValue('13')
      expect(parents[2]).toHaveValue('2')
    })

    it('adds and removes entries of a multi-value hierarchical select, up to MaxItems', async () => {
      render(<FormPage page={page({ questions: [areasQuestion], answers: { areas: [] } })} />)

      fireEvent.change(screen.getAllByRole('combobox')[0], { target: { value: '2' } })
      fireEvent.click(screen.getByRole('button', { name: 'Add another therapy area' }))
      expect(screen.queryByRole('button', { name: 'Add another therapy area' })).toBeNull()

      const second = screen.getAllByRole('combobox')[1]
      fireEvent.change(second, { target: { value: '1' } })
      fireEvent.change(screen.getAllByRole('combobox')[2], { target: { value: '11' } })
      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({
        areas: ['2', '11'],
      })

      fireEvent.click(screen.getByRole('button', { name: /^Remove\s*entry 2$/ }))
      expect(screen.getAllByRole('combobox')).toHaveLength(1)
    })

    it('reports the same area chosen twice', async () => {
      render(
        <FormPage page={page({ questions: [areasQuestion], answers: { areas: ['2', '2'] } })} />,
      )

      save()

      expect(
        await screen.findAllByText('You have selected the same option more than once.'),
      ).not.toHaveLength(0)
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })
  })
})
