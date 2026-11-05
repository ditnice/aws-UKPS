import { MigrateUpArgs, MigrateDownArgs, sql } from '@payloadcms/db-postgres'

export async function up({ db }: MigrateUpArgs): Promise<void> {
  await db.execute(sql`
   ALTER TABLE "pages" ADD COLUMN "page_header_preheading" varchar;
  ALTER TABLE "pages" ADD COLUMN "page_header_lead" varchar;
  ALTER TABLE "pages" ADD COLUMN "page_header_description" varchar;
  ALTER TABLE "_pages_v" ADD COLUMN "version_page_header_preheading" varchar;
  ALTER TABLE "_pages_v" ADD COLUMN "version_page_header_lead" varchar;
  ALTER TABLE "_pages_v" ADD COLUMN "version_page_header_description" varchar;`)
}

export async function down({ db }: MigrateDownArgs): Promise<void> {
  await db.execute(sql`
   ALTER TABLE "pages" DROP COLUMN "page_header_preheading";
  ALTER TABLE "pages" DROP COLUMN "page_header_lead";
  ALTER TABLE "pages" DROP COLUMN "page_header_description";
  ALTER TABLE "_pages_v" DROP COLUMN "version_page_header_preheading";
  ALTER TABLE "_pages_v" DROP COLUMN "version_page_header_lead";
  ALTER TABLE "_pages_v" DROP COLUMN "version_page_header_description";`)
}
