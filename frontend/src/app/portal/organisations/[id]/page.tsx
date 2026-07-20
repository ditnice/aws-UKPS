import { notFound } from 'next/navigation'
import { Suspense } from 'react'

import { Button } from '@nice-digital/nds-button'
import { PageHeader } from '@nice-digital/nds-page-header'

import { getOrganisationById } from '@/client/generated/sdk.gen'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'

import { OrganisationUsersTable } from './_components/OrganisationUsersTable'

interface Props {
  params: Promise<{ id: string }>
  searchParams: Promise<{ page?: string }>
}

function parsePage(page: string | undefined): number {
  const parsedPage = Number(page)

  return Number.isInteger(parsedPage) && parsedPage >= 1 ? parsedPage : 1
}

export default async function OrganisationPage({ params, searchParams }: Props) {
  const { id } = await params
  const { page } = await searchParams
  const organisationId = Number(id)
  const currentPage = parsePage(page)

  if (!Number.isInteger(organisationId)) {
    notFound()
  }

  const { data: organisation, error } = await getOrganisationById({
    path: { id: organisationId },
  })

  if (error || !organisation) {
    return (
      <section>
        <h1>Unable to load organisation</h1>
        <p role="alert">There was a problem retrieving the organisation. Please try again later.</p>
      </section>
    )
  }

  return (
    <>
      <PageHeader heading={organisation.organisationName} verticalPadding="loose" />

      <h2>Company details</h2>
      <SummaryList>
        <SummaryListRow
          label="Company or organisation type"
          value={organisation.organisationType}
        />
        <SummaryListRow label="Company name" value={organisation.organisationName} />
        <SummaryListRow label="Head office address" value={organisation.headOfficeAddress} />
        <SummaryListRow
          label="Head office email address"
          value={organisation.headOfficeEmail}
        ></SummaryListRow>
        <SummaryListRow
          label="Head office phone number"
          value={organisation.headOfficeTelephone}
        ></SummaryListRow>
      </SummaryList>

      <Button variant={Button.variants.secondary}>Edit details</Button>

      <h2>Search and filter</h2>
      <Suspense fallback={<p>Loading users...</p>} key={currentPage}>
        <OrganisationUsersTable currentPage={currentPage} organisationId={organisationId} />
      </Suspense>
    </>
  )
}
