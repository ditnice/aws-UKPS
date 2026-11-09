import { ErrorState } from '@/components/Placeholder/ErrorState'

import { extractErrorMessage } from './getFieldErrorMessage'

const FormLevelErrors = <T,>({ errors }: { errors: T[] }) => {
  return (
    <>
      {errors?.length ? (
        <ErrorState>
          <div>
            {errors.map((e) => {
              const errorMessage = extractErrorMessage(e)
              return errorMessage ? <div key={errorMessage}>{errorMessage}</div> : <></>
            })}
          </div>
        </ErrorState>
      ) : (
        <></>
      )}
    </>
  )
}

export default FormLevelErrors
