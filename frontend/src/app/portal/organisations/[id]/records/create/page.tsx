import { notFound } from 'next/navigation'

import { BackLinkBrowser } from '@/components/BackLinkBrowser/BackLinkBrowser'
import { PageHeader } from '@/components/PageHeader/PageHeader'
import { parsePositiveInteger } from '@/lib/valueParsing'

import CreateMedicineRecordForm from './CreateMedicineRecordForm'

type CreateMedicineRecordPageProps = {
  params: Promise<{ id: string }>
}
const CreateMedicineRecordPage = async ({ params }: CreateMedicineRecordPageProps) => {
  const { id } = await params
  const organisationIdNumber = parsePositiveInteger(id) ?? notFound()

  return (
    <>
      <PageHeader
        heading="Add medicine and associated product details"
        preheading="Medicine and associated product details"
        backLink={<BackLinkBrowser />}
      />
      <CreateMedicineRecordForm organisationId={organisationIdNumber} />
    </>
  )
}

export default CreateMedicineRecordPage
