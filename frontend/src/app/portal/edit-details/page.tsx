import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import { EditDetailsForm } from './_components/EditDetailsForm'

export default function EditDetails() {
  return (
    <>
      <PageHeader backLink={<BackLinkBrowser />} heading="Edit your details" />

      <EditDetailsForm />
    </>
  )
}
