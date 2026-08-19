import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { EditOrganisationForm } from './_components/EditOrganisationForm'

export default function EditOrganisation() {
  return (
    <>
      <PageHeader
        backLink={<BackLink href="#">Back</BackLink>}
        heading="Edit your company&#39;s details"
      />

      <EditOrganisationForm />
    </>
  )
}
