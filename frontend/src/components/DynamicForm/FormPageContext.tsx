'use client'

import { createContext, useContext } from 'react'

export type FormPageContextValue = {
  /**
   * The organisation that owns the record, for suggestions such as its company codes.
   *
   * `FormPage` always provides it, from `RecordPageDto.organisationId`. It is optional so that a
   * question rendered outside a form page, such as in a component test or the component library,
   * still works: without it, questions that would offer suggestions fall back to plain inputs.
   */
  organisationId?: number
}

const FormPageContext = createContext<FormPageContextValue>({})

export const FormPageContextProvider = FormPageContext.Provider

/** The record context of the form page a question is rendered on. */
export const useFormPageContext = () => useContext(FormPageContext)
