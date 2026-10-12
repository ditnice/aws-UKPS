import clsx from 'clsx'

import {
  ActionBanner as NdsActionBanner,
  type ActionBannerProps,
} from '@nice-digital/nds-action-banner'

import styles from './ActionBanner.module.scss'

export type { ActionBannerProps }

export function ActionBanner({ className, ...props }: ActionBannerProps) {
  return <NdsActionBanner {...props} className={clsx(styles.banner, className)} />
}
