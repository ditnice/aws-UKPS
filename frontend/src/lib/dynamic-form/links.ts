import type { RecordPageDto } from '@/client/generated'

export const pageHref = (page: Pick<RecordPageDto, 'recordId'>, pageId: string) =>
  `/portal/records/${page.recordId}/${pageId}`
export const taskListHref = (recordId: number) => `/portal/records/${recordId}/task-list`
export const checkAnswersHref = (recordId: number) =>
  `/portal/records/${recordId}/check-your-answers`
/** The `returnTo` value on a page link that sends the user back to check your answers after saving. */
export const checkAnswersReturnTo = 'check-your-answers'
