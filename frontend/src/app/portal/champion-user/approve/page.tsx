import { Button } from '@nice-digital/nds-button'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import styles from './page.module.scss'

export default function ApproveUser() {
  return (
    <>
      <PageHeader backLink={<BackLink href="#">Back</BackLink>} heading="Approve user" />
      <p>You are about to approve julie.brooks@email.com&#39;s request for an account.</p>
      <p className={styles.marginBottom}>
        Once aproved they will be able to access your organisations UKPS account.
      </p>
      <Button variant="cta" className={styles.marginRight}>
        Approve user
      </Button>
      <Button variant="secondary">Cancel</Button>
    </>
  )
}
