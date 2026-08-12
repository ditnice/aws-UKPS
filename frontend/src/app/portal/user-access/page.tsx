import { Button } from '@nice-digital/nds-button'

import { BackLink } from '@/components/BackLink/BackLink'
import { PageHeader } from '@/components/PageHeader/PageHeader'

import styles from './page.module.scss'

export default function RequestSumbitted() {
  return (
    <>
      <PageHeader
        backLink={<BackLink href="#">Back</BackLink>}
        heading="Manage user&#39;s access"
      />
      <p>julie.brooks@example.com is a standard user.</p>
      <p>Choose what you want to do:</p>
      <ul className={styles.marginBottom}>
        <li>Change permissions - chnage what the user can do.</li>
        <li>
          Deactivate user - temporarily stop the user&#39;s access. You can reactivate them later.
        </li>
        <li>Remove user - permanently remove the user&#39;s access.</li>
      </ul>

      <Button variant="cta" className={styles.marginRight}>
        Change permissions
      </Button>
      <Button variant="secondary" className={styles.marginRight}>
        Deactivate user
      </Button>
      <Button variant="secondary">Remove user</Button>

      <div className={styles.marginTop}>
        <a href="URL">Cancel</a>
      </div>
    </>
  )
}
