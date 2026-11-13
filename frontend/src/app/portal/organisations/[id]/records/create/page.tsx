import OrganisationPageWrapper from '../../_components/OrganisationPageWrapper'

import CreateMedicineRecordForm from './CreateMedicineRecordForm'
import { CreateRecordPageHeader } from './CreateRecordPageHeader'

type CreateMedicineRecordPageProps = {
  params: Promise<{ id: string }>
}
const CreateMedicineRecordPage = async ({ params }: CreateMedicineRecordPageProps) => {
  const { id: organisationId } = await params

  return (
    <OrganisationPageWrapper organisationId={organisationId}>
      {(organisation) => (
        <>
          <CreateRecordPageHeader backHref={`/portal/organisations/${organisation.id}/records`} />
          <CreateMedicineRecordForm organisationId={organisation.id} />
        </>
      )}
    </OrganisationPageWrapper>
  )
}

export default CreateMedicineRecordPage
