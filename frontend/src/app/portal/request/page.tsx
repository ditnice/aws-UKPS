import Link from 'next/link'

import { Button } from '@nice-digital/nds-button'

import { PageHeader } from '@/components/PageHeader/PageHeader'

import styles from './page.module.scss'

export default function PortalDashboard() {
  return (
    <>
      <PageHeader heading="Request access to UK PharmaScan" />
      <p>
        Use this service to request access to UK PharmaScan. UK PharmaScan collects information
        about medicines in development. This helps the NHS and other organisations plan services and
        make informed decisions.
      </p>
      <p>
        Your organisation&#39;s champion user will review your request. We&#39;ll email you when
        your organisation&#39;s champion user makes a decision.
      </p>
      <h2>Before you start</h2>
      <p>You&#39;ll need:</p>
      <ul className={styles.marginBottom}>
        <li>the name of your organisation registered with UK PharmaScan</li>
        <li>a work email address from your organisation</li>
        <li>an authenticator application on your phone</li>
      </ul>
      <p className={styles.marginBottom}>
        You must work for, or on behalf of, an organisation registered to use UK PharmaScan. If your
        organisation is not registered, <a href="URL">register your organisation</a>.
      </p>
      <p className={styles.marginBottom}>It takes around 5 minutes to request access.</p>
      <Link href="/portal/request/provide-details">
        <Button variant={Button.variants.cta}>Start now</Button>
      </Link>
    </>
  )
}
