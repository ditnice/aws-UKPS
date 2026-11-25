import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import type { RecordPageDto, RecordPageQuestionDto } from '@/client/generated'

import { FormPage } from './FormPage'

const mocks = vi.hoisted(() => ({
  push: vi.fn(),
  saveRecordFormPage: vi.fn(),
  getOrganisationCompanyCodes: vi.fn(),
}))

vi.mock('next/navigation', () => ({
  useRouter: () => ({ push: mocks.push }),
}))

vi.mock('@/client/generated', async (importOriginal) => {
  const original = await importOriginal<typeof import('@/client/generated')>()
  return {
    ...original,
    saveRecordFormPage: mocks.saveRecordFormPage,
    getOrganisationCompanyCodes: mocks.getOrganisationCompanyCodes,
  }
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
  dateName: null,
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
  organisationId: 3,
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
  itemLabel: 'Therapeutic area',
  removeLabel: 'Remove therapeutic area',
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

const groupQuestion = question({
  id: 'group',
  type: 'Group',
  label: 'Studies',
  hint: null,
  itemLabel: 'Study',
  rules: [],
  children: [question({ id: 'study_name', type: 'Text', label: 'Study name', hint: null })],
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
      expect(
        screen.getByText('. We will treat everything you enter on this page as confidential.'),
      ).toHaveClass('visually-hidden')
      expect(screen.getByRole('heading', { level: 1 })).toHaveAccessibleName(
        /Commercial in confidence\. We will treat everything you enter on this page as confidential\./,
      )
    })

    it('does not show the badge by default', () => {
      render(<FormPage page={page()} />)

      expect(screen.queryByText('Commercial in confidence')).toBeNull()
      expect(
        screen.queryByText('. We will treat everything you enter on this page as confidential.'),
      ).toBeNull()
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
        screen.getByRole('button', { name: 'Save and continue to task list' }),
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

    it('gives each question wrapper its question id, so error links can scroll to it', () => {
      const { container } = render(<FormPage page={page()} />)

      const wrapper = container.querySelector('[id="medicines_indication_detail.indication"]')
      expect(wrapper).not.toBeNull()
      expect(wrapper).toHaveAttribute('data-question-id', 'medicines_indication_detail.indication')
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

    describe('label size', () => {
      const label = (text: string) => screen.getByText(text, { selector: 'label' })
      const legend = (text: string) => screen.getByText(text, { selector: 'legend' })
      const other = question({ id: 'other', type: 'Text', label: 'Other', hint: null })

      it('makes each question label lead text on a page with several questions', () => {
        render(
          <FormPage
            page={page({
              questions: [
                question({}),
                other,
                question({ id: 'kind', type: 'Select', label: 'Kind', options: yesNo }),
                question({ id: 'co', type: 'Radio', label: 'Co-marketed?', options: yesNo }),
                question({ id: 'numbers', type: 'TextList', label: 'Numbers', rules: [] }),
              ],
              answers: {},
            })}
          />,
        )

        expect(
          label('What is the indication this product is seeking a licence for?').closest(
            '[class*="leadLabel"]',
          ),
        ).not.toBeNull()
        expect(label('Other').className).toMatch(/leadLabel/)
        expect(label('Kind').className).toMatch(/leadLabel/)
        expect(legend('Co-marketed?').className).toMatch(/leadLegend/)
        expect(legend('Numbers').className).toMatch(/leadLegend/)
      })

      it('keeps the standard size for the labels inside a question', () => {
        render(
          <FormPage
            page={page({
              questions: [
                other,
                question({ id: 'numbers', type: 'TextList', label: 'Numbers', rules: [] }),
              ],
              answers: {},
            })}
          />,
        )

        // The add another entry's own label, hidden behind the legend.
        expect(document.querySelector('[id="numbers"] .input__label')?.className).not.toMatch(
          /leadLabel/,
        )
      })

      it('keeps the standard size for a question revealed beneath an option', () => {
        const coMarketing = coMarketingPage(['Yes'], { co: 'Yes' })
        render(
          <FormPage
            page={{
              ...coMarketing,
              questions: [...coMarketing.questions, other],
              answers: { ...coMarketing.answers, other: null },
            }}
          />,
        )

        expect(legend('Is this product co-marketed?').className).toMatch(/leadLegend/)
        expect(label('Co-marketing company name').className).not.toMatch(/leadLabel/)
      })

      it("keeps a group's heading and its items' labels at their own sizes", () => {
        render(
          <FormPage
            page={page({
              questions: [other, groupQuestion],
              answers: { other: null, group: [] },
            })}
          />,
        )

        expect(screen.getByRole('heading', { level: 2, name: 'Studies' })).toBeInTheDocument()
        expect(screen.getByText('Studies').closest('legend')?.className).not.toMatch(/leadLegend/)
        expect(label('Study name').className).not.toMatch(/leadLabel/)
      })

      it('makes the labels of a hierarchical select lead text on a page of several', () => {
        render(
          <FormPage
            page={page({
              questions: [other, areasQuestion],
              answers: { other: null, areas: ['13'] },
            })}
          />,
        )

        // The question label is the group's own heading; each entry's parent label is hidden.
        expect(
          screen.getByRole('heading', { level: 2, name: areasQuestion.label }),
        ).toBeInTheDocument()
        expect(label('Select the disease or condition (optional)').className).toMatch(/leadLabel/)
      })

      it('does not make the label lead text on a single-question page', () => {
        render(<FormPage page={page()} />)

        expect(
          label('What is the indication this product is seeking a licence for?').closest(
            '[class*="leadLabel"]',
          ),
        ).toBeNull()
      })
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

      fireEvent.click(screen.getByRole('button', { name: 'Save and continue to task list' }))

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
            stray: ['This is not a question on this page'],
          },
        },
        response: { ok: false, status: 400 },
      })
      render(
        <FormPage page={page({ answers: { 'medicines_indication_detail.indication': 'x' } })} />,
      )

      save()

      expect(await screen.findByText('Server says no')).toBeInTheDocument()
      expect(screen.getByRole('alert')).toHaveTextContent('This is not a question on this page')
      expect(mocks.push).not.toHaveBeenCalled()
    })

    it('saves a corrected answer after the server rejected the earlier one', async () => {
      mocks.saveRecordFormPage.mockResolvedValueOnce({
        error: { errors: { 'medicines_indication_detail.indication': ['Server says no'] } },
        response: { ok: false, status: 400 },
      })
      render(
        <FormPage page={page({ answers: { 'medicines_indication_detail.indication': 'x' } })} />,
      )

      save()

      expect(await screen.findByText('Server says no')).toBeInTheDocument()

      fireEvent.change(screen.getByRole('textbox'), { target: { value: 'Hepatitis C' } })
      save()

      await waitFor(() => {
        expect(mocks.push).toHaveBeenCalledWith('/portal/records/5/bnf-chapter')
      })
      expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(2)
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
    it('associates radio hints and errors and revalidates on selection', async () => {
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
      await waitFor(() => expect(screen.queryByText('Choose yes or no')).not.toBeInTheDocument())
      expect(yes).toHaveFocus()
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

    it('associates checkbox hints and errors and revalidates on selection', async () => {
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
      expect(document.querySelectorAll('.checkbox--error')).toHaveLength(1)
      await user.click(first)
      await waitFor(() =>
        expect(screen.queryByText('Choose at least one method')).not.toBeInTheDocument(),
      )
      expect(first).toHaveFocus()
      expect(first).not.toHaveAttribute('aria-invalid')
      expect(screen.getByRole('group', { name: 'Choose contact methods' })).not.toHaveAttribute(
        'aria-invalid',
      )
    })

    it.each(['Checkbox', 'Radio'] as const)('shows links in a %s hint', (type) => {
      render(
        <FormPage
          page={page({
            questions: [
              question({
                id: 'choices',
                type,
                multiple: type === 'Checkbox',
                label: 'Choose',
                hint: 'Read [the guide](https://example.com/guide).',
                options: tree,
              }),
            ],
            answers: { choices: type === 'Checkbox' ? [] : null },
          })}
        />,
      )

      expect(screen.getByRole('link', { name: 'the guide (opens in a new tab)' })).toHaveAttribute(
        'href',
        'https://example.com/guide',
      )
      const option = type === 'Checkbox' ? 'checkbox' : 'radio'
      expect(screen.getByRole(option, { name: '1: Addiction' })).toHaveAccessibleDescription(
        'Read the guide (opens in a new tab).',
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

    it('describes the group of hierarchical selects with the question hint', () => {
      render(
        <FormPage
          page={page({
            questions: [{ ...areasQuestion, hint: 'Select up to 2 therapeutic areas.' }],
            answers: { areas: ['13', '2'] },
          })}
        />,
      )

      const hint = screen.getByText('Select up to 2 therapeutic areas.')
      expect(hint.closest('fieldset')?.getAttribute('aria-describedby')?.split(' ')).toContain(
        hint.id,
      )
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

      fireEvent.click(
        screen.getByRole('button', { name: 'Remove therapeutic area Therapeutic area 2' }),
      )
      expect(screen.getAllByRole('combobox')).toHaveLength(1)
    })

    it('names every select of a multi-value hierarchical select after its entry', () => {
      render(
        <FormPage page={page({ questions: [areasQuestion], answers: { areas: ['13', '11'] } })} />,
      )

      for (const entry of ['Therapeutic area 1', 'Therapeutic area 2']) {
        expect(screen.getByRole('group', { name: entry })).toBeInTheDocument()
        expect(screen.getByRole('combobox', { name: entry })).toBeInTheDocument()
        expect(
          screen.getByRole('combobox', {
            name: `Select the disease or condition (optional), ${entry}`,
          }),
        ).toBeInTheDocument()
      }
    })

    it('adds an entry of a hierarchical select, focusing its parent select', async () => {
      const user = userEvent.setup()
      render(<FormPage page={page({ questions: [areasQuestion], answers: { areas: ['2'] } })} />)

      expect(screen.queryByRole('button', { name: /Remove/ })).not.toBeInTheDocument()
      await user.click(screen.getByRole('button', { name: 'Add another therapy area' }))

      expect(screen.getByRole('combobox', { name: 'Therapeutic area 2' })).toHaveFocus()
    })

    it('removes the first entry of a hierarchical select and focuses the new first heading', async () => {
      const user = userEvent.setup()
      render(
        <FormPage page={page({ questions: [areasQuestion], answers: { areas: ['13', '2'] } })} />,
      )

      await user.click(
        screen.getByRole('button', { name: 'Remove therapeutic area Therapeutic area 1' }),
      )

      expect(screen.getAllByRole('combobox')).toHaveLength(1)
      expect(screen.getByRole('combobox', { name: 'Therapeutic area 1' })).toHaveValue('2')
      expect(
        screen.getByRole('heading', { name: 'Therapeutic area 1' }).closest('legend'),
      ).toHaveFocus()
      expect(screen.queryByRole('button', { name: /Remove/ })).not.toBeInTheDocument()
    })

    it('shows the error of a multi-value hierarchical select once, and clears it on removal', async () => {
      const user = userEvent.setup()
      render(
        <FormPage page={page({ questions: [areasQuestion], answers: { areas: ['13', '13'] } })} />,
      )

      save()

      // findBy fails if the message is shown more than once.
      const message = await screen.findByText('You have selected the same option more than once')
      expect(message.closest('fieldset')?.getAttribute('aria-describedby')?.split(' ')).toContain(
        message.id,
      )
      const selects = screen.getAllByRole('combobox')
      expect(selects).toHaveLength(4)
      for (const select of selects) {
        expect(select).toBeInvalid()
        expect(select.getAttribute('aria-describedby')?.split(' ')).toContain(message.id)
      }

      await user.click(
        screen.getByRole('button', { name: 'Remove therapeutic area Therapeutic area 2' }),
      )

      expect(
        screen.queryByText('You have selected the same option more than once'),
      ).not.toBeInTheDocument()
      for (const select of screen.getAllByRole('combobox')) {
        expect(select).toBeValid()
        expect(select).not.toHaveAttribute('aria-describedby')
      }
    })

    it('reports the same area chosen twice', async () => {
      render(
        <FormPage page={page({ questions: [areasQuestion], answers: { areas: ['2', '2'] } })} />,
      )

      save()

      expect(
        await screen.findAllByText('You have selected the same option more than once'),
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

  describe('date questions', () => {
    beforeEach(() => {
      vi.useFakeTimers({ toFake: ['Date'] })
      vi.setSystemTime(new Date(2026, 5, 15, 12))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    const precisionDate = question({
      id: 'uk_submission_date',
      type: 'PrecisionDate',
      label: 'What is the UK regulatory submission date?',
      hint: 'Choose the level of detail you are currently able to provide.',
      dateName: 'submission',
      rules: [{ kind: 'Required', value: null, message: 'Select an option' }],
    })
    const datePage = (answer: string | null = null) =>
      page({ questions: [precisionDate], answers: { uk_submission_date: answer } })

    /** The shown date parts, by label, that are marked as invalid. */
    const invalidParts = () =>
      ['Quarter', 'Day', 'Month', 'Year'].filter((label) =>
        screen
          .queryAllByRole(label === 'Quarter' ? 'combobox' : 'textbox', { name: label })
          .some((control) => control.getAttribute('aria-invalid') === 'true'),
      )

    const enterActualDate = async (
      user: ReturnType<typeof userEvent.setup>,
      day: string,
      month: string,
      year: string,
    ) => {
      await user.click(screen.getByRole('radio', { name: 'Actual date' }))
      for (const [label, text] of [
        ['Day', day],
        ['Month', month],
        ['Year', year],
      ]) {
        if (text) {
          await user.type(screen.getByRole('textbox', { name: label }), text)
        }
      }
    }

    it('sends an estimated quarter as the year and quarter', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await user.click(screen.getByRole('radio', { name: 'Estimated quarter' }))
      await user.selectOptions(screen.getByRole('combobox', { name: 'Quarter' }), '2')
      await user.type(screen.getByRole('textbox', { name: 'Year' }), '2027')
      save()

      await waitFor(() => {
        expect(mocks.saveRecordFormPage).toHaveBeenCalledWith(
          expect.objectContaining({
            body: expect.objectContaining({ answers: { uk_submission_date: '2027-Q2' } }),
          }),
        )
      })
    })

    it('loads a saved actual date into its parts', () => {
      render(<FormPage page={datePage('2027-05-14')} />)

      expect(screen.getByRole('radio', { name: 'Actual date' })).toBeChecked()
      expect(screen.getByRole('textbox', { name: 'Day' })).toHaveValue('14')
      expect(screen.getByRole('textbox', { name: 'Month' })).toHaveValue('5')
      expect(screen.getByRole('textbox', { name: 'Year' })).toHaveValue('2027')
    })

    it('asks to select an option on the radios when no precision is chosen', async () => {
      const { container } = render(<FormPage page={datePage()} />)

      save()

      const message = await screen.findByText('Select an option')
      expect(message.closest('[data-component="radios-conditional"]')).toBeNull()
      const invalid = container.querySelectorAll('.radio--error')
      expect(invalid).toHaveLength(1)
      expect(invalid[0]).toContainElement(screen.getByRole('radio', { name: 'Estimated quarter' }))
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('asks for the date when only the precision is chosen, marking every part', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await user.click(screen.getByRole('radio', { name: 'Estimated month' }))
      save()

      expect(await screen.findByText('Enter the estimated submission date')).toBeInTheDocument()
      expect(invalidParts()).toEqual(['Month', 'Year'])
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('asks for the date when any part is blank, marking only the blank parts', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await enterActualDate(user, '', 'AB', '2025')
      save()

      expect(await screen.findByText('Enter the actual submission date')).toBeInTheDocument()
      expect(invalidParts()).toEqual(['Day'])
    })

    it('asks for a quarter when only the year is entered', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await user.click(screen.getByRole('radio', { name: 'Estimated quarter' }))
      await user.type(screen.getByRole('textbox', { name: 'Year' }), '2027')
      save()

      expect(await screen.findByText('Select a quarter')).toBeInTheDocument()
      expect(invalidParts()).toEqual(['Quarter'])
    })

    it('rejects parts that are not real, marking only those parts', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await user.click(screen.getByRole('radio', { name: 'Estimated month' }))
      await user.type(screen.getByRole('textbox', { name: 'Month' }), '16')
      await user.type(screen.getByRole('textbox', { name: 'Year' }), '2027')
      save()

      expect(
        await screen.findByText('Estimated date of submission must be a real date'),
      ).toBeInTheDocument()
      expect(invalidParts()).toEqual(['Month'])
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('rejects a day that is not in its month, marking only the day', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await enterActualDate(user, '31', '4', '2026')
      save()

      expect(
        await screen.findByText('Actual date of submission must be a real date'),
      ).toBeInTheDocument()
      expect(invalidParts()).toEqual(['Day'])
    })

    it('rejects an actual date in the future, marking every part', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await enterActualDate(user, '16', '6', '2026')
      save()

      expect(
        await screen.findByText('Actual date of submission must be today or in the past'),
      ).toBeInTheDocument()
      expect(invalidParts()).toEqual(['Day', 'Month', 'Year'])
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('shows the error above the chosen precision, not on the radios', async () => {
      const user = userEvent.setup()
      const { container } = render(<FormPage page={datePage()} />)

      await user.click(screen.getByRole('radio', { name: 'Estimated quarter' }))
      await user.selectOptions(screen.getByRole('combobox', { name: 'Quarter' }), '1')
      await user.type(screen.getByRole('textbox', { name: 'Year' }), '2025')
      save()

      const message = await screen.findByText('Estimated date of submission must be in the future')
      expect(message.closest('[data-component="radios-conditional"]')).not.toBeNull()
      expect(container.querySelector('.radio--error')).toBeNull()
      expect(invalidParts()).toEqual(['Quarter', 'Year'])
      for (const control of [
        screen.getByRole('combobox', { name: 'Quarter' }),
        screen.getByRole('textbox', { name: 'Year' }),
      ]) {
        expect(control).toHaveAccessibleDescription(expect.stringContaining(message.textContent!))
      }
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('marks every part for an error from the server', async () => {
      mocks.saveRecordFormPage.mockResolvedValue({
        error: {
          errors: {
            uk_submission_date: ['Actual date of submission must be today or in the past'],
          },
        },
        response: { ok: false, status: 400 },
      })
      render(<FormPage page={datePage('2026-06-15')} />)

      save()

      expect(
        await screen.findByText('Actual date of submission must be today or in the past'),
      ).toBeInTheDocument()
      expect(invalidParts()).toEqual(['Day', 'Month', 'Year'])
    })

    it('starts a newly chosen precision empty', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await user.click(screen.getByRole('radio', { name: 'Estimated quarter' }))
      await user.selectOptions(screen.getByRole('combobox', { name: 'Quarter' }), '2')
      await enterActualDate(user, '14', '5', '2027')
      await user.click(screen.getByRole('radio', { name: 'Estimated quarter' }))

      expect(screen.getByRole('combobox', { name: 'Quarter' })).toHaveValue('')
      expect(screen.getByRole('textbox', { name: 'Year' })).toHaveValue('')

      await user.click(screen.getByRole('radio', { name: 'Actual date' }))

      for (const part of ['Day', 'Month', 'Year']) {
        expect(screen.getByRole('textbox', { name: part })).toHaveValue('')
      }
    })

    it('clears the error when another precision is chosen, and asks again on the next save', async () => {
      const user = userEvent.setup()
      render(<FormPage page={datePage()} />)

      await enterActualDate(user, '31', '4', '2026')
      save()
      expect(
        await screen.findByText('Actual date of submission must be a real date'),
      ).toBeInTheDocument()

      await user.click(screen.getByRole('radio', { name: 'Estimated month' }))
      expect(
        screen.queryByText('Actual date of submission must be a real date'),
      ).not.toBeInTheDocument()

      // Moving into, between and out of the parts does not revalidate.
      await user.click(screen.getByRole('textbox', { name: 'Month' }))
      await user.keyboard('7')
      await user.tab()
      await user.tab()
      expect(screen.queryByText('Enter the estimated submission date')).not.toBeInTheDocument()
      expect(invalidParts()).toEqual([])

      // The next save does, marking only the blank year.
      save()
      expect(await screen.findByText('Enter the estimated submission date')).toBeInTheDocument()
      expect(invalidParts()).toEqual(['Year'])
    })

    describe('actual month', () => {
      const monthPage = (answer: string | null = null) =>
        page({
          page: { id: 'global', title: 'Global first submission', commercialInConfidence: true },
          questions: [
            question({ id: 'region', type: 'Text', label: 'Where?', hint: null, rules: [] }),
            question({
              id: 'global_date',
              type: 'ActualMonth',
              label: 'Enter the date of GFS of this product',
              hint: null,
              rules: [],
            }),
          ],
          answers: { region: null, global_date: answer },
        })

      it('sends the month and year beside another question', async () => {
        const user = userEvent.setup()
        render(<FormPage page={monthPage()} />)

        expect(
          screen.getByRole('group', { name: 'Enter the date of GFS of this product' }),
        ).toBeInTheDocument()
        expect(screen.queryByRole('textbox', { name: 'Day' })).not.toBeInTheDocument()
        await user.type(screen.getByRole('textbox', { name: 'Month' }), '7')
        await user.type(screen.getByRole('textbox', { name: 'Year' }), '2025')
        save()

        await waitFor(() => {
          expect(mocks.saveRecordFormPage).toHaveBeenCalledWith(
            expect.objectContaining({
              body: expect.objectContaining({
                answers: { region: null, global_date: '2025-07' },
              }),
            }),
          )
        })
      })

      it('loads a saved month into its parts', () => {
        render(<FormPage page={monthPage('2025-07')} />)

        expect(screen.getByRole('textbox', { name: 'Month' })).toHaveValue('7')
        expect(screen.getByRole('textbox', { name: 'Year' })).toHaveValue('2025')
      })

      it('rejects a month after this one without saving', async () => {
        const user = userEvent.setup()
        render(<FormPage page={monthPage()} />)

        await user.type(screen.getByRole('textbox', { name: 'Month' }), '7')
        await user.type(screen.getByRole('textbox', { name: 'Year' }), '2026')
        save()

        expect(
          await screen.findByText('The date must be this month or in the past'),
        ).toBeInTheDocument()
        expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
      })
    })

    describe('actual date', () => {
      const actualDatePage = (answer: string | null = null) =>
        page({
          page: { id: 'orphan', title: 'Orphan medicine status', commercialInConfidence: false },
          questions: [
            question({ id: 'region', type: 'Text', label: 'Where?', hint: null, rules: [] }),
            question({
              id: 'granted_date',
              type: 'ActualDate',
              label: 'Date EU orphan medicine status granted',
              dateName: 'EU orphan medicine status',
              hint: null,
              rules: [],
            }),
          ],
          answers: { region: null, granted_date: answer },
        })

      it('sends the day, month and year beside another question', async () => {
        const user = userEvent.setup()
        render(<FormPage page={actualDatePage()} />)

        expect(
          screen.getByRole('group', { name: 'Date EU orphan medicine status granted' }),
        ).toBeInTheDocument()
        await user.type(screen.getByRole('textbox', { name: 'Day' }), '26')
        await user.type(screen.getByRole('textbox', { name: 'Month' }), '7')
        await user.type(screen.getByRole('textbox', { name: 'Year' }), '2025')
        save()

        await waitFor(() => {
          expect(mocks.saveRecordFormPage).toHaveBeenCalledWith(
            expect.objectContaining({
              body: expect.objectContaining({
                answers: { region: null, granted_date: '2025-07-26' },
              }),
            }),
          )
        })
      })

      it('loads a saved date into its parts', () => {
        render(<FormPage page={actualDatePage('2025-07-26')} />)

        expect(screen.getByRole('textbox', { name: 'Day' })).toHaveValue('26')
        expect(screen.getByRole('textbox', { name: 'Month' })).toHaveValue('7')
        expect(screen.getByRole('textbox', { name: 'Year' })).toHaveValue('2025')
      })

      it('rejects a date after today without saving', async () => {
        const user = userEvent.setup()
        render(<FormPage page={actualDatePage()} />)

        await user.type(screen.getByRole('textbox', { name: 'Day' }), '1')
        await user.type(screen.getByRole('textbox', { name: 'Month' }), '1')
        await user.type(screen.getByRole('textbox', { name: 'Year' }), '2999')
        save()

        expect(
          await screen.findByText(
            'Actual date of EU orphan medicine status must be today or in the past',
          ),
        ).toBeInTheDocument()
        expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
      })
    })
  })

  describe('when the user came from check your answers', () => {
    const fromCheckAnswers = () =>
      render(
        <FormPage page={page({ questions: [question({ rules: [] })] })} returnToCheckAnswers />,
      )

    it('offers a single button that returns to check your answers', () => {
      fromCheckAnswers()

      expect(
        screen.getByRole('button', { name: 'Save and return to check your answers' }),
      ).toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Save and continue' })).toBeNull()
      expect(screen.queryByRole('button', { name: 'Save and continue to task list' })).toBeNull()
      expect(screen.queryByRole('button', { name: 'Save and return to task list' })).toBeNull()
    })

    it('links back to check your answers', () => {
      fromCheckAnswers()

      expect(screen.getByRole('link', { name: 'Back' })).toHaveAttribute(
        'href',
        '/portal/records/5/check-your-answers',
      )
      expect(screen.getByRole('link', { name: 'Return to check your answers' })).toBeInTheDocument()
    })

    it('goes to check your answers after saving, even when the task has another page', async () => {
      fromCheckAnswers()

      fireEvent.click(screen.getByRole('button', { name: 'Save and return to check your answers' }))

      await waitFor(() =>
        expect(mocks.push).toHaveBeenCalledWith('/portal/records/5/check-your-answers'),
      )
    })
  })
})

describe('FormPage clinical trials', () => {
  const trialsQuestion = question({
    id: 'record_clinical_trial',
    type: 'Group',
    label: 'Enter clinical trial details',
    hint: 'Include all studies that support marketing authorisation and HTA submission.',
    addAnotherLabel: 'Add another clinical trial',
    itemLabel: 'Clinical trial',
    removeLabel: 'Remove clinical trial',
    rules: [{ kind: 'Required', value: null, message: 'Add at least one clinical trial' }],
    children: [
      question({
        id: 'study_name',
        type: 'Text',
        label: 'Study name',
        hint: 'Enter the official or commonly used study title.',
        width: 'Full',
        rules: [{ kind: 'Required', value: null, message: 'Enter study name' }],
      }),
      question({
        id: 'clinical_trials_gov_number',
        type: 'PrefixedNumber',
        label: 'Trial number from clinical trials.gov',
        hint: 'Enter the 8 digit identification number.',
        prefixes: ['NCT'],
        prefixDigits: 8,
        numberName: 'Trial number',
        rules: [
          { kind: 'Required', value: null, message: 'Enter the trial number' },
          { kind: 'Distinct', value: null, message: 'Trial number must be different' },
        ],
      }),
      question({
        id: 'other_registry_numbers',
        type: 'TextList',
        multiple: true,
        label: 'Trial number from other registry (optional)',
        hint: 'Enter any additional trial numbers from other registries.',
        addAnotherLabel: 'Add another trial number',
        itemLabel: 'Other registry number',
        removeLabel: 'Remove',
        width: 'Full',
        rules: [{ kind: 'Distinct', value: null, message: 'Registry number must be different' }],
      }),
    ],
  })

  const trialsPage = (answer: unknown = []) =>
    page({
      page: {
        id: 'clinical-trial-details',
        title: 'Clinical trial details',
        commercialInConfidence: false,
      },
      nextPageId: 'recruiting-in-uk',
      questions: [trialsQuestion],
      answers: { record_clinical_trial: answer },
    })

  const savedTrial = {
    study_name: 'ION-UK',
    clinical_trials_gov_number: 'NCT06123456',
    other_registry_numbers: ['A1', 'B2'],
  }

  const sentAnswers = () =>
    (mocks.saveRecordFormPage.mock.calls[0][0] as { body: { answers: Record<string, unknown> } })
      .body.answers

  it('starts with one empty trial headed "Clinical trial 1" and no remove button', () => {
    render(<FormPage page={trialsPage()} />)

    expect(
      // The NDS page header puts the task caption inside the h1.
      screen.getByRole('heading', { level: 1, name: /Enter clinical trial details$/ }),
    ).toBeInTheDocument()
    expect(screen.getByRole('heading', { level: 2, name: 'Clinical trial 1' })).toBeInTheDocument()
    expect(screen.getByLabelText('Study name')).toHaveValue('')
    expect(screen.queryByRole('button', { name: /Remove clinical trial/ })).not.toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Add another trial number' })).toBeInTheDocument()
  })

  it('makes the study name and the other registry number full width', () => {
    render(<FormPage page={trialsPage()} />)

    const otherRegistryBox = screen
      .getByLabelText('Other registry number')
      .closest('[data-component="input"]')?.parentElement

    expect(screen.getByLabelText('Study name')).toHaveStyle({ width: '100%' })
    expect(otherRegistryBox?.style.getPropertyValue('--input-basis')).toBe('100%')
  })

  it('gives each trial an id that a link can scroll to', () => {
    render(
      <FormPage
        page={trialsPage([
          savedTrial,
          { ...savedTrial, clinical_trials_gov_number: 'NCT06123457' },
        ])}
      />,
    )

    expect(screen.getByRole('group', { name: 'Clinical trial 1' })).toHaveAttribute(
      'id',
      'record_clinical_trial-item-0',
    )
    expect(screen.getByRole('group', { name: 'Clinical trial 2' })).toHaveAttribute(
      'id',
      'record_clinical_trial-item-1',
    )
  })

  it('opens with a new blank trial after the saved ones when asked to add one', () => {
    render(<FormPage addGroupItem page={trialsPage([savedTrial])} />)

    expect(screen.getByRole('heading', { level: 2, name: 'Clinical trial 2' })).toBeInTheDocument()
    const studyNames = screen.getAllByLabelText('Study name')
    expect(studyNames[0]).toHaveValue('ION-UK')
    expect(studyNames[1]).toHaveValue('')
  })

  it('opens with one blank trial when asked to add one and none are saved', () => {
    render(<FormPage addGroupItem page={trialsPage()} />)

    expect(screen.getByRole('heading', { level: 2, name: 'Clinical trial 1' })).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Clinical trial 2' })).not.toBeInTheDocument()
  })

  it('shows the trial number with a fixed NCT prefix', () => {
    render(<FormPage page={trialsPage([savedTrial])} />)

    expect(screen.getByText('NCT')).toBeInTheDocument()
    expect(screen.getByLabelText('Trial number from clinical trials.gov')).toHaveValue('06123456')
  })

  it('keeps one canonical prefix when a whole ID is pasted in any case', async () => {
    const user = userEvent.setup()
    render(<FormPage page={trialsPage([{ ...savedTrial, clinical_trials_gov_number: '' }])} />)

    await user.click(screen.getByLabelText('Trial number from clinical trials.gov'))
    await user.paste('nct06123456')

    expect(screen.getByLabelText('Trial number from clinical trials.gov')).toHaveValue('06123456')
    save()
    await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
    expect(sentAnswers()).toEqual({ record_clinical_trial: [savedTrial] })
  })

  it('groups the trials in a fieldset described by the hint', () => {
    render(<FormPage page={trialsPage()} />)

    expect(
      screen.getByRole('group', { name: 'Enter clinical trial details' }),
    ).toHaveAccessibleDescription(
      'Include all studies that support marketing authorisation and HTA submission.',
    )
  })

  it('loads saved trials and their registry numbers', () => {
    render(<FormPage page={trialsPage([savedTrial])} />)

    expect(screen.getByLabelText('Study name')).toHaveValue('ION-UK')
    expect(screen.getByLabelText('Other registry number 1')).toHaveValue('A1')
    expect(screen.getByLabelText('Other registry number 2')).toHaveValue('B2')
  })

  it('adds a trial, focusing its first field, and sends every trial as an item', async () => {
    const user = userEvent.setup()
    render(<FormPage page={trialsPage([savedTrial])} />)

    await user.click(screen.getByRole('button', { name: 'Add another clinical trial' }))

    expect(screen.getByRole('heading', { level: 2, name: 'Clinical trial 2' })).toBeInTheDocument()
    const studyNames = screen.getAllByLabelText('Study name')
    expect(studyNames[1]).toHaveFocus()
    await user.type(studyNames[1], 'ION-EU')
    await user.type(
      screen.getAllByLabelText('Trial number from clinical trials.gov')[1],
      '06123457',
    )
    save()

    await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
    expect(sentAnswers()).toEqual({
      record_clinical_trial: [
        savedTrial,
        {
          study_name: 'ION-EU',
          clinical_trials_gov_number: 'NCT06123457',
          other_registry_numbers: [],
        },
      ],
    })
  })

  it('removes a trial and moves focus to the heading before it', async () => {
    const user = userEvent.setup()
    const second = {
      ...savedTrial,
      study_name: 'ION-EU',
      clinical_trials_gov_number: 'NCT06123457',
    }
    render(<FormPage page={trialsPage([savedTrial, second])} />)

    await user.click(screen.getByRole('button', { name: 'Remove clinical trial Clinical trial 2' }))

    expect(screen.queryByRole('heading', { name: 'Clinical trial 2' })).not.toBeInTheDocument()
    expect(screen.getByText('Clinical trial 1').closest('legend')).toHaveFocus()
  })

  it('labels a single registry number without a number and several with numbers', async () => {
    const user = userEvent.setup()
    render(<FormPage page={trialsPage([{ ...savedTrial, other_registry_numbers: [] }])} />)

    expect(screen.getByLabelText('Other registry number')).toBeVisible()
    expect(screen.queryByLabelText('Other registry number 1')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Add another trial number' }))

    expect(screen.queryByLabelText('Other registry number')).not.toBeInTheDocument()
    expect(screen.getByLabelText('Other registry number 1')).toBeInTheDocument()
    expect(screen.getByLabelText('Other registry number 2')).toBeInTheDocument()
  })

  it('adds and removes registry numbers, focusing the new box and then the list legend', async () => {
    const user = userEvent.setup()
    render(<FormPage page={trialsPage([{ ...savedTrial, other_registry_numbers: [] }])} />)

    await user.click(screen.getByRole('button', { name: 'Add another trial number' }))

    const second = screen.getByLabelText('Other registry number 2')
    expect(second).toHaveFocus()
    await user.click(screen.getByRole('button', { name: 'Remove Other registry number 2' }))
    expect(
      screen.getByText('Trial number from other registry (optional)', { selector: 'legend' }),
    ).toHaveFocus()
  })

  it('drops blank registry numbers from the answer', async () => {
    render(
      <FormPage page={trialsPage([{ ...savedTrial, other_registry_numbers: ['A1', '  '] }])} />,
    )

    save()

    await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
    expect(sentAnswers()).toEqual({
      record_clinical_trial: [{ ...savedTrial, other_registry_numbers: ['A1'] }],
    })
  })

  it('shows each problem against its trial without saving', async () => {
    const user = userEvent.setup()
    render(
      <FormPage
        page={trialsPage([
          {
            study_name: '',
            clinical_trials_gov_number: 'NCT1234',
            other_registry_numbers: ['A1', 'a1'],
          },
          { ...savedTrial },
          { ...savedTrial, study_name: 'Copy' },
        ])}
      />,
    )

    await user.click(screen.getByRole('button', { name: 'Save and continue' }))

    expect(await screen.findByText('Enter study name')).toBeInTheDocument()
    expect(screen.getByText('Trial number must be 8 digits long')).toBeInTheDocument()
    expect(screen.getByText('Registry number must be different')).toBeInTheDocument()
    expect(screen.getByText('Trial number must be different')).toBeInTheDocument()
    expect(screen.getAllByLabelText('Study name')[0]).toHaveAttribute('aria-invalid', 'true')
    expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
  })

  it.each([1, 2])(
    'clears positional registry errors when removing entry %i after validation',
    async (entry) => {
      const user = userEvent.setup()
      render(
        <FormPage
          page={trialsPage([{ ...savedTrial, other_registry_numbers: ['A1', 'A1', 'B2'] }])}
        />,
      )

      await user.click(screen.getByRole('button', { name: 'Save and continue' }))
      expect(await screen.findByText('Registry number must be different')).toBeInTheDocument()

      await user.click(
        screen.getByRole('button', {
          name: `Remove Other registry number ${entry}`,
        }),
      )

      expect(screen.queryByText('Registry number must be different')).not.toBeInTheDocument()
      const first = screen.getByLabelText('Other registry number 1')
      const second = screen.getByLabelText('Other registry number 2')
      expect(first).toHaveValue('A1')
      expect(second).toHaveValue('B2')
      expect(first).not.toHaveAttribute('aria-invalid', 'true')
      expect(second).not.toHaveAttribute('aria-invalid', 'true')
    },
  )

  it.each(['remove a trial', 'fill a blank registry entry', 'remove a registry entry'])(
    'ignores stale server errors when users %s during a save and allows retrying',
    async (change) => {
      const user = userEvent.setup()
      const result = {
        error: {
          title: 'Invalid',
          errors: {
            'record_clinical_trial[0].study_name': ['Study name from the server'],
            'record_clinical_trial[0].other_registry_numbers[0]': ['Entry from the server'],
          },
        },
        response: { ok: false, status: 400 },
      }
      let resolveSave!: (value: typeof result) => void
      mocks.saveRecordFormPage.mockReturnValueOnce(
        new Promise<typeof result>((resolve) => {
          resolveSave = resolve
        }),
      )
      mocks.saveRecordFormPage.mockResolvedValueOnce({
        data: { nextPageId: 'recruiting-in-uk' },
        response: { ok: true, status: 200 },
      })
      render(
        <FormPage
          page={trialsPage([
            { ...savedTrial, other_registry_numbers: ['', 'B2'] },
            { ...savedTrial, study_name: 'ION-EU', clinical_trials_gov_number: 'NCT06123457' },
          ])}
        />,
      )

      const saveButton = screen.getByRole('button', { name: 'Save and continue' })
      await user.click(saveButton)
      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
      expect(saveButton).toBeDisabled()

      if (change === 'remove a trial') {
        await user.click(
          screen.getByRole('button', { name: 'Remove clinical trial Clinical trial 1' }),
        )
        expect(screen.getByLabelText('Study name')).toHaveValue('ION-EU')
      } else if (change === 'fill a blank registry entry') {
        await user.type(screen.getAllByLabelText('Other registry number 1')[0], 'A1')
      } else {
        await user.click(
          screen.getAllByRole('button', {
            name: 'Remove Other registry number 1',
          })[0],
        )
      }

      resolveSave(result)
      await waitFor(() => expect(saveButton).toBeEnabled())

      expect(screen.queryByText('Study name from the server')).not.toBeInTheDocument()
      expect(screen.queryByText('Entry from the server')).not.toBeInTheDocument()
      expect(mocks.push).not.toHaveBeenCalled()

      await user.click(saveButton)
      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(2))
      expect(mocks.push).toHaveBeenCalledWith('/portal/records/5/recruiting-in-uk')
    },
  )

  it('shows server errors against the trial, entry and group they name', async () => {
    mocks.saveRecordFormPage.mockResolvedValue({
      error: {
        title: 'Invalid',
        errors: {
          record_clinical_trial: ['Add at least one clinical trial'],
          'record_clinical_trial[0].study_name': ['Study name from the server'],
          // The server numbers entries without the blank one shown first.
          'record_clinical_trial[0].other_registry_numbers[0]': ['Entry from the server'],
        },
      },
      response: { ok: false, status: 400 },
    })
    render(<FormPage page={trialsPage([{ ...savedTrial, other_registry_numbers: ['', 'B2'] }])} />)

    save()

    expect(await screen.findByText('Study name from the server')).toBeInTheDocument()
    expect(screen.getByText('Add at least one clinical trial')).toBeInTheDocument()
    expect(screen.getByText('Entry from the server')).toBeInTheDocument()
    expect(screen.getByLabelText('Other registry number 2')).toHaveAttribute('aria-invalid', 'true')
    expect(screen.getByLabelText('Other registry number 1')).not.toHaveAttribute(
      'aria-invalid',
      'true',
    )
  })
})

describe('FormPage names and identifiers', () => {
  const genericNames = question({
    id: 'record_product_detail.generic_names',
    type: 'TextList',
    multiple: true,
    label: 'Active substance',
    hint: 'Enter the standard, non-proprietary name for the active substances.',
    addAnotherLabel: 'Add another active substance',
    rules: [
      { kind: 'Required', value: null, message: 'Enter generic name' },
      { kind: 'Distinct', value: null, message: 'Generic names must be distinct' },
    ],
  })

  const namesPage = (answer: string[] = ['sofosbuvir', 'velpatasvir']) =>
    page({
      section: { id: 'product-record-details', title: 'Provide product record details' },
      task: { id: 'names-and-identifiers', title: 'Names and identifiers' },
      page: {
        id: 'names-and-identifiers',
        title: 'Provide product record details',
        commercialInConfidence: false,
      },
      nextPageId: null,
      questions: [genericNames],
      answers: { [genericNames.id]: answer },
    })

  // The task has one page, so its button returns to the task list.
  const saveAndReturn = () =>
    fireEvent.click(screen.getByRole('button', { name: 'Save and continue to task list' }))

  const sentAnswers = () =>
    (mocks.saveRecordFormPage.mock.calls[0][0] as { body: { answers: Record<string, unknown> } })
      .body.answers

  it('lists the saved entries under the question as a group', () => {
    render(<FormPage page={namesPage()} />)

    expect(screen.getByRole('group', { name: 'Active substance' })).toBeInTheDocument()
    expect(screen.getByLabelText('Active substance 1')).toHaveValue('sofosbuvir')
    expect(screen.getByLabelText('Active substance 2')).toHaveValue('velpatasvir')
  })

  it('adds an entry, focusing it, and sends the entries in order without blanks', async () => {
    const user = userEvent.setup()
    render(<FormPage page={namesPage(['sofosbuvir'])} />)

    await user.click(screen.getByRole('button', { name: 'Add another active substance' }))
    expect(screen.getByLabelText('Active substance 2')).toHaveFocus()
    await user.click(screen.getByRole('button', { name: 'Add another active substance' }))
    await user.type(screen.getByLabelText('Active substance 3'), 'voxilaprevir')
    saveAndReturn()

    await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
    expect(sentAnswers()).toEqual({ [genericNames.id]: ['sofosbuvir', 'voxilaprevir'] })
  })

  it('removes an entry', async () => {
    const user = userEvent.setup()
    render(<FormPage page={namesPage()} />)

    await user.click(screen.getByRole('button', { name: 'Remove Active substance 1' }))
    saveAndReturn()

    await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
    expect(sentAnswers()).toEqual({ [genericNames.id]: ['velpatasvir'] })
  })

  it('shows a repeated entry against that entry without saving', async () => {
    render(<FormPage page={namesPage(['sofosbuvir', 'SOFOSBUVIR'])} />)

    saveAndReturn()

    expect(await screen.findByText('Generic names must be distinct')).toBeInTheDocument()
    expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
  })

  describe('company code', () => {
    const companyCode = question({
      id: 'record_product_detail.company_code',
      type: 'Text',
      label: 'Company code',
      hint: 'Enter the primary internal code or working name your company uses.',
      display: 'autocomplete:company-codes',
      rules: [{ kind: 'Required', value: null, message: 'Enter company code' }],
    })

    const companyCodePage = (overrides: Partial<RecordPageQuestionDto> = {}) =>
      page({
        ...namesPage(),
        questions: [{ ...companyCode, ...overrides }, genericNames],
        answers: { [companyCode.id]: 'ABC', [genericNames.id]: ['sofosbuvir'] },
      })

    beforeEach(() => {
      mocks.getOrganisationCompanyCodes.mockResolvedValue({
        data: { items: ['ABC-123', 'ABC-456'] },
      })
    })

    it("suggests the organisation's company codes and saves the one chosen", async () => {
      const user = userEvent.setup()
      render(<FormPage page={companyCodePage()} />)

      const input = screen.getByRole('combobox', { name: 'Company code' })
      expect(input).toHaveValue('ABC')
      await user.type(input, '-4')
      await user.click(await screen.findByRole('option', { name: 'ABC-456' }))
      saveAndReturn()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
      expect(mocks.getOrganisationCompanyCodes).toHaveBeenCalledWith(
        expect.objectContaining({
          path: { organisationId: 3 },
          query: expect.objectContaining({ Search: 'ABC-4' }),
        }),
      )
      expect(sentAnswers()).toEqual({
        [companyCode.id]: 'ABC-456',
        [genericNames.id]: ['sofosbuvir'],
      })
    })

    it('saves a company code that is not among the suggestions', async () => {
      const user = userEvent.setup()
      render(<FormPage page={companyCodePage()} />)

      const input = screen.getByRole('combobox', { name: 'Company code' })
      await user.clear(input)
      await user.type(input, 'NEW-1')
      saveAndReturn()

      await waitFor(() => expect(mocks.saveRecordFormPage).toHaveBeenCalledTimes(1))
      expect(sentAnswers()).toMatchObject({ [companyCode.id]: 'NEW-1' })
    })

    it('still requires a company code', async () => {
      const user = userEvent.setup()
      render(<FormPage page={companyCodePage()} />)

      await user.clear(screen.getByRole('combobox', { name: 'Company code' }))
      saveAndReturn()

      expect(await screen.findByText('Enter company code', { selector: 'p *, p' })).toBeVisible()
      expect(mocks.saveRecordFormPage).not.toHaveBeenCalled()
    })

    it('is a plain text input without the autocomplete display hint', () => {
      render(<FormPage page={companyCodePage({ display: null })} />)

      expect(screen.queryByRole('combobox')).not.toBeInTheDocument()
      expect(screen.getByRole('textbox', { name: 'Company code' })).toHaveValue('ABC')
    })

    it('is a plain text input when the display hint names an unknown source', () => {
      render(<FormPage page={companyCodePage({ display: 'autocomplete:unknown' })} />)

      expect(screen.queryByRole('combobox')).not.toBeInTheDocument()
      expect(screen.getByRole('textbox', { name: 'Company code' })).toHaveValue('ABC')
    })

    it('is a plain text input for the bare autocomplete hint, which names no source', () => {
      render(<FormPage page={companyCodePage({ display: 'autocomplete' })} />)

      expect(screen.queryByRole('combobox')).not.toBeInTheDocument()
    })
  })

  it('shows a server error on an entry against the entry it names', async () => {
    mocks.saveRecordFormPage.mockResolvedValue({
      error: { errors: { [`${genericNames.id}[1]`]: ['Server says this one is repeated'] } },
      response: { ok: false, status: 400 },
    })
    render(<FormPage page={namesPage(['sofosbuvir', '', 'velpatasvir'])} />)

    saveAndReturn()

    expect(await screen.findByText('Server says this one is repeated')).toBeInTheDocument()
    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
    expect(mocks.push).not.toHaveBeenCalled()
  })
})
