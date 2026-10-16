/** The URL of a page of a record revision's content form. */
export const recordPageHref = (recordId: number, revisionId: number, pageId: string) =>
  `/portal/records/${recordId}/revisions/${revisionId}/${pageId}`
