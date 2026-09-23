import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import ArrayInput, { ArrayInputProps, Field } from './ArrayInput'

const defaultProps: ArrayInputProps<string> = {
  addItemLabel: 'add-item-label',
  removeItemLabel: 'remove-item-label',
  labelPrefix: 'label-prefix',
  hint: 'hint',
  field: {
    name: 'test-field',
    state: {
      value: ['1', '2', '3'],
      meta: { errors: [] },
    },
    pushValue: vi.fn(),
    removeValue: vi.fn(),
  },
  getSubfield: (name, subfieldRender) => {
    return subfieldRender({
      name,
      handleBlur: vi.fn(),
      state: { value: name, meta: { errors: [] } },
      handleChange: vi.fn(),
    })
  },
}

const renderComponent = (overrides?: Partial<ArrayInputProps<string>>) => {
  render(<ArrayInput {...{ ...defaultProps, ...overrides }} />)
}

const getInputByLabelText = (labelText: string): HTMLInputElement => {
  const input = screen.getByLabelText(labelText)
  expect(input).toBeInstanceOf(HTMLInputElement)
  return input as HTMLInputElement
}

afterEach(cleanup)

describe('ArrayInput', () => {
  it('should render', () => {
    renderComponent()

    const input = screen.getByLabelText(defaultProps.labelPrefix)

    expect(input).toBeDefined()
  })

  it('should render an input for each value', () => {
    renderComponent()

    expect(getInputByLabelText('label-prefix')).toBeDefined()
    expect(getInputByLabelText('label-prefix 2')).toBeDefined()
    expect(getInputByLabelText('label-prefix 3')).toBeDefined()
  })

  it('should render each input with the expected name', () => {
    renderComponent()

    expect(getInputByLabelText('label-prefix').getAttribute('name')).toBe('test-field[0]')
    expect(getInputByLabelText('label-prefix 2').getAttribute('name')).toBe('test-field[1]')
    expect(getInputByLabelText('label-prefix 3').getAttribute('name')).toBe('test-field[2]')
  })

  it('should render each input with the expected value', () => {
    renderComponent()

    expect(getInputByLabelText('label-prefix').value).toBe('test-field[0]')
    expect(getInputByLabelText('label-prefix 2').value).toBe('test-field[1]')
    expect(getInputByLabelText('label-prefix 3').value).toBe('test-field[2]')
  })

  it('should render the hint on the first input only', () => {
    renderComponent()

    expect(screen.getByText(defaultProps.hint)).toBeDefined()
  })

  it('should not render a remove button for the first item', () => {
    renderComponent()

    expect(screen.queryByTestId('remove-button-0')).toBeNull()
  })

  it('should render a remove button for each subsequent item', () => {
    renderComponent()

    expect(screen.getByTestId('remove-button-1')).toBeDefined()
    expect(screen.getByTestId('remove-button-2')).toBeDefined()
  })

  it('should render the remove button with the configured label', () => {
    renderComponent()

    expect(screen.getByTestId('remove-button-1').innerHTML).toBe(defaultProps.removeItemLabel)
  })

  it('should call removeValue with the item index when a remove button is clicked', () => {
    const removeValue = vi.fn()

    renderComponent({
      field: {
        ...defaultProps.field,
        removeValue,
      },
    })

    fireEvent.click(screen.getByTestId('remove-button-1'))

    expect(removeValue).toHaveBeenCalledWith(1)
  })

  it('should call removeValue with the correct index when removing the last item', () => {
    const removeValue = vi.fn()

    renderComponent({
      field: {
        ...defaultProps.field,
        removeValue,
      },
    })

    fireEvent.click(screen.getByTestId('remove-button-2'))

    expect(removeValue).toHaveBeenCalledWith(2)
  })

  it('should render the add button with the configured label', () => {
    renderComponent()

    expect(screen.getByTestId('add-item-button').textContent).toBe(defaultProps.addItemLabel)
  })

  it('should call pushValue with an empty string when the add button is clicked', () => {
    const pushValue = vi.fn()

    renderComponent({
      field: {
        ...defaultProps.field,
        pushValue,
      },
    })

    fireEvent.click(screen.getByTestId('add-item-button'))

    expect(pushValue).toHaveBeenCalledWith('')
  })

  it('should call pushValue only once when the add button is clicked once', () => {
    const pushValue = vi.fn()

    renderComponent({
      field: {
        ...defaultProps.field,
        pushValue,
      },
    })

    fireEvent.click(screen.getByTestId('add-item-button'))

    expect(pushValue).toHaveBeenCalledTimes(1)
  })

  it('should call subfield handleChange when an input changes', () => {
    const handleChange = vi.fn()

    const getSubfield: ArrayInputProps<string>['getSubfield'] = (name, subfieldRender) =>
      subfieldRender({
        name,
        handleBlur: vi.fn(),
        state: {
          value: name,
          meta: { errors: [] },
        },
        handleChange,
      })

    renderComponent({ getSubfield })

    fireEvent.change(screen.getByLabelText('label-prefix 2'), {
      target: { value: 'new value' },
    })

    expect(handleChange).toHaveBeenCalledWith('new value')
  })

  it('should call subfield handleBlur when an input loses focus', () => {
    const handleBlur = vi.fn()

    const getSubfield: ArrayInputProps<string>['getSubfield'] = (name, subfieldRender) =>
      subfieldRender({
        name,
        handleBlur,
        state: {
          value: name,
          meta: { errors: [] },
        },
        handleChange: vi.fn(),
      })

    renderComponent({ getSubfield })

    fireEvent.blur(screen.getByLabelText('label-prefix'))

    expect(handleBlur).toHaveBeenCalledTimes(1)
  })

  it('should render the field error', () => {
    renderComponent({
      field: {
        ...defaultProps.field,
        state: {
          ...defaultProps.field.state,
          meta: {
            errors: ['There is an error'],
          },
        },
      },
    })

    expect(screen.getByText('There is an error')).toBeDefined()
  })

  it('should render a subfield error', () => {
    const getSubfield: ArrayInputProps<string>['getSubfield'] = (name, subfieldRender) =>
      subfieldRender({
        name,
        handleBlur: vi.fn(),
        state: {
          value: name,
          meta: {
            errors: ['Invalid value'],
          },
        },
        handleChange: vi.fn(),
      })

    renderComponent({ getSubfield })

    expect(screen.getAllByText('Invalid value')).toHaveLength(3)
  })

  it('should render an error state on an input when the subfield has an error', () => {
    const getSubfield: ArrayInputProps<string>['getSubfield'] = (name, subfieldRender) =>
      subfieldRender({
        name,
        handleBlur: vi.fn(),
        state: {
          value: name,
          meta: {
            errors: ['Invalid value'],
          },
        },
        handleChange: vi.fn(),
      })

    renderComponent({ getSubfield })

    expect(screen.getByLabelText('label-prefix').getAttribute('aria-invalid')).toBe('true')
  })

  it('should render no inputs when the field has no values', () => {
    renderComponent({
      field: {
        ...defaultProps.field,
        state: {
          value: [],
          meta: { errors: [] },
        },
      },
    })

    expect(screen.queryByLabelText('label-prefix')).toBeNull()
    expect(screen.getByTestId('add-item-button')).toBeDefined()
  })
})
