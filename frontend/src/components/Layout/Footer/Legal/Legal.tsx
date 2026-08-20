import styles from './Legal.module.scss'

export function Legal() {
  return (
    <div className={styles.wrapper}>
      <div className={styles.container}>
        <div className={styles.layout}>
          <div className={styles.menuWrapper}>
            <nav className={styles.menu} aria-label="Legal menu">
              <ul>
                <li>
                  <a href="https://www.nice.org.uk/accessibility">Accessibility</a>
                </li>
                <li>
                  <a href="https://www.nice.org.uk/freedom-of-information">
                    Freedom of information
                  </a>
                </li>
                <li>
                  <a href="https://www.nice.org.uk/glossary">Glossary</a>
                </li>
                <li>
                  <a href="https://www.nice.org.uk/terms-and-conditions">Terms and conditions</a>
                </li>
                <li>
                  <a href="https://www.nice.org.uk/privacy-notice">Privacy notice</a>
                </li>
                <li>
                  <a href="https://www.nice.org.uk/cookies">Cookies</a>
                </li>
              </ul>
            </nav>
            <p className={styles.copyright}>
              &copy; <abbr title="National Institute for Health and Care Excellence">NICE</abbr>{' '}
              {new Date().getFullYear()}. All rights reserved. Subject to{' '}
              <a href="https://www.nice.org.uk/terms-and-conditions#notice-of-rights">
                Notice of rights
              </a>
              .
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
