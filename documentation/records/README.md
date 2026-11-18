# Records core logic

How medicine and vaccine records are created, edited, submitted and published.

| Document | Covers |
| --- | --- |
| [Record lifecycle](lifecycle.md) | Every row written at each stage, from the first save to archive and discard |
| [Status management](status-management.md) | Which record and revision statuses can coexist, the transitions between them, the actions offered and the status displayed |
| [Validation and concurrency](validation-and-concurrency.md) | Page rules, cross-page rules, check your answers and submit, and how concurrent edits are detected |

## TODO

- [ ] Update the [validation and concurrency guide](validation-and-concurrency.md) to describe actual `xmin` optimistic concurrency rather than row locking, the `revisionHash` name and scope, actual conflict codes, and no-change save behaviour.
- [ ] In that guide, distinguish planned check-your-answers, draft copy-forward and vaccine publication from implemented functionality.
