import Link from 'next/link'

import { Button } from '@nice-digital/nds-button'

import { PageHeader } from '@/components/PageHeader/PageHeader'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'

import styles from './page.module.scss'

export default function RequestSumbitted() {
  return (
    <>
      <PageHeader heading="Account request submitted" />
      <p>
        Your request to access UK PharmaScan has been sent to your organisation&#39;s champion user
        for review
      </p>
      <hr></hr>
      <h2>What you told us</h2>
      <SummaryList variant="two-column" className={styles.marginBottom}>
        <SummaryListRow label="Organisation" value="Global Car Pharmaceuticals" />
        <SummaryListRow label="Full name" value="Julie Brooks" />
        <SummaryListRow label="Email address" value="test@test.com" />
        <SummaryListRow label="Contact number" value="+445628103821" />
      </SummaryList>
      <Link href="/portal">
        <Button>Return to UK PharmaScan home</Button>
      </Link>
    </>
  )
}
