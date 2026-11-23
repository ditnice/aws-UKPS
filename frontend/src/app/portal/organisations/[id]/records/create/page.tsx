import { getUsersMe, UserRole } from '@/client/generated'
import { createServerApiClient } from '@/client/server-api'

import { NotPermitted } from '../../_components/NotPermitted'
import OrganisationPageWrapper from '../../_components/OrganisationPageWrapper'

import CreateMedicineRecordForm from './CreateMedicineRecordForm'
import { CreateRecordPageHeader } from './CreateRecordPageHeader'

type CreateMedicineRecordPageProps = {
  params: Promise<{ id: string }>
}
const CreateMedicineRecordPage = async ({ params }: CreateMedicineRecordPageProps) => {
  const { id: organisationId } = await params

  const apiClient = await createServerApiClient()
  const { data: currentUser } = await getUsersMe({ client: apiClient })

  if (currentUser?.userRole === UserRole.SUPER) {
    return <NotPermitted />
  }

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
// TODO: Check that the user has the correct AllowedPharmaceuticalEntity to be on this page
export default CreateMedicineRecordPage
