import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { RegistrationRequestForm } from './_components/RegistrationRequestForm'

export default function RegistrationRequest() {
  return (
    <>
      <PageHeader
        backLink={<BackLink href="/portal/register">Back</BackLink>}
        heading="Provide your details"
      />

      <RegistrationRequestForm />
    </>
  )
}
