"""Reference data, therapeutic categories and organisations for the synthetic medicine records."""

import re

# --- Reference data ---------------------------------------------------------------------------

BNF_CHAPTERS = [
    ("1", "Gastro-Intestinal System"),
    ("1.3", "Antisecretory drugs and mucosal protectants"),
    ("1.4", "Acute diarrhoea"),
    ("1.5", "Chronic bowel disorders"),
    ("1.6", "Laxatives"),
    ("1.9", "Drugs affecting intestinal secretions"),
    ("1.10", "Drugs for hepatic disorders"),
    ("2", "Cardiovascular System"),
    ("2.1", "Positive inotropic drugs"),
    ("2.2", "Diuretics"),
    ("2.3", "Anti-arrhythmic drugs"),
    ("2.4", "Beta-adrenoceptor blocking drugs"),
    ("2.5", "Hypertension and heart failure"),
    ("2.6", "Nitrates, calcium-channel blockers and other antianginal drugs"),
    ("2.8", "Anticoagulants and protamine"),
    ("2.9", "Antiplatelet drugs"),
    ("2.10", "Stable angina, acute coronary syndromes and fibrinolysis"),
    ("2.11", "Antifibrinolytic drugs and haemostatics"),
    ("2.12", "Lipid-regulating drugs"),
    ("2.13", "Local sclerosants"),
    ("3", "Respiratory System"),
    ("3.1", "Bronchodilators"),
    ("3.2", "Corticosteroids"),
    ("3.3", "Cromoglicate and related therapy, leukotriene receptor antagonists"),
    ("3.4", "Antihistamines, hyposensitisation and allergic emergencies"),
    ("3.5", "Respiratory stimulants and pulmonary surfactants"),
    ("3.7", "Mucolytics"),
    ("3.11", "Antifibrotics"),
    ("4", "Central Nervous System"),
    ("4.1", "Hypnotics and anxiolytics"),
    ("4.2", "Drugs used in psychoses and related disorders"),
    ("4.3", "Antidepressant drugs"),
    ("4.4", "CNS stimulants and drugs used for ADHD"),
    ("4.6", "Drugs used in nausea and vertigo"),
    ("4.7", "Analgesics"),
    ("4.8", "Antiepileptic drugs"),
    ("4.9", "Drugs used in parkinsonism and related disorders"),
    ("4.10", "Drugs used in substance dependence"),
    ("4.11", "Drugs for dementia"),
    ("5", "Infections"),
    ("5.1", "Antibacterial drugs"),
    ("5.2", "Antifungal drugs"),
    ("5.3", "Antiviral drugs"),
    ("5.4", "Antiprotozoal drugs"),
    ("5.5", "Anthelmintics"),
    ("6", "Endocrine System"),
    ("6.1", "Drugs used in diabetes"),
    ("6.2", "Thyroid and antithyroid drugs"),
    ("6.3", "Corticosteroids (endocrine)"),
    ("6.4", "Sex hormones"),
    ("6.5", "Hypothalamic and pituitary hormones and anti-oestrogens"),
    ("6.6", "Drugs affecting bone metabolism"),
    ("6.7", "Other endocrine drugs"),
    ("7", "Obstetrics, Gynaecology and Urinary-Tract Disorders"),
    ("7.1", "Drugs used in obstetrics"),
    ("7.2", "Treatment of vaginal and vulval conditions"),
    ("7.4", "Drugs for genito-urinary disorders"),
    ("8", "Malignant Disease and Immunosuppression"),
    ("8.1", "Cytotoxic drugs"),
    ("8.2", "Drugs affecting the immune response"),
    ("8.3", "Sex hormones and hormone antagonists in malignant disease"),
    ("9", "Nutrition and Blood"),
    ("9.1", "Anaemias and some other blood disorders"),
    ("9.5", "Minerals"),
    ("9.8", "Metabolic disorders"),
    ("9.13", "Oral nutrition"),
    ("10", "Musculoskeletal and Joint Diseases"),
    ("10.1", "Drugs used in rheumatic diseases and gout"),
    ("10.2", "Drugs used in neuromuscular disorders"),
    ("10.3", "Drugs for soft-tissue disorders and topical pain relief"),
    ("11", "Eye"),
    ("11.3", "Anti-infective eye preparations"),
    ("11.4", "Corticosteroids and other anti-inflammatory preparations"),
    ("11.5", "Mydriatics and cycloplegics"),
    ("11.6", "Treatment of glaucoma"),
    ("11.8", "Miscellaneous ophthalmic preparations"),
    ("12", "Ear, Nose and Oropharynx"),
    ("12.1", "Drugs acting on the ear"),
    ("12.2", "Drugs acting on the nose"),
    ("12.3", "Drugs acting on the oropharynx"),
    ("13", "Skin"),
    ("13.2", "Emollient and barrier preparations"),
    ("13.3", "Topical local anaesthetics and antipruritics"),
    ("13.4", "Topical corticosteroids"),
    ("13.5", "Preparations for eczema and psoriasis"),
    ("13.6", "Acne and rosacea"),
    ("13.10", "Anti-infective skin preparations"),
    ("14", "Immunological Products and Vaccines"),
    ("14.5", "Immunoglobulins"),
    ("15", "Anaesthesia"),
    ("15.1", "General anaesthesia"),
    ("15.2", "Local anaesthesia"),
]

THERAPEUTIC_AREAS = [
    ("1", "Addiction"),
    ("1.1", "Opioid use disorder"),
    ("1.2", "Neonatal abstinence syndrome (NAS)"),
    ("1.3", "Alcohol and stimulant use disorders"),
    ("2", "Allergy"),
    ("2.1", "Food allergy"),
    ("2.2", "Allergic and hypersensitivity reactions"),
    ("3", "Analgesia/Anesthesiology/Anti-inflammatory"),
    ("3.3", "Fever reduction"),
    ("3.4", "Pain"),
    ("3.5", "Neuropathic pain"),
    ("4", "Cardiovascular Disease"),
    ("4.1", "Acute coronary disease"),
    ("4.2", "Congenital heart disease"),
    ("4.4", "Dilated Cardiomyopathy (DCM) and symptomatic chronic heart failure"),
    ("4.5", "Hypertension"),
    ("4.6", "Structural heart and valve disease"),
    ("4.8", "Tachyarrhythmia"),
    ("5", "Dermatology"),
    ("5.4", "Dermatosis (steroid-responsive dermatosis)"),
    ("5.6", "Ichthyosis vulgaris"),
    ("5.9", "Pigmentation disorders"),
    ("5.12", "Hidradenitis suppurativa and inflammatory skin disease"),
    ("5.15", "Tinea"),
    ("6", "Endocrinology/Metabolism/Bone"),
    ("6.2", "Bone mineral density (BMD)"),
    ("6.5", "Diabetes Mellitus"),
    ("6.7", "Growth-hormone-secreting lesions of the pituitary gland"),
    ("6.8", "Gynecomastia"),
    ("6.10", "Inherited metabolic disorders"),
    ("6.11", "Hyperparathyroidism"),
    ("7", "Gastroenterology"),
    ("7.1", "Antiemesis"),
    ("7.4", "Crohn's Disease"),
    ("7.6", "Erosive Esophagitis"),
    ("7.8", "Liver and biliary disease"),
    ("7.11", "Ulcerative Colitis (UC)"),
    ("7.12", "Oral and dental disorders"),
    ("8", "Hematology/Coagulation"),
    ("8.1", "Anaemia"),
    ("8.3", "Iron overload due to blood transfusions-dependent anemia (chronic)"),
    ("8.4", "Sickle cell disease"),
    ("8.6", "Thrombocytopenia"),
    ("8.7", "Thromboembolism"),
    ("9", "Immunomodulators"),
    ("9.1", "Immune suppression"),
    ("9.2", "Prevention of organ rejection following renal transplantation"),
    ("9.3", "Primary immunodeficiency"),
    ("10", "Infectious Disease (viral)"),
    ("10.2", "Viral encephalitis and arboviral infections"),
    ("10.3", "Hepatitis C virus (HCV)"),
    ("10.5", "Human Immunodeficiency Virus (HIV) infection"),
    ("10.7", "Respiratory and enteric viral infections"),
    ("11", "Infectious Disease (non viral)"),
    ("11.3", "Candidiasis"),
    ("11.6", "Complicated intra-abdominal infections (cIAI)"),
    ("11.8", "Fungal infection (invasive)"),
    ("11.11", "Meningitis (bacterial)"),
    ("11.13", "Parasitic infections"),
    ("11.15", "Resistant infection or infections unresponsive to first choice antibiotic"),
    ("11.17", "Tuberculosis"),
    ("12", "Musculoskeletal"),
    ("12.1", "Osteoarthritis"),
    ("12.2", "Fracture healing and bone repair"),
    ("12.3", "Rare bone disorders"),
    ("13", "Neurology"),
    ("13.1", "Alzheimer's disease and dementia"),
    ("13.3", "Motor neurone disease"),
    ("13.5", "Multiple sclerosis (MS)"),
    ("13.6", "Neuromuscular disorders"),
    ("13.8", "Seizures"),
    ("13.9", "Stroke and cerebrovascular disease"),
    ("13.10", "Hearing and vestibular disorders"),
    ("14", "Oncology"),
    ("14.2", "Breast cancer"),
    ("14.3", "Gastrointestinal cancers"),
    ("14.4", "CNS malignancies and solid tumors"),
    ("14.5", "Genitourinary cancers"),
    ("14.6", "Hematologic tumors"),
    ("14.7", "Lung cancer"),
    ("14.8", "Melanoma"),
    ("15", "Ophthalmology"),
    ("15.1", "Conjunctivitis"),
    ("15.2", "Intraocular pressure"),
    ("15.3", "Retinal disease"),
    ("15.4", "Ocular inflammation"),
    ("16", "Psychiatry"),
    ("16.2", "Anxiety and trauma-related disorders"),
    ("16.4", "Bipolar disorder"),
    ("16.5", "Depression/Major Depressive Disorder (MDD)"),
    ("16.6", "Eating disorders"),
    ("17", "Pulmonary"),
    ("17.1", "Allergic Rhinitis"),
    ("17.2", "Asthma"),
    ("17.3", "Chronic obstructive pulmonary disease (COPD)"),
    ("17.4", "Respiratory failure and ventilation disorders"),
    ("18", "Renal Disease"),
    ("18.1", "Glomerular and inherited kidney disease"),
    ("18.2", "End stage renal disease"),
    ("19", "Rheumatology"),
    ("19.1", "Rheumatoid arthritis"),
    ("19.2", "Juvenile Idiopathic Arthritis (JIA)/Juvenile Rheumatoid Arthritis (JRA)"),
    ("19.3", "Axial spondyloarthritis"),
    ("19.4", "Vasculitis and connective tissue disease"),
    ("20", "Urologic"),
    ("20.1", "Male reproductive and urological disorders"),
    ("20.2", "Detrusor overactivity in neurological condition"),
    ("20.5", "Obstetric and gynaecological conditions"),
]

FORMULATION_TYPES = [
    "Capsule",
    "Cream",
    "Cutaneous solution",
    "Dispersible tablet",
    "Dry powder inhaler",
    "Enteric coated tablet",
    "Eye drops",
    "Gel",
    "Granules",
    "Implant",
    "Intramuscular injection",
    "Intrathecal injection",
    "Intravenous infusion",
    "Intravenous injection",
    "Intravitreal injection",
    "Metered dose inhaler",
    "Nasal spray",
    "Oral solution",
    "Oral suspension",
    "Powder for oral solution",
    "Powder for solution for infusion",
    "Powder for solution for injection",
    "Solution for infusion",
    "Solution for nebuliser",
    "Subcutaneous infusion",
    "Subcutaneous injection",
    "Subretinal injection",
    "Tablet",
    "Other",
    "Unknown at present",
]

MHRA_PROCEDURE_TYPES = [
    "MHRA national assessment procedure - standard",
    "MHRA national assessment procedure - accelerated",
    "International Recognition Procedure",
    "EC decision reliance procedure",
    "EU mutual recognition reliance procedure",
    "Access consortium",
    "Project Orbis",
    "Rolling review procedure",
    "Unknown / Other – please provide details",
]

IRP_REFERENCE_REGULATORS = [
    "Australia",
    "Canada",
    "European Union",
    "Japan",
    "Singapore",
    "Switzerland",
    "United States",
]

IRP_ROUTES = ["Recognition A", "Recognition B"]

ATMP_CLASSIFICATIONS = [
    "Gene therapy medicinal product",
    "Somatic cell therapy medicinal product",
    "Tissue engineered product",
    "Combined ATMP",
]

PATIENT_PATHWAY_POINTS = [
    "At diagnosis",
    "Before first-line treatment",
    "At disease progression or relapse",
    "Before second or subsequent line of treatment",
    "During treatment (response monitoring)",
    "Other",
]

UK_PATIENT_POPULATION_RANGES = [
    "Less than 1 per 50,000",
    "Between 1 per 50,000 and 25 per 100,000",
    "Between 25 and 50 per 100,000",
    "Between 50 and 150 per 100,000",
    "Between 150 and 250 per 100,000",
    "Between 250 and 500 per 100,000",
    "Between 500 and 750 per 100,000",
    "Between 750 and 1,000 per 100,000",
    "Between 1,000 and 1,500 per 100,000",
    "Between 1,500 and 2,000 per 100,000",
    "Between 2,000 and 3,000 per 100,000",
    "Over 3,000 per 100,000",
    "Unknown",
]


def _hierarchy(entries):
    rows = []
    for order, (code, label) in enumerate(entries, start=1):
        parent = code.rsplit(".", 1)[0] if "." in code else None
        rows.append({"code": code, "label": label, "parentCode": parent, "displayOrder": order})
    return rows


def _flat(labels):
    return [{"label": label, "displayOrder": order} for order, label in enumerate(labels, start=1)]


REFERENCE_DATA = {
    "bnfChapters": _hierarchy(BNF_CHAPTERS),
    "therapeuticAreas": _hierarchy(THERAPEUTIC_AREAS),
    "formulationTypes": _flat(FORMULATION_TYPES),
    "mhraProcedureTypes": _flat(MHRA_PROCEDURE_TYPES),
    "irpReferenceRegulators": _flat(IRP_REFERENCE_REGULATORS),
    "irpRoutes": _flat(IRP_ROUTES),
    "atmpClassifications": _flat(ATMP_CLASSIFICATIONS),
    "patientPathwayPoints": _flat(PATIENT_PATHWAY_POINTS),
    "ukPatientPopulationRanges": _flat(UK_PATIENT_POPULATION_RANGES),
}

# --- Therapeutic categories -------------------------------------------------------------------
# Each indication's disease is matched against these in order. A category determines the
# therapeutic area, BNF chapter, modes of action, comparators and typical biomarkers, so that
# clinically linked fields agree with each other.
#
# Modes of action are tagged with the kind of product they describe, which also sets the route
# of administration: "ab" (antibody), "bio" (other injectable biologic or oligonucleotide),
# "sm" (small molecule), "any" (either a small molecule or a biologic) or "atmp".


class Category:
    def __init__(
        self,
        key,
        pattern,
        areas,
        bnf,
        modes,
        comparators,
        genomic_markers=(),
        non_genomic_markers=(),
        is_cancer=False,
        population_bias="common",
    ):
        self.key = key
        self.pattern = re.compile(pattern, re.IGNORECASE) if pattern else None
        self.areas = areas
        self.bnf = bnf
        self.modes = modes
        self.comparators = comparators
        self.genomic_markers = genomic_markers
        self.non_genomic_markers = non_genomic_markers
        self.is_cancer = is_cancer
        self.population_bias = population_bias


# (area code, optional disease pattern that selects it)
CATEGORIES = [
    Category(
        "oncology",
        r"cancer|carcinoma|sarcoma|lymphoma|leuka?emia|myeloma|melanoma|tumou?r|neoplas|blastoma|"
        r"adenoma|gastrinoma|glucagonoma|chordoma|choristoma|adamantinoma|"
        r"mucoepidermoid|cell transformation|local cancer|paraneoplastic|thymoma|melanotic|pancoast",
        [
            ("14.2", r"breast"),
            ("14.3", r"intestin|duoden|cecal|anus|rectal|gastr|pancrea|glucagon|liver"),
            ("14.5", r"prostat|renal|bladder|pelvic|testi"),
            ("14.6", r"leuk|lymph|myeloma|hairy cell|dendritic"),
            ("14.7", r"lung|bronch|pleural|pancoast"),
            ("14.8", r"melan|freckle"),
            ("14.4", None),
        ],
        ["8.1", "8.2", "8.3"],
        [
            ("PD-1 inhibitor", "ab"),
            ("PD-L1 inhibitor", "ab"),
            ("HER2-directed antibody-drug conjugate", "ab"),
            ("TROP2-directed antibody-drug conjugate", "ab"),
            ("CD3 x CD20 bispecific T-cell engager", "ab"),
            ("PARP inhibitor", "sm"),
            ("CDK4/6 inhibitor", "sm"),
            ("EGFR tyrosine kinase inhibitor", "sm"),
            ("KRAS G12C inhibitor", "sm"),
            ("BTK inhibitor", "sm"),
            ("BCL-2 inhibitor", "sm"),
            ("VEGF receptor tyrosine kinase inhibitor", "sm"),
            ("Androgen receptor inhibitor", "sm"),
        ],
        [
            "pembrolizumab",
            "nivolumab",
            "atezolizumab",
            "durvalumab",
            "carboplatin",
            "pemetrexed",
            "FOLFOX",
            "capecitabine",
            "bevacizumab",
            "trastuzumab",
            "rituximab",
            "cetuximab",
            "docetaxel",
        ],
        genomic_markers=(
            ("KRAS G12C", "KRAS G12C mutation"),
            ("BRAF V600E", "BRAF V600E mutation"),
            ("EGFR", "EGFR exon 19 deletion or exon 21 L858R mutation"),
            ("ALK", "ALK gene fusion"),
            ("ROS1", "ROS1 gene fusion"),
            ("BRCA1/2", "Germline or somatic BRCA1/2 pathogenic variant"),
            ("FLT3", "FLT3-ITD or FLT3-TKD mutation"),
            ("IDH1", "IDH1 R132 mutation"),
            ("PIK3CA", "PIK3CA activating mutation"),
            ("NTRK", "NTRK1/2/3 gene fusion"),
            ("MET", "MET exon 14 skipping alteration"),
            ("RET", "RET gene fusion"),
        ),
        non_genomic_markers=(
            "PD-L1 expression (tumour proportion score of 50% or more) by immunohistochemistry",
            "HER2 protein overexpression (IHC 3+) by immunohistochemistry",
            "Claudin 18.2 expression by immunohistochemistry",
            "Oestrogen receptor-positive status by immunohistochemistry",
        ),
        is_cancer=True,
    ),
    Category(
        "ophthalmology",
        r"ocul|eye|retin|choroid|scler(al|itis)|iritis|corne|conjunctiv|blephar|nystagmus|"
        r"mydriasis|scotoma|night blindness|exophthalmos|orbital|keratitis",
        [
            ("15.1", r"conjunctiv"),
            ("15.2", r"pressure|glaucoma"),
            ("15.3", r"retin|night blindness|scotoma|choroid"),
            ("15.4", r"iritis|scler|keratitis|infection"),
            ("15.4", None),
        ],
        ["11.4", "11.6", "11.8", "11.3"],
        [
            ("VEGF inhibitor", "ab"),
            ("Angiopoietin-2/VEGF-A bispecific antibody", "ab"),
            ("Complement C3 inhibitor", "bio"),
            ("Rho kinase inhibitor", "sm"),
            ("AAV-based gene therapy", "atmp"),
        ],
        ["aflibercept", "ranibizumab", "faricimab", "latanoprost", "dexamethasone implant"],
        genomic_markers=(("RPE65", "Biallelic RPE65 pathogenic variants"),),
        population_bias="rare",
    ),
    Category(
        "haematology",
        r"an(a)?emia|platelet|thrombo|coagul|purpura|afibrino|paraprotein|sickle|globulin|"
        r"pelger|rh isoimmun|hemarthrosis|lymphocytopenia|hemorrhagic shock|hyperglobulin|"
        r"snake bite",
        [
            ("8.1", r"an(a)?emia"),
            ("8.4", r"sickle"),
            ("8.6", r"platelet|thrombocyt|purpura"),
            ("8.7", r"thrombo|coagul|afibrino|hemarthrosis"),
            ("8.3", None),
        ],
        ["9.1", "2.8", "2.11"],
        [
            ("Complement C5 inhibitor", "ab"),
            ("Factor XIa inhibitor", "any"),
            ("Thrombopoietin receptor agonist", "sm"),
            ("HIF prolyl hydroxylase inhibitor", "sm"),
            ("FcRn antagonist", "ab"),
            ("AAV-based gene therapy", "atmp"),
        ],
        ["eculizumab", "eltrombopag", "hydroxycarbamide", "apixaban", "best supportive care"],
        genomic_markers=(("HBB", "HBB pathogenic variants"),),
        population_bias="rare",
    ),
    Category(
        "viral",
        r"virus|viral|viridae|dengue|japanese encephalitis|equine encephalomyelitis|"
        r"hepatitis|hiv|aids|herpe|croup",
        [
            ("10.3", r"hepatitis"),
            ("10.5", r"hiv|aids|retrovir"),
            ("10.2", r"encephal|dengue|alphavirus"),
            ("10.7", None),
        ],
        ["5.3"],
        [
            ("Viral polymerase inhibitor", "sm"),
            ("HIV capsid inhibitor", "sm"),
            ("Neutralising monoclonal antibody", "ab"),
            ("Viral entry inhibitor", "any"),
        ],
        ["sofosbuvir-velpatasvir", "bictegravir-based regimen", "standard antiviral therapy"],
    ),
    Category(
        "infection",
        r"infect|bacteri|fung|candid|mycos|tubercul|syphilis|chancre|brucell|amebiasis|"
        r"helminth|onchocerc|toxoplasm|microsporid|actinomycosis|pinta|osteomyelitis|abscess|"
        r"peritonitis|septic|lymphogranuloma|ecthyma|lupus vulgaris|erythema induratum|"
        r"ectoparasit|infestation|ludwig|mastoiditis|tonsillitis|tracheitis",
        [
            ("11.3", r"candid"),
            ("11.8", r"fung|mycos"),
            ("11.11", r"nervous system bacterial|mening"),
            ("11.13", r"helminth|onchocerc|toxoplasm|amebiasis|microsporid|parasit|enoplida"),
            ("11.17", r"tubercul|lupus vulgaris|erythema induratum|mycobact"),
            ("11.6", r"peritonitis|abscess|intra-abdominal"),
            ("11.15", None),
        ],
        ["5.1", "5.2", "5.4", "5.5"],
        [
            ("Beta-lactam/beta-lactamase inhibitor combination", "sm"),
            ("Macrocyclic peptide antibacterial", "bio"),
            ("Monoclonal antibody against bacterial toxin", "ab"),
            ("Bacteriophage therapy", "bio"),
        ],
        ["meropenem", "piperacillin-tazobactam", "voriconazole", "standard antibiotic therapy"],
    ),
    Category(
        "neurology",
        r"brain|cerebr|encephal|neuro|neurit|(?<!cardio)myopath|muscular dystroph|myotonia|"
        r"myasthenia|"
        r"multiple sclerosis|lateral sclerosis|epilep|seizure|chorea|nerve|paralysis|dyssynergia|"
        r"lissenceph|alzheimer|"
        r"agnosia|dizziness|intracranial|cranial|spinal|axonal|moyamoya|mening|merrf|"
        r"mitochondrial|niemann|\bmuscular|hearing|cochlear|meniere|ageusia|voice|tarlov|"
        r"arachnoid|shaken baby",
        [
            ("13.1", r"alzheimer|dementia|agnosia|amyloid"),
            ("13.3", r"amyotrophic"),
            ("13.5", r"multiple sclerosis|encephalomyelitis|leukoencephal"),
            ("13.6", r"myopath|dystroph|myotonia|myasthenia|muscular|paralysis"),
            ("13.8", r"epilep|seizure|myoclonic"),
            ("13.9", r"infarction|hemorrhage|hematoma|moyamoya|angiopathy|isch"),
            ("13.10", r"hearing|cochlear|meniere|dizziness"),
            ("13.6", None),
        ],
        ["4.8", "4.9", "4.11", "10.2"],
        [
            ("Anti-amyloid monoclonal antibody", "ab"),
            ("Antisense oligonucleotide", "bio"),
            ("FcRn antagonist", "ab"),
            ("Sodium channel blocker", "sm"),
            ("BTK inhibitor (CNS-penetrant)", "sm"),
            ("AAV-based gene therapy", "atmp"),
        ],
        ["ocrelizumab", "levetiracetam", "riluzole", "efgartigimod", "best supportive care"],
        genomic_markers=(("SMN1", "SMN1 homozygous deletion"), ("SOD1", "SOD1 pathogenic variant")),
        population_bias="rare",
    ),
    Category(
        "psychiatry",
        r"bipolar|depress|schizo|suicide|eating disorder|stress disorder|neuroses|anxiety|mental health",
        [
            ("16.4", r"bipolar"),
            ("16.5", r"depress|suicide"),
            ("16.6", r"eating"),
            ("16.2", None),
        ],
        ["4.1", "4.2", "4.3"],
        [
            ("Orexin receptor antagonist", "sm"),
            ("NMDA receptor antagonist", "sm"),
            ("Muscarinic M1/M4 receptor agonist", "sm"),
            ("5-HT2A receptor agonist (psychedelic)", "sm"),
        ],
        ["sertraline", "quetiapine", "lithium", "lurasidone", "cognitive behavioural therapy"],
    ),
    Category(
        "addiction",
        r"abuse|cocaine|alcohol-related|marijuana|abstinence|substance",
        [("1.2", r"neonatal"), ("1.3", r"cocaine|marijuana|alcohol"), ("1.1", None)],
        ["4.10"],
        [("Opioid receptor modulator", "sm"), ("Long-acting opioid partial agonist", "bio")],
        ["buprenorphine", "methadone", "naltrexone", "psychosocial support"],
    ),
    Category(
        "pain",
        r"pain|neuralgia|fever|algesia|postoperative|fatigue|burning mouth|hiccup",
        [("3.3", r"fever"), ("3.5", r"neuralgia|burning|algesia"), ("3.4", None)],
        ["4.7", "4.6", "15.2"],
        [
            ("NaV1.8 sodium channel inhibitor", "sm"),
            ("CGRP receptor antagonist", "sm"),
            ("Nerve growth factor inhibitor", "ab"),
        ],
        ["paracetamol", "ibuprofen", "pregabalin", "morphine", "ondansetron"],
    ),
    Category(
        "dermatology",
        r"derm|skin|hair|pigment|kerato|hidradenitis|prurigo|pityriasis|lichen|pemphigoid|"
        r"intertrigo|chilblains|rhinophyma|hyperhidrosis|hirsutism|chloracne|scleromyxedema|"
        r"ichthyos|tinea|acne|radiodermatitis",
        [
            ("5.9", r"pigment|vitiligo"),
            ("5.12", r"hidradenitis|prurigo|lichen|pemphigoid"),
            ("5.6", r"ichthyos|kerato"),
            ("5.15", r"tinea|candid"),
            ("5.4", None),
        ],
        ["13.4", "13.5", "13.6", "13.2"],
        [
            ("IL-17A/F inhibitor", "ab"),
            ("IL-23 inhibitor", "ab"),
            ("IL-4/IL-13 inhibitor", "ab"),
            ("JAK1 inhibitor", "sm"),
            ("TYK2 inhibitor", "sm"),
        ],
        ["adalimumab", "secukinumab", "dupilumab", "methotrexate", "topical corticosteroids"],
    ),
    Category(
        "endocrine",
        r"diabet|thyroid|goiter|parathyroid|pituitary|adrenal|adrenogenital|mineralocorticoid|"
        r"gynecomastia|eunuchism|hypophosphat|hypercalc|ketosis|alkalosis|metabol|lipoprotein|"
        r"lipid|nelson|lactose|hyperglycinemia|crigler|sulfatase|nutrition|refsum|"
        r"sulfatase|nesidioblastosis|calciphylaxis|xanthomatosis|sulfatidosis",
        [
            ("6.5", r"diabet|ketosis|nesidioblastosis"),
            ("6.7", r"pituitary|nelson"),
            ("6.8", r"gynecomastia"),
            ("6.11", r"parathyroid|hypercalc"),
            ("6.2", r"hypophosphat|calciphylaxis"),
            ("6.10", None),
        ],
        ["6.1", "6.5", "6.7", "9.8"],
        [
            ("GLP-1 receptor agonist", "any"),
            ("GIP/GLP-1 receptor co-agonist", "bio"),
            ("Enzyme replacement therapy", "bio"),
            ("Somatostatin receptor ligand", "bio"),
            ("Calcium-sensing receptor modulator", "sm"),
            ("AAV-based gene therapy", "atmp"),
        ],
        ["semaglutide", "metformin", "insulin glargine", "agalsidase beta", "best supportive care"],
        genomic_markers=(("UGT1A1", "Biallelic UGT1A1 pathogenic variants"),),
        population_bias="rare",
    ),
    Category(
        "gastroenterology",
        r"bowel|intestin|colitis|crohn|gastr|esophag|duoden|rectal|proct|liver|hepat|cholest|"
        r"jaundice|biliary|gallbladder|bilirubin|enterocolitis|nausea|vomiting|portal|"
        r"pneumoperitoneum|plummer|tooth|teeth|dental|gingiv|jaw|tongue|stomatitis|"
        r"malocclusion|cementosis|odontogenic|cleft|mandibulofacial|mikulicz|periapical|"
        r"sialorrhea|stomatognathic|diaphragmatic hernia",
        [
            ("7.1", r"nausea|vomiting"),
            ("7.4", r"crohn|inflammatory bowel"),
            ("7.6", r"esophag"),
            ("7.11", r"colitis|proct|rectal"),
            ("7.8", r"liver|hepat|cholest|jaundice|biliary|gallbladder|bilirubin|portal"),
            ("7.12", r"tooth|teeth|dental|gingiv|jaw|tongue|stomatitis|malocclusion|"
                     r"cementosis|odontogenic|cleft|mandibulofacial|mikulicz|periapical|sialorrhea|"
                     r"stomatognathic"),
            ("7.4", None),
        ],
        ["1.3", "1.5", "1.10", "12.3"],
        [
            ("Alpha4beta7 integrin inhibitor", "ab"),
            ("IL-23 inhibitor", "ab"),
            ("S1P receptor modulator", "sm"),
            ("FXR agonist", "sm"),
            ("NK1 receptor antagonist", "sm"),
        ],
        ["vedolizumab", "ustekinumab", "infliximab", "ursodeoxycholic acid", "ondansetron"],
    ),
    Category(
        "respiratory",
        r"pulmon|lung|asthma|emphysema|bronch|respirat|dyspnea|hypercapnia|hypoventilation|"
        r"pneumothorax|rhinitis|nasal|cheyne|middle lobe",
        [
            ("17.1", r"rhinitis|nasal"),
            ("17.2", r"asthma"),
            ("17.3", r"emphysema|bronch|middle lobe"),
            ("17.4", None),
        ],
        ["3.1", "3.2", "3.3", "3.11"],
        [
            ("TSLP inhibitor", "ab"),
            ("IL-5 receptor antagonist", "ab"),
            ("PDE4 inhibitor", "sm"),
            ("CFTR modulator", "sm"),
        ],
        ["tezepelumab", "benralizumab", "inhaled corticosteroid/LABA", "roflumilast"],
        non_genomic_markers=("Blood eosinophil count of 300 cells/microlitre or more",),
    ),
    Category(
        "allergy",
        r"allergic|hypersensitivity|milk|acute-phase|immune reconstitution",
        [("2.1", r"milk|food"), ("2.2", None)],
        ["3.4"],
        [("Anti-IgE monoclonal antibody", "ab"), ("Oral immunotherapy", "sm")],
        ["omalizumab", "adrenaline auto-injector", "antihistamines"],
    ),
    Category(
        "immunology",
        r"transplant|lymph node|dysgammaglobulin|immunodeficien|lymphangiectasis|"
        r"lymphatic|cryoglobulin|sjogren|wiskott|shwartzman",
        [("9.2", r"transplant"), ("9.3", r"globulin|immunodeficien"), ("9.1", None)],
        ["8.2", "14.5"],
        [("Anti-CD40L monoclonal antibody", "ab"), ("Calcineurin inhibitor", "sm")],
        ["tacrolimus", "mycophenolate mofetil", "immunoglobulin replacement therapy"],
    ),
    Category(
        "renal",
        r"renal|kidney|nephr|bartter|hypokalemic",
        [("18.1", r"nephritis|nephrosis|bartter|hereditary"), ("18.2", None)],
        ["2.2", "9.5"],
        [
            ("SGLT2 inhibitor", "sm"),
            ("Complement factor B inhibitor", "sm"),
            ("Endothelin receptor antagonist", "sm"),
        ],
        ["dapagliflozin", "ramipril", "haemodialysis", "best supportive care"],
    ),
    Category(
        "urology",
        r"bladder|urethr|urinary|aspermia|sertoli|spermat|lithiasis|gynatresia|hydrocolpos|"
        r"hematometra|amenorrhea|placental|hellp|fetal|pregnancy|breast|eunuch|uter|vagin|"
        r"abortion",
        [
            ("20.2", r"bladder|urinary"),
            ("20.5", r"gynatresia|hydrocolpos|hematometra|amenorrhea|placental|hellp|fetal|"
                     r"pregnancy|breast|uter|vagin|abortion"),
            ("20.1", None),
        ],
        ["7.4", "7.1", "7.2"],
        [
            ("Beta-3 adrenoceptor agonist", "sm"),
            ("Oxytocin receptor antagonist", "sm"),
            ("Botulinum toxin type A", "bio"),
        ],
        ["mirabegron", "solifenacin", "atosiban", "standard obstetric care"],
    ),
    Category(
        "rheumatology",
        r"(?<!osteo)arthritis|spondylitis|ankylosis|rheum|polymyalgia|polychondritis|cartilage|"
        r"discitis|synovial|tietze",
        [
            ("19.1", r"rheumatoid|arthritis"),
            ("19.3", r"spondylitis|ankylosis"),
            ("19.4", r"polymyalgia|polychondritis|rheumatic"),
            ("19.4", None),
        ],
        ["10.1"],
        [
            ("JAK1/2 inhibitor", "sm"),
            ("IL-6 receptor antagonist", "ab"),
            ("TNF-alpha inhibitor", "ab"),
            ("IL-17A inhibitor", "ab"),
        ],
        ["adalimumab", "methotrexate", "tocilizumab", "baricitinib", "glucocorticoids"],
    ),
    Category(
        "cardiovascular",
        r"cardi|aort|arter|coronary|heart|hypertens|mitral|valve|valvular|ventric|\bisch|"
        r"aneurysm\b|"
        r"pericard|pre-excitation|jervell|septal|cor triatriatum|endocardial|fibromuscular|"
        r"wolff-parkinson|subclavian",
        [
            ("4.1", r"coronary|chest pain|isch"),
            ("4.2", r"congenital|septal|cor triatriatum|endocardial cushion|anomal"),
            ("4.4", r"cardiomyopathy|fibrosis|hypertrophy"),
            ("4.5", r"hypertens"),
            ("4.6", r"valve|mitral|aort|stenosis"),
            ("4.8", r"pre-excitation|jervell|arrhythm|wolff"),
            ("4.4", None),
        ],
        ["2.5", "2.3", "2.12", "2.8", "2.6"],
        [
            ("PCSK9 inhibitor", "ab"),
            ("SGLT2 inhibitor", "sm"),
            ("Cardiac myosin inhibitor", "sm"),
            ("Aldosterone synthase inhibitor", "sm"),
            ("Factor XIa inhibitor", "any"),
        ],
        ["sacubitril-valsartan", "bisoprolol", "atorvastatin", "apixaban", "ramipril"],
    ),
    Category(
        "musculoskeletal",
        r"osteo|bone|hallux|lordosis|fracture|injur|dislocation|carpal tunnel|contracture|"
        r"necrosis|osteolysis|atrophy|wound|hip|crush|disease progression",
        [
            ("12.1", r"osteoarthritis|osteophyte|hallux"),
            ("12.2", r"fracture|injur|wound|crush|necrosis|contracture|dislocation"),
            ("12.3", None),
        ],
        ["6.6", "10.3", "10.1"],
        [
            ("Anti-sclerostin monoclonal antibody", "ab"),
            ("Cathepsin K inhibitor", "sm"),
            ("Recombinant growth factor", "bio"),
        ],
        ["alendronic acid", "denosumab", "physiotherapy", "surgical management"],
        population_bias="rare",
    ),
]

# The BNF sections that fit each therapeutic area. Oncology is chosen separately.
AREA_BNF = {
    "1.1": ["4.10"], "1.2": ["4.10"], "1.3": ["4.10"],
    "2.1": ["3.4"], "2.2": ["3.4"],
    "3.3": ["4.7"], "3.4": ["4.7", "10.3"], "3.5": ["4.7", "4.8"],
    "4.1": ["2.10", "2.9"], "4.2": ["2.5"], "4.4": ["2.5", "2.1"], "4.5": ["2.5", "2.2"],
    "4.6": ["2.5"], "4.8": ["2.3"],
    "5.4": ["13.4"], "5.6": ["13.2"], "5.9": ["13.5"], "5.12": ["13.5"], "5.15": ["13.10"],
    "6.2": ["6.6"], "6.5": ["6.1"], "6.7": ["6.7", "6.5"], "6.8": ["6.4"], "6.10": ["9.8"],
    "6.11": ["6.7"],
    "7.1": ["4.6"], "7.4": ["1.5"], "7.6": ["1.3"], "7.8": ["1.10"], "7.11": ["1.5"],
    "7.12": ["12.3"],
    "8.1": ["9.1"], "8.3": ["9.1"], "8.4": ["9.1"], "8.6": ["9.1"], "8.7": ["2.8"],
    "9.1": ["8.2"], "9.2": ["8.2"], "9.3": ["14.5"],
    "10.2": ["5.3"], "10.3": ["5.3"], "10.5": ["5.3"], "10.7": ["5.3"],
    "11.3": ["5.2"], "11.6": ["5.1"], "11.8": ["5.2"], "11.11": ["5.1"], "11.13": ["5.4", "5.5"],
    "11.15": ["5.1"], "11.17": ["5.1"],
    "12.1": ["10.1", "10.3"], "12.2": ["6.6"], "12.3": ["6.6"],
    "13.1": ["4.11"], "13.3": ["4.9"], "13.5": ["8.2"], "13.6": ["10.2"], "13.8": ["4.8"],
    "13.9": ["2.9"], "13.10": ["12.1"],
    "15.1": ["11.3", "11.4"], "15.2": ["11.6"], "15.3": ["11.8"], "15.4": ["11.4"],
    "16.2": ["4.1", "4.3"], "16.4": ["4.2"], "16.5": ["4.3"], "16.6": ["4.3"],
    "17.1": ["12.2", "3.4"], "17.2": ["3.1", "3.2", "3.3"], "17.3": ["3.1", "3.2"],
    "17.4": ["3.5"],
    "18.1": ["2.5"], "18.2": ["9.5"],
    "19.1": ["10.1"], "19.2": ["10.1"], "19.3": ["10.1"], "19.4": ["10.1"],
    "20.1": ["7.4"], "20.2": ["7.4"], "20.5": ["7.1", "7.2"],
}

# Modes of action for therapeutic areas that need a narrower choice than their category.
AREA_MODES = {
    "11.3": [("Glucan synthase inhibitor (antifungal)", "sm"), ("Triazole antifungal", "sm")],
    "11.8": [("Glucan synthase inhibitor (antifungal)", "sm"), ("Triazole antifungal", "sm")],
    "11.13": [("Benzimidazole anthelmintic", "sm"), ("Nitroimidazole antiprotozoal", "sm")],
}

# Used when no category pattern matches the disease name.
FALLBACK_CATEGORY_KEYS = [
    "endocrine",
    "neurology",
    "musculoskeletal",
    "gastroenterology",
    "haematology",
    "dermatology",
]

# Order matters: earlier categories win when a disease name matches several patterns.
_MATCH_ORDER = [
    "oncology",
    "ophthalmology",
    "haematology",
    "viral",
    "infection",
    "psychiatry",
    "addiction",
    "pain",
    "neurology",
    "dermatology",
    "endocrine",
    "gastroenterology",
    "cardiovascular",
    "respiratory",
    "allergy",
    "immunology",
    "renal",
    "urology",
    "rheumatology",
    "musculoskeletal",
]
CATEGORIES.sort(key=lambda category: _MATCH_ORDER.index(category.key))

CATEGORIES_BY_KEY = {category.key: category for category in CATEGORIES}

RARE_DISEASE_PATTERN = re.compile(
    r"dystroph|congenital|hereditary|inborn|familial|sulfatase|myopath|atresia|"
    r"dysplasia|aneuploidy|chromosome|deletion|nondisjunction|niemann|refsum|merrf|"
    r"lissenceph|osteopoikilosis|proteinosis|myotonia|sulfatase|crigler",
    re.IGNORECASE,
)

# --- Organisations ----------------------------------------------------------------------------

HORIZON_SCANNING_ORGANISATIONS = [
    "Northern Horizon Scanning Research Centre",
    "Midlands Medicines Intelligence Unit",
    "London Health Technology Horizon Scanning",
    "South West Innovation Observatory",
    "East of England Medicines Intelligence Service",
    "Scottish Horizon Scanning Collaboration",
    "Welsh Medicines Horizon Scanning Network",
    "Northern Ireland Medicines Forecasting Unit",
    "National Specialised Commissioning Intelligence Team",
    "Academic Health Science Horizon Scanning Partnership",
]

STRATEGIC_ORGANISATIONS = [
    "National Medicines Commissioning Board",
    "UK Medicines Access Planning Group",
    "Regional Medicines Optimisation Committee",
    "Integrated Care Pharmacy Network",
]

INTERNAL_ORGANISATIONS = ["UKPS Quality Assurance"]

TOWNS = [
    ("Cambridge", "CB2"),
    ("Oxford", "OX4"),
    ("Manchester", "M15"),
    ("Leeds", "LS1"),
    ("Birmingham", "B3"),
    ("Edinburgh", "EH3"),
    ("Cardiff", "CF10"),
    ("Belfast", "BT1"),
    ("Slough", "SL1"),
    ("Reading", "RG1"),
    ("Stevenage", "SG1"),
    ("Macclesfield", "SK10"),
    ("London", "EC1A"),
    ("London", "W1T"),
    ("Uxbridge", "UB8"),
]

STREETS = [
    "Science Park",
    "Innovation Way",
    "Discovery Drive",
    "Riverside Business Park",
    "Station Road",
    "Kings Parade",
    "Granta Park",
    "Medicity Road",
    "Quayside",
    "Chancery Lane",
]
