import Link from 'next/link'

import { Button } from '@/components/Button/Button'
import { PageHeader } from '@/components/PageHeader/PageHeader'

export default function RegistrationInfo() {
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
      <ul>
        <li>the name of your organisation registered with UK PharmaScan</li>
        <li>a work email address from your organisation</li>
        <li>an authenticator application on your phone</li>
      </ul>
      <p>
        You must work for, or on behalf of, an organisation registered to use UK PharmaScan. If your
        organisation is not registered, <a href="URL">register your organisation</a>.
      </p>
      <p>It takes around 5 minutes to request access.</p>
      <Button elementType={Link} href="/register/provide-details" variant="cta">
        Start now
      </Button>
    </>
  )
}
