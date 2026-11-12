import {
  formOptions,
  initialFormState,
  mergeForm,
  useForm,
  useTransform,
} from '@tanstack/react-form-nextjs'
import { useActionState } from 'react'
import z from 'zod'

export const formSchema = z.object({
  value: z.email('Enter a valid email address'),
})

type UpdateUserDetailsFormValue = {
  value: string
}

const updateUserDetailOptions = formOptions({
  defaultValues: {
    value: '',
  } satisfies UpdateUserDetailsFormValue,
  validators: {
    onDynamic: formSchema,
  },
})

const updateUserDetailsAction = async (prev: unknown, formData: FormData) => {
  return prev
}

type EditFullNameFormProps = {}
const EditFullNameForm = ({}: EditFullNameFormProps) => {
  const [state, action] = useActionState(updateUserDetailsAction, initialFormState)
  //   const form = useForm({
  //     ...updateUserDetailOptions,
  //     transform: useTransform(
  //       (baseForm) =>
  //         mergeForm(baseForm, {
  //           ...state!,
  //           values: { ...updateUserDetailOptions.defaultValues, ...state?.values },
  //         }),
  //       [state],
  //     ),
  //   })
  return <></>
}

export default EditFullNameForm
