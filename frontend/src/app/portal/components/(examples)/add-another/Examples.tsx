'use client'

import { useForm } from '@tanstack/react-form'

import { AddAnother } from '@/components/AddAnother/AddAnother'

import { Example } from '../../_components/Example'

export function Examples() {
  const form = useForm({
    defaultValues: {
      names: ['Example name', 'Another name'],
    },
  })

  return (
    <Example title="Repeatable text inputs">
      <form.Field name="names" mode="array">
        {(field) => (
          <AddAnother
            title="Other names"
            labelPrefix="Name"
            hint="Enter another name or identifier associated with this item."
            addItemLabel="Add another name"
            removeItemLabel="Remove name"
            field={field}
            getSubfield={(name, renderSubfield) => (
              <form.Field name={name}>{renderSubfield}</form.Field>
            )}
          />
        )}
      </form.Field>
    </Example>
  )
}
