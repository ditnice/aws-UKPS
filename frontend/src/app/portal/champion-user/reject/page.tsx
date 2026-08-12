import { Button } from '@nice-digital/nds-button'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import styles from './page.module.scss'

export default function RequestSumbitted() {
  return (
    <>
      <PageHeader backLink={<BackLink href="#">Back</BackLink>} heading="Reject user" />
      <p>You are about to reject julie.brooks@email.com&#39;s request for an account.</p>
      <Button variant="cta" className={styles.marginRight}>
        Reject user
      </Button>
      <Button variant="secondary">Cancel</Button>
    </>
  )
}
