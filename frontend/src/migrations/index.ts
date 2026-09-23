import * as migration_20260921_135429_initial_payload_schema from './20260921_135429_initial_payload_schema'

export const migrations = [
  {
    up: migration_20260921_135429_initial_payload_schema.up,
    down: migration_20260921_135429_initial_payload_schema.down,
    name: '20260921_135429_initial_payload_schema',
  },
]
