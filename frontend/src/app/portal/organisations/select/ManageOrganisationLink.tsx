'use client'

import { useRouter } from 'next/navigation'

import { updateCurrentOrganisation } from '@/client/generated'
import { Button } from '@/components/Button/Button'

type ManageOrganisationLinkProps = {
  organisationId: number
  organisationName: string
}

const ManageOrganisationLink = ({
  organisationId,
  organisationName,
}: ManageOrganisationLinkProps) => {
  const router = useRouter()
  const href = `/portal/organisations/${organisationId}/records`

  const handleClick = async (event: React.MouseEvent<HTMLAnchorElement>) => {
    event.preventDefault()
    const response = await updateCurrentOrganisation({ body: { organisationId } })
    if (response.error) {
      router.push('?error=organisation-selection')
    } else {
      router.push(href)
    }
  }

  return (
    <Button
      data-testid="action-link"
      variant="link"
      onClick={handleClick}
      aria-label={`Manage Organisation - ${organisationName}`}
    >
      Manage
    </Button>
  )
}

export default ManageOrganisationLink
