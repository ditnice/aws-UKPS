import Link from 'next/link'

import { PharmaceuticalEntity } from '@/client/generated'
import { ActionBanner } from '@/components/ActionBanner/ActionBanner'
import { Button } from '@/components/Button/Button'

type CreateRecordActionBannerProps = {
  organisationId: string
  allowedPharmaceuticalEntity: PharmaceuticalEntity | undefined
}

export const CreateRecordActionBanner = ({
  organisationId,
  allowedPharmaceuticalEntity,
}: CreateRecordActionBannerProps) => {
  const canCreateMedicine =
    allowedPharmaceuticalEntity === PharmaceuticalEntity.MEDICINES ||
    allowedPharmaceuticalEntity === PharmaceuticalEntity.BOTH

  const canCreateVaccine =
    allowedPharmaceuticalEntity === PharmaceuticalEntity.VACCINES ||
    allowedPharmaceuticalEntity === PharmaceuticalEntity.BOTH

  return (
    <ActionBanner
      variant="subtle"
      title="Create Record"
      cta={
        <>
          {canCreateMedicine && (
            <Button
              elementType={Link}
              href={`/portal/organisations/${organisationId}/records/create`}
            >
              Create medicine record
            </Button>
          )}

          {canCreateVaccine && (
            <Button
              elementType={Link}
              href={`/portal/organisations/${organisationId}/records/vaccines/create`}
            >
              Create vaccine record
            </Button>
          )}
        </>
      }
    >
      To create a new record select the record type you want to create.{' '}
      <a>View a record template (opens in new tab)</a> to check what information you will need to
      provide.
    </ActionBanner>
  )
}
// TODO: Add the link to the button to create a vaccine record
