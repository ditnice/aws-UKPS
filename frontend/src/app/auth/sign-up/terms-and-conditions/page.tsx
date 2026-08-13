import Link from 'next/link'

import { Button } from '@nice-digital/nds-button'
import { Panel } from '@nice-digital/nds-panel'

import { PageHeader } from '@/components/PageHeader/PageHeader'

import styles from './page.module.scss'
import { PrintPageLink } from './PrintPageLink'

type SignUpTermsAndConditionsProps = {
  searchParams: Promise<{
    setupToken?: string
  }>
}

export default async function SignUpTermsAndConditions({
  searchParams,
}: SignUpTermsAndConditionsProps) {
  const setupToken = (await searchParams).setupToken?.trim()

  if (!setupToken) {
    return (
      <>
        <PageHeader heading="There is a problem with your sign-up link"></PageHeader>
        <p>This sign-up link is missing a setup token.</p>
      </>
    )
  }

  const setPasswordHref = `/auth/sign-up/set-password?${new URLSearchParams({
    setupToken,
  }).toString()}`

  return (
    <>
      <PageHeader heading="Terms and conditions"></PageHeader>

      <p>Read and accept the terms and conditions before continuing.</p>

      <Panel>
        By accepting, you confirm you will use UK PharmaScan only for authorised purposes in line
        with your organisation&apos;s data use agreement.
      </Panel>

      <p>[Full terms and conditions to be defined - placeholder]</p>

      <hr />

      <p>
        <strong>I confirm that I have read and agree to the terms and conditions.</strong>
      </p>

      <div className={styles.actions}>
        <Button elementType={Link} href={setPasswordHref} variant="cta">
          Accept and continue
        </Button>
      </div>

      <div className={styles.actions}>
        <PrintPageLink />
      </div>
    </>
  )
}
