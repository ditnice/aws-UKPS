import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import FormLevelErrors from './FormLevelErrors'
import { extractErrorMessage } from './getFieldErrorMessage'

vi.mock('@/components/Placeholder/ErrorState', () => ({
  ErrorState: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="error-state">{children}</div>
  ),
}))

vi.mock('./getFieldErrorMessage', () => ({
  extractErrorMessage: vi.fn(),
}))

const mockExtractErrorMessage = vi.mocked(extractErrorMessage)

describe('FormLevelErrors', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Default: treat each error as a string message.
    mockExtractErrorMessage.mockImplementation((e: unknown) => e as string)
  })

  it('renders nothing when errors is an empty array', () => {
    const { container } = render(<FormLevelErrors errors={[]} />)

    expect(container).toBeEmptyDOMElement()
    expect(screen.queryByTestId('error-state')).not.toBeInTheDocument()
    expect(mockExtractErrorMessage).not.toHaveBeenCalled()
  })

  it('renders nothing when errors is undefined', () => {
    const { container } = render(<FormLevelErrors errors={undefined as unknown as string[]} />)

    expect(container).toBeEmptyDOMElement()
  })

  it('renders a single error message inside ErrorState', () => {
    render(<FormLevelErrors errors={['Something went wrong']} />)

    const errorState = screen.getByTestId('error-state')
    expect(errorState).toBeInTheDocument()
    expect(errorState).toHaveTextContent('Something went wrong')
  })

  it('renders every error message when there are multiple errors', () => {
    const errors = ['First error', 'Second error', 'Third error']
    render(<FormLevelErrors errors={errors} />)

    errors.forEach((e) => {
      expect(screen.getByText(e)).toBeInTheDocument()
    })
  })

  it('calls extractErrorMessage once for each error', () => {
    const errors = ['a', 'b']
    render(<FormLevelErrors errors={errors} />)

    expect(mockExtractErrorMessage).toHaveBeenCalledTimes(2)
    expect(mockExtractErrorMessage).toHaveBeenNthCalledWith(1, 'a')
    expect(mockExtractErrorMessage).toHaveBeenNthCalledWith(2, 'b')
  })

  it('works with non-string error objects via extractErrorMessage', () => {
    const errors = [{ message: 'Name is required' }, { message: 'Email is invalid' }]
    mockExtractErrorMessage.mockImplementation((e: unknown) => (e as { message: string }).message)

    render(<FormLevelErrors errors={errors} />)

    expect(screen.getByText('Name is required')).toBeInTheDocument()
    expect(screen.getByText('Email is invalid')).toBeInTheDocument()
  })

  it('skips errors for which no message can be extracted', () => {
    mockExtractErrorMessage.mockImplementation((e: unknown) =>
      e === 'bad' ? undefined : (e as string),
    )

    render(<FormLevelErrors errors={['Valid error', 'bad', 'Another valid error']} />)

    expect(screen.getByText('Valid error')).toBeInTheDocument()
    expect(screen.getByText('Another valid error')).toBeInTheDocument()
    expect(screen.getByTestId('error-state').firstElementChild?.children).toHaveLength(2)
  })

  it('still renders ErrorState (with no messages) when every message is empty', () => {
    mockExtractErrorMessage.mockReturnValue(undefined)

    render(<FormLevelErrors errors={['x', 'y']} />)

    const errorState = screen.getByTestId('error-state')
    expect(errorState).toBeInTheDocument()
    expect(errorState).toHaveTextContent('')
  })
})
