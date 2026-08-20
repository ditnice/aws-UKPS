import styles from './Footer.module.scss'
import { Legal } from './Legal/Legal'

export function Footer() {
  return (
    <footer className={styles.footer} data-component="footer">
      <div className={styles.container}>
        <p>Footer placeholder content</p>
      </div>
      <Legal />
    </footer>
  )
}
