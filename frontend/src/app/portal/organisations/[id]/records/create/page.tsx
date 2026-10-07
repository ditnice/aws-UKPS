import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import OrganisationPageWrapper from '../../_components/OrganisationPageWrapper'

import CreateMedicineRecordForm from './CreateMedicineRecordForm'

type CreateMedicineRecordPageProps = {
  params: Promise<{ id: string }>
}
const CreateMedicineRecordPage = async ({ params }: CreateMedicineRecordPageProps) => {
  const { id: organisationId } = await params

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <>
          <PageHeader
            heading="Add medicine and associated product details"
            preheading="Medicine and associated product details"
            backLink={<BackLinkBrowser />}
          />
          <CreateMedicineRecordForm organisationId={organisation.id} />
        </>
      )}
    </OrganisationPageWrapper>
  )
}

export default CreateMedicineRecordPage
