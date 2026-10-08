import { PublishedRecordDtoPublishedMedicineRecordDto } from '@/client/generated'
import { SummaryList, SummaryListRow } from '@/components/SummaryList/SummaryList'

import {
  biomarkerTypeLabels,
  designationStatusLabels,
  eamsOpinionDecisionLabels,
  formatEnum,
  formatEnumList,
  formatList,
  formatReferenceData,
  formatReferenceDataList,
  formatRegulatoryDate,
  formatText,
  genomicTestMandatoryStatusLabels,
  genomicTestNgtdRelationshipLabels,
  indicationPaediatricStatusLabels,
  medicineHtaAssessorLabels,
  medicineTechnologyStatusLabels,
  netUkBudgetImpactBandLabels,
  nhsServiceChangesRequiredLabels,
  notProvidedText,
  patientAccessSchemeRegionLabels,
  yesNoUnknownLabels,
} from '../_lib/recordValueFormatters'

import { Accordion, AccordionGroup } from './RecordAccordion'

type MedicineRecordDetailsProps = {
  record: PublishedRecordDtoPublishedMedicineRecordDto
}

type SectionProps = MedicineRecordDetailsProps

/**
 * Renders a published medicine record's data, grouped into sections.
 */
export const MedicineRecordDetails = ({ record }: MedicineRecordDetailsProps) => {
  return (
    <AccordionGroup>
      <Accordion title="Product record details" displayTitleAsHeading headingLevel={3}>
        <ProductRecordDetailsSection record={record} />
      </Accordion>
      <Accordion
        title="Indication and development information"
        displayTitleAsHeading
        headingLevel={3}
      >
        <IndicationAndDevelopmentSection record={record} />
      </Accordion>
      <Accordion title="Clinical trial information" displayTitleAsHeading headingLevel={3}>
        <ClinicalTrialsSection record={record} />
      </Accordion>
      <Accordion
        title="Regulatory, access and launch information"
        displayTitleAsHeading
        headingLevel={3}
      >
        <RegulatoryAccessAndLaunchSection record={record} />
      </Accordion>
      <Accordion
        title="Service readiness information (optional)"
        displayTitleAsHeading
        headingLevel={3}
      >
        <ServiceReadinessSection record={record} />
      </Accordion>
    </AccordionGroup>
  )
}

const ProductRecordDetailsSection = ({ record }: SectionProps) => {
  const productDetail = record.recordProductDetail
  const namesOfType = (nameType: 'GenericName' | 'OtherIdentifier') =>
    formatList(
      productDetail?.namesAndIdentifiers.filter((x) => x.nameType === nameType).map((x) => x.name),
    )

  return (
    <>
      <SummaryList title="Names and identifiers">
        <SummaryListRow label="Company code" value={formatText(productDetail?.companyCode)} />
        <SummaryListRow label="Other names or identifiers" value={namesOfType('OtherIdentifier')} />
        <SummaryListRow label="Generic names" value={namesOfType('GenericName')} />
        <SummaryListRow
          label="Branded name (optional)"
          value={formatText(productDetail?.brandedName)}
        />
        <SummaryListRow label="Record title" value={formatText(productDetail?.recordTitle)} />
      </SummaryList>
    </>
  )
}

const IndicationAndDevelopmentSection = ({ record }: SectionProps) => {
  const indication = record.medicinesIndicationDetail
  const background = record.medicinesDevelopmentBackground

  return (
    <>
      <SummaryList title="Indication details">
        <SummaryListRow
          label="What is the indication this product is seeking a licence for?"
          value={formatText(indication?.indication)}
        />
        <SummaryListRow label="BNF chapter" value={formatReferenceData(indication?.bnfChapter)} />
        <SummaryListRow
          label="Therapeutic area (optional)"
          value={formatReferenceDataList(indication?.therapeuticAreas)}
        />
        <SummaryListRow
          label="Is this product intended to treat children?"
          value={formatEnum(indication?.indicationIsPaediatric, indicationPaediatricStatusLabels)}
        />
        <SummaryListRow
          label="Is this product intended to treat cancer?"
          value={formatEnum(indication?.indicationIsCancer, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Is this product intended to treat a rare disease?"
          value={formatEnum(indication?.indicationIsRareDisease, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Is this product a personalised medicine?"
          value={formatEnum(indication?.isPersonalisedMedicine, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Formulation"
          value={formatReferenceData(indication?.formulationType)}
        />
        <SummaryListRow label="Presentation" value={formatText(indication?.presentation)} />
        <SummaryListRow label="Mode of action" value={formatText(indication?.modeOfAction)} />
        <SummaryListRow
          label="Proposed dose regimen (optional)"
          value={formatText(indication?.proposedDoseRegimen)}
        />
        <SummaryListRow
          label="Technology status"
          value={formatEnumList(
            indication?.medicineTechnologyStatus,
            medicineTechnologyStatusLabels,
          )}
        />
      </SummaryList>

      <SummaryList title="Development background">
        <SummaryListRow
          label="Is this product a repurposed medicine?"
          value={formatEnum(background?.isRepurposedMedicine, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Differences from current licensed indications"
          value={formatText(background?.repurposedMedicineDetails)}
        />
        <SummaryListRow
          label="Is your company the original developer of this product?"
          value={formatEnum(background?.isOriginatorCompany, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Originator company name"
          value={formatText(background?.originatorCompanyName)}
        />
        <SummaryListRow
          label="Is this product co-marketed?"
          value={formatEnum(background?.isCoMarketed, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Co-marketing company name"
          value={formatText(background?.coMarketingCompanyName)}
        />
      </SummaryList>
    </>
  )
}

const ClinicalTrialsSection = ({ record }: SectionProps) => {
  const recruitingInUk = record.recordClinicalTrialInformation?.recruitingInUk

  return (
    <>
      {record.recordClinicalTrials.length === 0 ? (
        <SummaryList>
          <SummaryListRow label="Clinical trials" value={notProvidedText} />
        </SummaryList>
      ) : (
        record.recordClinicalTrials.map((trial, index) => (
          <SummaryList key={trial.clinicalTrialsGovNumber} title={`Trial ${index + 1}`}>
            <SummaryListRow label="Study name" value={trial.studyName} />
            <SummaryListRow
              label="ClinicalTrials.gov number"
              value={formatText(trial.clinicalTrialsGovNumber)}
            />
            <SummaryListRow
              label="Trial number from other registry (optional)"
              value={formatList(trial.otherClinicalTrialNumbers)}
            />
          </SummaryList>
        ))
      )}
      <SummaryList>
        <SummaryListRow
          label="Any trials recruiting in the UK"
          value={formatEnum(recruitingInUk, yesNoUnknownLabels)}
        />
      </SummaryList>
    </>
  )
}

const RegulatoryAccessAndLaunchSection = ({ record }: SectionProps) => {
  const procedure = record.recordMhraProcedure
  const mhraDate = record.recordMhraDate
  const globalSubmission = record.medicinesGlobalSubmission
  const intlRecognition = record.medicinesIntlRecognition
  const hta = record.recordHta
  const euStatus = record.medicinesEuStatus
  const eamsPim = record.medicinesEamsPim

  return (
    <>
      <SummaryList title="MHRA procedure and dates">
        <SummaryListRow
          label="Select the procedure you currently expect to use for UK authorisation"
          value={formatReferenceData(procedure?.mhraProcedureType)}
        />
        <SummaryListRow
          label="MHRA regulatory procedure details (optional)"
          value={formatText(procedure?.procedureDetails)}
        />
        <SummaryListRow
          label="What is the UK regulatory submission date?"
          value={formatRegulatoryDate(mhraDate?.ukSubmissionDate)}
        />
        <SummaryListRow
          label="What is the global first submission region?"
          value={formatText(globalSubmission?.globalFirstSubmissionRegion)}
        />
        <SummaryListRow
          label="What is the global first submission date?"
          value={formatRegulatoryDate(globalSubmission?.globalSubmissionActualDate)}
        />
        <SummaryListRow
          label="Is conditional approval anticipated in the UK?"
          value={formatEnum(mhraDate?.ukConditionalApprovalAnticipated, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="What is the UK regulatory licence date?"
          value={formatRegulatoryDate(mhraDate?.ukLicenceDate)}
        />
        <SummaryListRow
          label="Who is the IRP reference regulator?"
          value={formatReferenceData(procedure?.irpReferenceRegulator)}
        />
        <SummaryListRow
          label="What is the IRP intended route?"
          value={formatReferenceData(intlRecognition?.irpRoute)}
        />
        <SummaryListRow
          label="What is the IRP regulatory submission date?"
          value={formatRegulatoryDate(intlRecognition?.intlSubmissionDate)}
        />
        <SummaryListRow
          label="What is the IRP regulatory licence date?"
          value={formatRegulatoryDate(intlRecognition?.intlLicenceDate)}
        />
        <SummaryListRow
          label="Is international conditional approval anticipated for this product?"
          value={formatEnum(
            intlRecognition?.intlConditionalApprovalAnticipated,
            yesNoUnknownLabels,
          )}
        />
      </SummaryList>

      <SummaryList title="Health technology assessment and launch">
        <SummaryListRow
          label="Do you intend to submit for a HTA?"
          value={formatEnum(hta?.medicineHtaSubmissionIntended, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Select the submission bodies you intend to submit to for a HTA"
          value={formatEnumList(hta?.medicineHtaBodies, medicineHtaAssessorLabels)}
        />
        <SummaryListRow
          label="AAdd additional details or clarifications, such as any planned differences in the indications submitted to each HTA."
          value={formatText(hta?.htaAdditionalDetails)}
        />
        <SummaryListRow
          label="Do you intend to follow the MHRA/NICE aligned pathway?"
          value={formatEnum(hta?.htaNiceAlignedPathway, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Enter the NICE technology appraisal development ID for this product (optional)"
          value={formatText(hta?.niceTaDevelopmentId)}
        />
        <SummaryListRow
          label="What is the UK commercial launch date?"
          value={formatRegulatoryDate(hta?.ukLaunchDate)}
        />
      </SummaryList>

      <SummaryList title="Special designations">
        <SummaryListRow
          label="Have you or will you apply for orphan medicine status in the EU?"
          value={formatEnum(euStatus?.euOrphanStatus, designationStatusLabels)}
        />
        <SummaryListRow
          label="Date EU orphan medicine status was granted"
          value={formatRegulatoryDate(euStatus?.euOrphanGrantedDate)}
        />
        <SummaryListRow
          label="EU orphan status number"
          value={formatText(euStatus?.euOrphanStatusNumber)}
        />
        <SummaryListRow
          label="Is this product classified as an ATMP in the EU?"
          value={formatEnum(euStatus?.euAtmpClassificationStatus, designationStatusLabels)}
        />
        <SummaryListRow
          label="Date ATMP classification recommendation was provided"
          value={formatRegulatoryDate(euStatus?.atmpRecommendationDate)}
        />
        <SummaryListRow
          label="ATMP classification outcome"
          value={formatReferenceData(euStatus?.atmpClassification)}
        />
        <SummaryListRow
          label="Do you have, or are you seeking, MHRA PIM designation?"
          value={formatEnum(eamsPim?.pimDesignationStatus, designationStatusLabels)}
        />
        <SummaryListRow
          label="Have you already or do you plan to submit to the Early Access to Medicines Scheme (EAMS)?"
          value={formatEnum(eamsPim?.willSubmitToEams, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="What is the EAMS submission date?"
          value={formatRegulatoryDate(eamsPim?.eamsSubmissionDate)}
        />
        <SummaryListRow
          label="Date of EAMS scientific opinion"
          value={formatRegulatoryDate(eamsPim?.eamsOpinionDate)}
        />
        <SummaryListRow
          label="What was the EAMS scientific opinion decision?"
          value={formatEnum(eamsPim?.eamsOpinionDecision, eamsOpinionDecisionLabels)}
        />
      </SummaryList>
    </>
  )
}

const ServiceReadinessSection = ({ record }: SectionProps) => {
  const testing = record.medicinesLaboratoryTesting
  const identification = record.medicinesPatientIdentification
  const treatment = record.medicinesTreatmentDetail
  const serviceImpact = record.medicinesServiceImpact
  const budgetImpact = record.medicinesBudgetImpact

  return (
    <>
      <SummaryList title="Laboratory testing details">
        <SummaryListRow
          label="Does the product require a specific biomarker or diagnostic test result before prescribing?"
          value={formatEnum(testing?.diagnosticTestRequired, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="What type of biomarker required for this test?"
          value={formatEnum(testing?.biomarkerType, biomarkerTypeLabels)}
        />
        <SummaryListRow
          label="Describe the non-genomic biomarker"
          value={formatText(testing?.nonGenomicBiomarkerDescription)}
        />
        <SummaryListRow
          label="What is the genomic target for this test?"
          value={formatText(testing?.genomicTarget)}
        />
        <SummaryListRow
          label="How does this test relate to what's currently in the National Genomic Test Directory (NGTD)?"
          value={formatEnum(
            testing?.genomicTestNgtdRelationship,
            genomicTestNgtdRelationshipLabels,
          )}
        />
        <SummaryListRow
          label="What sample type is needed?"
          value={formatText(testing?.genomicSampleType)}
        />
        <SummaryListRow
          label="Describe any turnaround time requirements for this test that differ from the current routinely commissioned testing pathway (optional)"
          value={formatText(testing?.genomicTurnaroundTimeDetails)}
        />
        <SummaryListRow
          label="At which point of the patient pathway should the test be conducted?"
          value={formatReferenceData(testing?.patientPathwayPoint)}
        />
        <SummaryListRow
          label="Timepoint of genomic test in patient pathway"
          value={formatText(testing?.genomicTestPathwayPointOther)}
        />
        <SummaryListRow
          label="What genomic alterations determine patient eligibility?"
          value={formatText(testing?.genomicAlterations)}
        />
        <SummaryListRow
          label="Are there additional genomic factors that affect treatment selection or sequencing?"
          value={formatText(testing?.additionalGenomicFactors)}
        />
        <SummaryListRow
          label="What test was used in trials?"
          value={formatText(testing?.genomicTestUsedInTrials)}
        />
        <SummaryListRow
          label="Enter the minimum specificity and sensitivity used"
          value={formatText(testing?.genomicTestSpecificitySensitivity)}
        />
        <SummaryListRow
          label="Is this test mandatory before treatment can start?"
          value={formatEnum(testing?.genomicTestMandatoryStatus, genomicTestMandatoryStatusLabels)}
        />
        <SummaryListRow
          label="Enter any notes about uncertainties, assumptions, or service considerations relating to the tests"
          value={formatText(testing?.genomicTestNotes)}
        />
        <SummaryListRow
          label="Describe any tests (genomic tests or otherwise) needed to monitor response to treatment beyond what is currently offered in NHS practice"
          value={formatText(testing?.monitoringTestsDetails)}
        />
        <SummaryListRow
          label="Describe any tests needed to assess safety of the treatment that are additional to what is currently used in routine clinical practice"
          value={formatText(testing?.safetyTestsDetails)}
        />
      </SummaryList>

      <SummaryList title="Patient and clinical requirements">
        <SummaryListRow
          label="Is a screening or surveillance programme required to identify eligible patients?"
          value={formatEnum(identification?.screeningRequired, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Briefly describe the type of screening or surveillance needed"
          value={formatText(identification?.screeningDetails)}
        />
        <SummaryListRow
          label="Do patients need to be identified for treatment more quickly than standard practice?"
          value={formatEnum(identification?.urgentIdentificationRequired, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Urgent identification details"
          value={formatText(identification?.urgentIdentificationDetails)}
        />
        <SummaryListRow
          label="Enter the proposed place in therapy and likely comparators"
          value={formatText(treatment?.proposedPlaceInTherapy)}
        />
        <SummaryListRow
          label="Enter the estimated duration of treatment"
          value={formatText(treatment?.estimatedDurationOfTreatment)}
        />
        <SummaryListRow
          label="Are any changes to NHS services required for the delivery of this product?"
          value={formatEnum(
            serviceImpact?.nhsServiceChangesRequired,
            nhsServiceChangesRequiredLabels,
          )}
        />
        <SummaryListRow
          label="Describe any changes to staffing, equipment, pathway, or commissioning needed for delivery of this medicine"
          value={formatText(serviceImpact?.nhsServiceChangesDetails)}
        />
        <SummaryListRow
          label="Are there specific requirements for the handling or storage of this product that may cause service issues?"
          value={formatEnum(serviceImpact?.handlingStorageRequirements, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Handling or storage details"
          value={formatText(serviceImpact?.handlingStorageDetails)}
        />
        <SummaryListRow
          label="Enter the UK patient population range for the condition"
          value={formatReferenceData(serviceImpact?.ukPatientPopulationRange)}
        />
        <SummaryListRow
          label="UK patient population notes"
          value={formatText(serviceImpact?.ukPatientPopulationNotes)}
        />
        <SummaryListRow
          label="Enter the estimated UK eligible population range"
          value={formatText(serviceImpact?.estimatedEligiblePatientPopulation)}
        />
      </SummaryList>

      <SummaryList title="Pricing and budget impact">
        <SummaryListRow
          label="Estimated uptake based on expected adoption patterns"
          value={formatText(budgetImpact?.estimatedUptake)}
        />
        <SummaryListRow
          label="Will UK patients be able to access this product on compassionate or early access basis, outside of clinical trials?"
          value={formatEnum(budgetImpact?.compassionateAccessAvailable, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Compassionate or early access details"
          value={formatText(budgetImpact?.compassionateAccessDetails)}
        />
        <SummaryListRow
          label="Is a Patient Access Scheme or alternative discount arrangement planned for this indication? "
          value={formatEnum(budgetImpact?.patientAccessSchemePlanned, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Patient Access Scheme regions"
          value={formatEnumList(
            budgetImpact?.patientAccessSchemeRegions,
            patientAccessSchemeRegionLabels,
          )}
        />
        <SummaryListRow
          label="Are there plans for indication specific pricing?"
          value={formatEnum(budgetImpact?.indicationSpecificPricingPlanned, yesNoUnknownLabels)}
        />
        <SummaryListRow
          label="Indication-specific pricing details"
          value={formatText(budgetImpact?.indicationSpecificPricingDetails)}
        />
        <SummaryListRow
          label="What is the estimated net budget impact for the UK over the first 3 years of NHS use?"
          value={formatEnum(budgetImpact?.netUkBudgetImpactBand, netUkBudgetImpactBandLabels)}
        />
      </SummaryList>
    </>
  )
}
