'use client'

import Link from 'next/link'
import { useRouter } from 'next/navigation'

import { updateCurrentOrganisation } from '@/client/generated'

type ManageOrganisationLinkProps = {
  organisationId: number
}

const ManageOrganisationLink = ({ organisationId }: ManageOrganisationLinkProps) => {
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
    <Link data-testid="action-link" href={href} onClick={handleClick}>
      Manage
    </Link>
  )
}

export default ManageOrganisationLink
