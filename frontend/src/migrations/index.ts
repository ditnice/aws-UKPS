import * as migration_20260921_135429_initial_payload_schema from './20260921_135429_initial_payload_schema'
import * as migration_20261102_161451_add_users_reset_password_requested_at from './20261102_161451_add_users_reset_password_requested_at'
import * as migration_20261102_161452_add_page_header from './20261102_161452_add_page_header'

export const migrations = [
  {
    up: migration_20260921_135429_initial_payload_schema.up,
    down: migration_20260921_135429_initial_payload_schema.down,
    name: '20260921_135429_initial_payload_schema',
  },
  {
    up: migration_20261102_161451_add_users_reset_password_requested_at.up,
    down: migration_20261102_161451_add_users_reset_password_requested_at.down,
    name: '20261102_161451_add_users_reset_password_requested_at',
  },
  {
    up: migration_20261102_161452_add_page_header.up,
    down: migration_20261102_161452_add_page_header.down,
    name: '20261102_161452_add_page_header',
  },
]
