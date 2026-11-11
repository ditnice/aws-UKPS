import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
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
  showWhen: null,
  rules: [{ kind: 'Required', value: null, message: 'Enter the indication' }],
  options: null,
  ...overrides,
})

const tree = [
  {
    value: '1',
    label: '1: Addiction',
    hint: null,
    children: [
      { value: '11', label: '1.1: Opioid use disorder', hint: null, children: null },
      { value: '13', label: '1.3: Smoking cessation', hint: null, children: null },
    ],
  },
  { value: '2', label: '2: Allergy', hint: null, children: null },
]

const page = (overrides: Partial<RecordPageDto> = {}): RecordPageDto => ({
  formVersion: '2026.10.1',
  pageHash: 'page-hash',
  recordId: 5,
  section: { id: 'product-indication', title: 'Product indication' },
  task: { id: 'indication-details', title: 'Indication details' },
  page: { id: 'indication', title: 'Indication', commercialInConfidence: false },
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

const niceTaIdQuestion = question({
  id: 'record_hta.nice_ta_development_id',
  type: 'PrefixedNumber',
  label: 'Enter the NICE technology appraisal development ID for this product (optional)',
  hint: 'Select the ID format and enter the NICE technology appraisal ID.',
  placeholder: 'Choose ID type',
  prefixes: ['GID-TA', 'GID-HST'],
  rules: [],
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

const yesNo = [
  { value: 'Yes', label: 'Yes', hint: null, children: null },
  { value: 'No', label: 'No', hint: null, children: null },
  { value: 'Unknown', label: 'Unknown at this stage', hint: null, children: null },
]

const coMarketingPage = (revealedBy: string[] = ['Yes'], answers: Record<string, unknown> = {}) =>
  page({
    page: { id: 'co-marketing', title: 'Co-marketing', commercialInConfidence: false },
    questions: [
      question({
        id: 'co',
        type: 'Radio',
        label: 'Is this product co-marketed?',
        hint: null,
        rules: [{ kind: 'Required', value: null, message: 'Select whether it is co-marketed' }],
        options: yesNo,
      }),
      question({
        id: 'co_name',
        type: 'Text',
        label: 'Co-marketing company name',
        hint: null,
        showWhen: { kind: 'AnswerIn', questionId: 'co', values: revealedBy },
        rules: [{ kind: 'Required', value: null, message: 'Enter the company name' }],
      }),
    ],
    answers: { co: null, co_name: null, ...answers },
  })

describe('FormPage', () => {
  describe('rendering', () => {
    it('uses the question label as the heading of a single-question page', () => {
      render(<FormPage page={page()} />)

      expect(
        // The NDS page header puts the task caption inside the h1.
        screen.getByRole('heading', {
          level: 1,
          name: /What is the indication this product is seeking a licence for\?$/,
        }),
      ).toBeInTheDocument()
      expect(screen.getByRole('link', { name: 'Return to task list' })).toBeInTheDocument()
    })

    it('captions the page with the task title, not the section title', () => {
      render(<FormPage page={page()} />)

      expect(screen.getByText('Indication details')).toBeInTheDocument()
      expect(screen.queryByText('Product indication')).toBeNull()
    })

    it('shows the Commercial in confidence badge when the page is flagged', () => {
      render(
        <FormPage
          page={page({
            page: { id: 'indication', title: 'Indication', commercialInConfidence: true },
          })}
        />,
      )

      expect(screen.getByText('Commercial in confidence')).toBeInTheDocument()
    })

    it('does not show the badge by default', () => {
      render(<FormPage page={page()} />)

      expect(screen.queryByText('Commercial in confidence')).toBeNull()
    })

    it('goes back to the task list from the first page of a task', () => {
      render(<FormPage page={page()} />)

      expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
        'href',
        '/portal/records/5/task-list',
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

    it('offers both save buttons while the task has another page', () => {
      render(<FormPage page={page()} />)

      expect(screen.getByRole('button', { name: 'Save and continue' })).toBeInTheDocument()
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
          pageHash: 'page-hash',
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
        expect(mocks.push).toHaveBeenCalledWith('/portal/records/5/task-list')
      })
    })

    it('saves once and returns to the task list from the second button, even with a next page', async () => {
      const user = userEvent.setup()
      render(<FormPage page={page()} />)

      await user.type(screen.getByRole('textbox'), 'Hepatitis C')
      await user.click(screen.getByRole('button', { name: 'Save and return to task list' }))

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledWith('/portal/records/5/task-list')
      })
      expect(mocks.push).not.toHaveBeenCalledWith('/portal/records/5/bnf-chapter')
      expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1)
    })

    it('validates before returning to the task list and still allows continuing after correction', async () => {
      const user = userEvent.setup()
      render(<FormPage page={page()} />)

      await user.click(screen.getByRole('button', { name: 'Save and return to task list' }))

      expect(await screen.findByText('Enter the indication')).toBeInTheDocument()
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
      expect(mocks.push).not.toHaveBeenCalled()

      await user.type(screen.getByRole('textbox'), 'Hepatitis C')
      await user.click(screen.getByRole('button', { name: 'Save and continue' }))

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledExactlyOnceWith('/portal/records/5/bnf-chapter')
      })
    })

    it.each([
      {
        outcome: 'server validation fails',
        status: 400,
        error: { errors: { 'medicines_indication_detail.indication': ['Server says no'] } },
        message: 'Server says no',
      },
      {
        outcome: 'the page has changed',
        status: 409,
        error: {
          code: 'page_answers_changed',
          detail: 'This page has changed since it was loaded.',
        },
        message: 'This page has changed since it was loaded.',
      },
      {
        outcome: 'the server fails',
        status: 500,
        error: { detail: 'Internal server error' },
        message: 'There was a problem saving your answers. Please try again later.',
      },
    ])(
      'stays on the page when returning to the task list and $outcome',
      async ({ status, error, message }) => {
        const user = userEvent.setup()
        mocks.saveRecordFormPage.mockResolvedValue({ error, response: { ok: false, status } })
        render(
          <FormPage page={page({ answers: { 'medicines_indication_detail.indication': 'x' } })} />,
        )

        await user.click(screen.getByRole('button', { name: 'Save and return to task list' }))

        expect(await screen.findByText(message)).toBeInTheDocument()
        expect(mocks.push).not.toHaveBeenCalled()
        expect(screen.getByRole('button', { name: 'Save and return to task list' })).toBeEnabled()
      },
    )

    it('disables both save buttons while returning to the task list to prevent duplicate saves', async () => {
      const user = userEvent.setup()
      type SaveResult = {
        data: { nextPageId: string }
        response: { ok: boolean; status: number }
      }
      let resolveSave!: (result: SaveResult) => void
      mocks.saveRecordFormPage.mockReturnValueOnce(
        new Promise<SaveResult>((resolve) => {
          resolveSave = resolve
        }),
      )
      render(
        <FormPage page={page({ answers: { 'medicines_indication_detail.indication': 'x' } })} />,
      )

      const returnButton = screen.getByRole('button', { name: 'Save and return to task list' })
      const continueButton = screen.getByRole('button', { name: 'Save and continue' })
      await user.click(returnButton)

      expect(returnButton).toBeDisabled()
      expect(continueButton).toBeDisabled()
      await user.click(returnButton)
      await user.click(continueButton)
      expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1)
      expect(mocks.push).not.toHaveBeenCalled()

      resolveSave({
        data: { nextPageId: 'bnf-chapter' },
        response: { ok: true, status: 200 },
      })
      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledExactlyOnceWith('/portal/records/5/task-list')
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
          code: 'page_answers_changed',
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

  describe('conditional questions', () => {
    it('reveals a question beneath the option that triggers it', () => {
      render(<FormPage page={coMarketingPage()} />)

      expect(
        screen.getByRole('heading', { level: 1, name: /Is this product co-marketed\?$/ }),
      ).toBeInTheDocument()
      expect(screen.queryByRole('textbox')).toBeNull()

      fireEvent.click(screen.getByLabelText('Yes'))

      const revealed = document.getElementById(
        screen.getByLabelText('Yes').getAttribute('aria-controls') ?? '',
      )
      expect(revealed).toContainElement(screen.getByLabelText('Co-marketing company name'))
      expect(revealed).toBeVisible()

      fireEvent.click(screen.getByLabelText('No'))

      expect(screen.queryByRole('textbox')).toBeNull()
    })

    it('validates a revealed question', async () => {
      render(<FormPage page={coMarketingPage()} />)

      fireEvent.click(screen.getByLabelText('Yes'))
      save()

      expect(await screen.findByText('Enter the company name')).toBeInTheDocument()
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('does not validate a hidden question, and sends it unanswered', async () => {
      render(<FormPage page={coMarketingPage(['Yes'], { co: 'Yes', co_name: 'Acme' })} />)

      expect(screen.getByRole('textbox')).toHaveValue('Acme')
      fireEvent.click(screen.getByLabelText('No'))
      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalled())
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({
        co: 'No',
        co_name: null,
      })
    })

    it('reveals a question beneath each option that triggers it, under the trigger heading', async () => {
      const user = userEvent.setup()
      render(<FormPage page={coMarketingPage(['Yes', 'Unknown'])} />)

      expect(
        screen.getByRole('heading', { level: 1, name: /Is this product co-marketed\?$/ }),
      ).toBeInTheDocument()
      expect(screen.queryByRole('textbox')).toBeNull()

      for (const option of ['Yes', 'Unknown at this stage']) {
        await user.click(screen.getByLabelText(option))
        const revealed = document.getElementById(
          screen.getByLabelText(option).getAttribute('aria-controls') ?? '',
        )
        expect(revealed).toContainElement(screen.getByLabelText('Co-marketing company name'))
        expect(revealed).toBeVisible()
      }

      await user.click(screen.getByLabelText('No'))
      expect(screen.queryByRole('textbox')).toBeNull()
    })

    it('mounts one copy of a question revealed by several options, holding the saved answer', () => {
      render(
        <FormPage page={coMarketingPage(['Yes', 'Unknown'], { co: 'Unknown', co_name: 'Acme' })} />,
      )

      // hidden: true also counts copies inside hidden reveals.
      expect(screen.getAllByRole('textbox', { hidden: true })).toHaveLength(1)
      expect(screen.getByRole('textbox')).toHaveValue('Acme')
    })

    it('keeps the typed answer when switching between options that reveal it', async () => {
      const user = userEvent.setup()
      render(<FormPage page={coMarketingPage(['Yes', 'Unknown'])} />)

      await user.click(screen.getByLabelText('Yes'))
      await user.type(screen.getByLabelText('Co-marketing company name'), 'Acme')
      await user.click(screen.getByLabelText('Unknown at this stage'))

      expect(screen.getByLabelText('Co-marketing company name')).toHaveValue('Acme')
    })
  })

  describe('controls', () => {
    it('associates radio hints and errors and revalidates after selection and blur', async () => {
      const user = userEvent.setup()
      render(
        <FormPage
          page={page({
            questions: [
              question({
                id: 'choice',
                type: 'Radio',
                label: 'Choose an answer',
                hint: 'Select one answer',
                rules: [{ kind: 'Required', value: null, message: 'Choose yes or no' }],
                options: [
                  { value: 'Yes', label: 'Yes', hint: null, children: null },
                  { value: 'No', label: 'No', hint: null, children: null },
                ],
              }),
            ],
            answers: { choice: null },
          })}
        />,
      )
      const yes = screen.getByRole('radio', { name: 'Yes' })
      expect(yes).toHaveAccessibleDescription('Select one answer')
      await user.click(screen.getByRole('button', { name: 'Save and continue' }))
      expect(await screen.findByText('Choose yes or no')).toBeInTheDocument()
      expect(screen.getByRole('group', { name: 'Choose an answer' })).not.toHaveAttribute(
        'aria-invalid',
      )
      for (const radio of screen.getAllByRole('radio')) {
        expect(radio).not.toHaveAttribute('aria-invalid')
        expect(radio).not.toHaveAttribute('aria-expanded')
        expect(radio).toHaveAccessibleDescription(/Select one answer.*Choose yes or no/)
      }
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
      await user.click(yes)
      await user.tab()
      await waitFor(() => expect(screen.queryByText('Choose yes or no')).not.toBeInTheDocument())
      expect(yes).toBeChecked()
      expect(yes).not.toHaveAttribute('aria-invalid')
      expect(screen.getByRole('group', { name: 'Choose an answer' })).not.toHaveAttribute(
        'aria-invalid',
      )
    })

    it('saves the chosen radio option', async () => {
      render(
        <FormPage
          page={page({
            page: { id: 'cancer', title: 'Treating cancer', commercialInConfidence: false },
            questions: [
              question({
                id: 'cancer',
                type: 'Radio',
                label: 'Is this a product intended to treat cancer?',
                hint: null,
                rules: [],
                options: [
                  { value: 'Yes', label: 'Yes', hint: null, children: null },
                  { value: 'No', label: 'No', hint: null, children: null },
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

    it('associates a radio option hint only with the option it describes', () => {
      render(
        <FormPage
          page={page({
            page: { id: 'biomarker-type', title: 'Biomarker type', commercialInConfidence: false },
            questions: [
              question({
                id: 'biomarker-type',
                type: 'Radio',
                label: 'What type of biomarker is required for this test?',
                hint: null,
                rules: [],
                options: [
                  {
                    value: 'GenomicBiomarker',
                    label: 'Genomic biomarker',
                    hint: 'For example, a gene mutation, chromosomal alteration',
                    children: null,
                  },
                  { value: 'Unknown', label: 'Unknown at this stage', hint: null, children: null },
                ],
              }),
            ],
            answers: { 'biomarker-type': null },
          })}
        />,
      )

      expect(screen.getByText('For example, a gene mutation, chromosomal alteration')).toBeVisible()
      expect(screen.getByRole('radio', { name: 'Genomic biomarker' })).toHaveAccessibleDescription(
        'For example, a gene mutation, chromosomal alteration',
      )
      expect(
        screen.getByRole('radio', { name: 'Unknown at this stage' }),
      ).not.toHaveAccessibleDescription()
    })

    it('associates checkbox hints and errors and revalidates on selection and blur', async () => {
      const user = userEvent.setup()
      render(
        <FormPage
          page={page({
            questions: [
              question({
                id: 'ticks',
                type: 'Checkbox',
                multiple: true,
                label: 'Choose contact methods',
                hint: 'Select all that apply',
                rules: [{ kind: 'Required', value: null, message: 'Choose at least one method' }],
                options: tree,
              }),
            ],
            answers: { ticks: [] },
          })}
        />,
      )
      const first = screen.getByRole('checkbox', { name: '1: Addiction' })
      expect(first).toHaveAccessibleDescription('Select all that apply')
      await user.click(screen.getByRole('button', { name: 'Save and continue' }))
      expect(await screen.findByText('Choose at least one method')).toBeInTheDocument()
      expect(screen.getByRole('group', { name: 'Choose contact methods' })).not.toHaveAttribute(
        'aria-invalid',
      )
      for (const input of screen.getAllByRole('checkbox')) {
        expect(input).not.toHaveAttribute('aria-invalid')
        expect(input).toHaveAccessibleDescription(
          /Select all that apply.*Choose at least one method/,
        )
      }
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
      await user.click(first)
      await user.tab()
      await waitFor(() =>
        expect(screen.queryByText('Choose at least one method')).not.toBeInTheDocument(),
      )
      expect(first).not.toHaveAttribute('aria-invalid')
      expect(screen.getByRole('group', { name: 'Choose contact methods' })).not.toHaveAttribute(
        'aria-invalid',
      )
    })

    it('saves ticked checkboxes in selection insertion order', async () => {
      const user = userEvent.setup()
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
      await user.click(screen.getByLabelText('1: Addiction'))
      await user.click(screen.getByLabelText('2: Allergy'))
      await user.click(screen.getByLabelText('1: Addiction'))
      await user.click(screen.getByRole('button', { name: 'Save and continue' }))

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalled())
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({ ticks: ['2', '1'] })
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
            page: { id: 'bnf-chapter', title: 'BNF chapter', commercialInConfidence: false },
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
            page: { id: 'bnf-chapter', title: 'BNF chapter', commercialInConfidence: false },
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

  describe('prefixed number', () => {
    const idPage = (answer: string | null = null) =>
      page({
        questions: [niceTaIdQuestion],
        answers: { 'record_hta.nice_ta_development_id': answer },
      })

    it('shows a saved value split across the type and the number', () => {
      render(<FormPage page={idPage('GID-HST123')} />)

      expect(screen.getByRole('combobox', { name: 'Choose ID type' })).toHaveValue('GID-HST')
      expect(screen.getByRole('textbox', { name: 'Number' })).toHaveValue('123')
    })

    it('saves the chosen type and number as one answer', async () => {
      render(<FormPage page={idPage()} />)

      fireEvent.change(screen.getByRole('combobox', { name: 'Choose ID type' }), {
        target: { value: 'GID-HST' },
      })
      fireEvent.change(screen.getByRole('textbox', { name: 'Number' }), {
        target: { value: '123' },
      })
      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({
        'record_hta.nice_ta_development_id': 'GID-HST123',
      })
    })

    it('replaces both parts when a whole ID is pasted into the number', async () => {
      render(<FormPage page={idPage('GID-TA1')} />)

      fireEvent.change(screen.getByRole('textbox', { name: 'Number' }), {
        target: { value: 'GID-HST123' },
      })

      expect(screen.getByRole('combobox', { name: 'Choose ID type' })).toHaveValue('GID-HST')
      expect(screen.getByRole('textbox', { name: 'Number' })).toHaveValue('123')
      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({
        'record_hta.nice_ta_development_id': 'GID-HST123',
      })
    })

    it('saves nothing chosen as a blank answer', async () => {
      render(<FormPage page={idPage()} />)

      save()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
      expect(mocks.saveRecordFormPage.mock.calls[0][0].body.answers).toEqual({
        'record_hta.nice_ta_development_id': null,
      })
    })

    it('asks for a type when only a number is entered', async () => {
      render(<FormPage page={idPage()} />)

      fireEvent.change(screen.getByRole('textbox', { name: 'Number' }), {
        target: { value: '10234' },
      })
      save()

      expect(await screen.findByText('Select ID type')).toBeInTheDocument()
      expect(screen.getByRole('combobox', { name: 'Choose ID type' })).toBeInvalid()
      expect(screen.getByRole('textbox', { name: 'Number' })).toBeValid()
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('asks for positive whole numbers only when the number has other characters', async () => {
      render(<FormPage page={idPage()} />)

      fireEvent.change(screen.getByRole('combobox', { name: 'Choose ID type' }), {
        target: { value: 'GID-TA' },
      })
      fireEvent.change(screen.getByRole('textbox', { name: 'Number' }), {
        target: { value: '12a' },
      })
      save()

      expect(await screen.findByText('ID must be a whole positive number')).toBeInTheDocument()
      expect(screen.getByRole('combobox', { name: 'Choose ID type' })).toBeValid()
      expect(screen.getByRole('textbox', { name: 'Number' })).toBeInvalid()
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('asks for no more than 5 digits when the number is too long', async () => {
      render(<FormPage page={idPage()} />)

      fireEvent.change(screen.getByRole('combobox', { name: 'Choose ID type' }), {
        target: { value: 'GID-TA' },
      })
      fireEvent.change(screen.getByRole('textbox', { name: 'Number' }), {
        target: { value: '123456' },
      })
      save()

      expect(await screen.findByText('ID must not be longer than 5 digits')).toBeInTheDocument()
      expect(screen.getByRole('combobox', { name: 'Choose ID type' })).toBeValid()
      expect(screen.getByRole('textbox', { name: 'Number' })).toBeInvalid()
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })
  })
})
