import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { EditDetailsForm } from './_components/EditDetailsForm'

export default function EditDetails() {
  return (
    <>
      <PageHeader backLink={<BackLink href="#">Back</BackLink>} heading="Edit your details" />

      <EditDetailsForm />
    </>
  )
}
