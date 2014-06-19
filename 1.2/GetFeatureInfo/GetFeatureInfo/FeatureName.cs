using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetFeatureInfo
{
    class FeatureName
    {
        public string[] iupac = new string[24] {
            "M", "R", "W", "S", "Y", "K",
            "V", "H", "D", "B", "X", "N",
            "m", "r", "w", "s", "y", "k",
            "v", "h", "d", "b", "x", "n"
        };

        public string[] motifFeatureName = new string[323] { 
            "AAAA", "AAAT", "AAAG", "AAAC", "AATA", "AATT", "AATG", "AATC", "AAGA", "AAGT",
            "AAGG", "AAGC", "AACA", "AACT", "AACG", "AACC", "ATAA", "ATAT", "ATAG", "ATAC",
            "ATTA", "ATTT", "ATTG", "ATTC", "ATGA", "ATGT", "ATGG", "ATGC", "ATCA", "ATCT",
            "ATCG", "ATCC", "AGAA", "AGAT", "AGAG", "AGAC", "AGTA", "AGTT", "AGTG", "AGTC",
            "AGGA", "AGGT", "AGGG", "AGGC", "AGCA", "AGCT", "AGCG", "AGCC", "ACAA", "ACAT",
            "ACAG", "ACAC", "ACTA", "ACTT", "ACTG", "ACTC", "ACGA", "ACGT", "ACGG", "ACGC",
            "ACCA", "ACCT", "ACCG", "ACCC", "TAAA", "TAAT", "TAAG", "TAAC", "TATA", "TATT",
            "TATG", "TATC", "TAGA", "TAGT", "TAGG", "TAGC", "TACA", "TACT", "TACG", "TACC",
            "TTAA", "TTAT", "TTAG", "TTAC", "TTTA", "TTTT", "TTTG", "TTTC", "TTGA", "TTGT",
            "TTGG", "TTGC", "TTCA", "TTCT", "TTCG", "TTCC", "TGAA", "TGAT", "TGAG", "TGAC",
            "TGTA", "TGTT", "TGTG", "TGTC", "TGGA", "TGGT", "TGGG", "TGGC", "TGCA", "TGCT",
            "TGCG", "TGCC", "TCAA", "TCAT", "TCAG", "TCAC", "TCTA", "TCTT", "TCTG", "TCTC",
            "TCGA", "TCGT", "TCGG", "TCGC", "TCCA", "TCCT", "TCCG", "TCCC", "GAAA", "GAAT",
            "GAAG", "GAAC", "GATA", "GATT", "GATG", "GATC", "GAGA", "GAGT", "GAGG", "GAGC",
            "GACA", "GACT", "GACG", "GACC", "GTAA", "GTAT", "GTAG", "GTAC", "GTTA", "GTTT",
            "GTTG", "GTTC", "GTGA", "GTGT", "GTGG", "GTGC", "GTCA", "GTCT", "GTCG", "GTCC",
            "GGAA", "GGAT", "GGAG", "GGAC", "GGTA", "GGTT", "GGTG", "GGTC", "GGGA", "GGGT",
            "GGGG", "GGGC", "GGCA", "GGCT", "GGCG", "GGCC", "GCAA", "GCAT", "GCAG", "GCAC",
            "GCTA", "GCTT", "GCTG", "GCTC", "GCGA", "GCGT", "GCGG", "GCGC", "GCCA", "GCCT",
            "GCCG", "GCCC", "CAAA", "CAAT", "CAAG", "CAAC", "CATA", "CATT", "CATG", "CATC",
            "CAGA", "CAGT", "CAGG", "CAGC", "CACA", "CACT", "CACG", "CACC", "CTAA", "CTAT",
            "CTAG", "CTAC", "CTTA", "CTTT", "CTTG", "CTTC", "CTGA", "CTGT", "CTGG", "CTGC",
            "CTCA", "CTCT", "CTCG", "CTCC", "CGAA", "CGAT", "CGAG", "CGAC", "CGTA", "CGTT",
            "CGTG", "CGTC", "CGGA", "CGGT", "CGGG", "CGGC", "CGCA", "CGCT", "CGCG", "CGCC",
            "CCAA", "CCAT", "CCAG", "CCAC", "CCTA", "CCTT", "CCTG", "CCTC", "CCGA", "CCGT",
            "CCGG", "CCGC", "CCCA", "CCCT", "CCCG", "CCCC", "AAA", "AAT", "AAC", "AAG",
            "ATA", "ATT", "ATC", "ATG", "ACA", "ACT", "ACC", "ACG", "AGA", "AGT",
            "AGC", "AGG", "TAA", "TAT", "TAC", "TAG", "TTA", "TTT", "TTC", "TTG",
            "TCA", "TCT", "TCC", "TCG", "TGA", "TGT", "TGC", "TGG", "CAA", "CAT",
            "CAC", "CAG", "CTA", "CTT", "CTC", "CTG", "CCA", "CCT", "CCC", "CCG",
            "CGA", "CGT", "CGC", "CGG", "GAA", "GAT", "GAC", "GAG", "GTA", "GTT",
            "GTC", "GTG", "GCA", "GCT", "GCC", "GCG", "GGA", "GGT", "GGC", "GGG",
            "%G+C", "CpG ratio", "TG"
        };

        public string[] motifFeatureName6 = new string[30] {
            "AAWGGR", "CCDGGV", "CCCSGS", "AAATKT", "BCCCWG", "GSCCCS",
            "ATGVAA", "GGVCCH", "CCGSSC", "TGVAAA", "CCCWGH", "CGSCCS",
            "CWGAMA", "GGSCTB", "VGCGGG", "AATKAA", "CCTGMV", "GRGCSC",
            "AAATGV", "GMCCCN", "TCCSSG", "TGRAAT", "SCCWCR", "KCCSGC",
            "GVAAAT", "WGCCCH", "CTCCSS", "TRAATT", "CKGSCM", "SGMGCC"
        };

        public string[] motifFeatureName6Expanded = new string[133] {
            "AAAGGA", "AAAGGG", "AATGGA", "AATGGG", "CCAGGA", "CCAGGC", "CCAGGG", "CCGGGA", 
            "CCGGGC", "CCGGGG", "CCTGGA", "CCTGGC", "CCTGGG", "CCCCGC", "CCCCGG", "CCCGGC", 
            "CCCGGG", "AAATGT", "AAATTT", "CCCCAG", "CCCCTG", "GCCCAG", "GCCCTG", "TCCCAG", 
            "TCCCTG", "GCCCCC", "GCCCCG", "GGCCCC", "GGCCCG", "ATGAAA", "ATGCAA", "ATGGAA", 
            "GGACCA", "GGACCC", "GGACCT", "GGCCCA", "GGCCCT", "GGGCCA", "GGGCCC", "GGGCCT", 
            "CCGCCC", "CCGCGC", "CCGGCC", "TGAAAA", "TGCAAA", "TGGAAA", "CCCAGA", "CCCAGC", 
            "CCCAGT", "CCCTGA", "CCCTGC", "CCCTGT", "CGCCCC", "CGCCCG", "CGGCCC", "CGGCCG", 
            "CAGAAA", "CAGACA", "CTGAAA", "CTGACA", "GGCCTC", "GGCCTG", "GGCCTT", "GGGCTC", 
            "GGGCTG", "GGGCTT", "AGCGGG", "CGCGGG", "GGCGGG", "AATGAA", "AATTAA", "CCTGAA", 
            "CCTGAC", "CCTGAG", "CCTGCA", "CCTGCC", "CCTGCG", "GAGCCC", "GAGCGC", "GGGCGC", 
            "AAATGA", "AAATGC", "AAATGG", "GACCCG", "GACCCA", "GACCCT", "GACCCC", "GCCCCA", 
            "GCCCCT", "TCCCCG", "TCCCGG", "TCCGCG", "TCCGGG", "TGAAAT", "TGGAAT", "CCCACA", 
            "CCCACG", "CCCTCA", "CCCTCG", "GCCACA", "GCCACG", "GCCTCA", "GCCTCG", "GCCCGC", 
            "GCCGGC", "TCCCGC", "TCCGGC", "GAAAAT", "GCAAAT", "GGAAAT", "AGCCCA", "AGCCCC", 
            "AGCCCT", "TGCCCA", "TGCCCC", "TGCCCT", "CTCCCC", "CTCCCG", "CTCCGC", "CTCCGG", 
            "TAAATT", "TGAATT", "CGGCCA", "CGGGCA", "CGGGCC", "CTGCCA", "CTGCCC", "CTGGCA", 
            "CTGGCC", "CGAGCC", "CGCGCC", "GGAGCC", "GGCGCC"
        };

        public string[] tfbsConsSitesFeatureName = new string[258] {
            "Eponine_TSS", "V$MYOD_01", "V$E47_01", "V$CMYB_01", "V$AP4_01", "V$MEF2_01", "V$ELK1_01", "V$SP1_01",
            "V$EVI1_06", "V$ATF_01", "V$HOX13_01", "V$E2F_01", "V$ELK1_02", "V$RSRFC4_01", "V$CETS1P54_01", "V$P300_01",
            "V$P53_01", "V$NFE2_01", "V$CREB_01", "V$CREBP1_01", "V$CREBP1CJUN_01", "V$SOX5_01", "V$E4BP4_01", "V$E2F_02",
            "V$NFKAPPAB50_01", "V$NFKAPPAB65_01", "V$CREL_01", "V$NFKAPPAB_01", "V$NMYC_01", "V$MYOGNF1_01", "V$COMP1_01", "V$HEN1_02",
            "V$YY1_01", "V$IRF1_01", "V$IRF2_01", "V$TAL1BETAE47_01", "V$TAL1ALPHAE47_01", "V$HEN1_01", "V$YY1_02", "V$TAL1BETAITF2_01",
            "V$E47_02", "V$CP2_01", "V$GATA1_01", "V$GATA2_01", "V$GATA3_01", "V$EVI1_01", "V$EVI1_02", "V$EVI1_03",
            "V$EVI1_04", "V$EVI1_05", "V$MZF1_01", "V$MZF1_02", "V$ZID_01", "V$IK1_01", "V$IK2_01", "V$IK3_01",
            "V$CDP_01", "V$PBX1_01", "V$PAX6_01", "V$PAX2_01", "V$S8_01", "V$CDP_02", "V$CDPCR1_01", "V$CDPCR3_01",
            "V$CDPCR3HD_01", "V$NRF2_01", "V$CEBPB_01", "V$CREB_02", "V$TAXCREB_01", "V$TAXCREB_02", "V$CEBPA_01", "V$CEBPB_02",
            "V$MYCMAX_01", "V$MAX_01", "V$USF_01", "V$USF_02", "V$MYCMAX_02", "V$PBX1_02", "V$GATA1_02", "V$GATA1_03",
            "V$GATA1_04", "V$HFH1_01", "V$FOXD3_01", "V$HNF3B_01", "V$HNF1_01", "V$TST1_01", "V$HNF4_01", "V$OCT1_01",
            "V$OCT1_02", "V$OCT1_03", "V$OCT1_04", "V$AHR_01", "V$LYF1_01", "V$PAX5_01", "V$PAX5_02", "V$BRN2_01",
            "V$HSF1_01", "V$HSF2_01", "V$BRACH_01", "V$SRF_01", "V$ARP1_01", "V$RORA1_01", "V$RORA2_01", "V$COUP_01",
            "V$SRY_02", "V$OCT1_05", "V$OCT1_06", "V$AP1FJ_Q2", "V$AP1_Q2", "V$AP1_Q6", "V$AP4_Q5", "V$AP4_Q6",
            "V$CREB_Q2", "V$CREB_Q4", "V$CREBP1_Q2", "V$MYB_Q6", "V$MYOD_Q6", "V$NFY_Q6", "V$SRF_Q6", "V$USF_Q6",
            "V$AP1_Q4", "V$AP2_Q6", "V$CEBP_Q2", "V$ER_Q6", "V$GR_Q6", "V$NF1_Q6", "V$NFKB_Q6", "V$OCT1_Q6",
            "V$SP1_Q6", "V$AP1_C", "V$CEBP_C", "V$GATA_C", "V$GRE_C", "V$HNF1_C", "V$NFKB_C", "V$NFY_C",
            "V$OCT_C", "V$SEF1_C", "V$SRF_C", "V$TATA_C", "V$USF_C", "V$SREBP1_01", "V$SREBP1_02", "V$HAND1E47_01",
            "V$STAT_01", "V$STAT1_01", "V$STAT3_01", "V$MEF2_02", "V$MEF2_03", "V$MEF2_04", "V$AHRARNT_01", "V$ARNT_01",
            "V$AHRARNT_02", "V$NKX25_01", "V$NKX25_02", "V$PPARA_01", "V$EGR1_01", "V$NGFIC_01", "V$EGR3_01", "V$EGR2_01",
            "V$OCT1_07", "V$CHOP_01", "V$GFI1_01", "V$XBP1_01", "V$TATA_01", "V$NRSF_01", "V$RREB1_01", "V$ISRE_01",
            "V$HLF_01", "V$OLF1_01", "V$AML1_01", "V$P53_02", "V$LMO2COM_01", "V$LMO2COM_02", "V$MIF1_01", "V$RFX1_01",
            "V$RFX1_02", "V$TCF11MAFG_01", "V$TCF11_01", "V$NFY_01", "V$HFH3_01", "V$FREAC2_01", "V$FREAC3_01", "V$FREAC4_01",
            "V$FREAC7_01", "V$NFAT_Q6", "V$GATA1_05", "V$PAX3_01", "V$PAX4_01", "V$PAX4_02", "V$PAX4_03", "V$PAX4_04",
            "V$MSX1_01", "V$HOXA3_01", "V$EN1_01", "V$SOX9_B1", "V$HNF4_01_B", "V$AREB6_01", "V$AREB6_02", "V$AREB6_03",
            "V$AREB6_04", "V$CART1_01", "V$TGIF_01", "V$MEIS1_01", "V$MEIS1AHOXA9_01", "V$MEIS1BHOXA9_02", "V$FOXJ2_01", "V$FOXJ2_02",
            "V$NKX61_01", "V$HMX1_01", "V$CHX10_01", "V$SPZ1_01", "V$ZIC1_01", "V$ZIC2_01", "V$ZIC3_01", "V$NKX3A_01",
            "V$IRF7_01", "V$MRF2_01", "V$FAC1_01", "V$STAT5A_01", "V$STAT5B_01", "V$STAT5A_02", "V$GATA6_01", "V$POU3F2_01",
            "V$POU3F2_02", "V$POU6F1_01", "V$ROAZ_01", "V$AP2REP_01", "V$AP2ALPHA_01", "V$AP2GAMMA_01", "V$TBP_01", "V$FOXO4_01",
            "V$FOXO1_01", "V$FOXO1_02", "V$FOXO4_02", "V$FOXO3_01", "V$CDC5_01", "V$LUN1_01", "V$ATF6_01", "V$NCX_01",
            "V$NKX22_01", "V$PAX2_02", "V$BACH2_01", "V$MAZR_01", "V$BACH1_01", "V$STAT1_03", "V$STAT3_02", "V$LHX3_01",
            "V$PPARG_01", "V$PPARG_02", "V$E2F_03", "V$AP1_01", "V$GCNF_01", "V$PPARG_03", "V$RP58_01", "V$HTF_01",
            "V$ARNT_02", "V$MYCMAX_03"
        };

        public string[] rmskFeatureName = new string[105] {
                "L2", "AluSx", "MIRb", "MIR", "AluJb", "AluJo", "AluY",
                "AluSq", "AluSg", "MIR3", "AluSp", "L1M5", "L3", "AluSc",
                "MIRm", "FLAM_C", "L1ME4a", "MER5A", "L1ME3B", "L1MC4", "AluSg/x",
                "HAL1", "L1ME1", "L1MEc", "L1MC4a", "FRAM", "FLAM_A", "MER5B",
                "MLT1A0", "L1MB7", "L1M4", "THE1B", "MLT1K", "MSTA", "L1MB3",
                "L1MC5", "MLT1D", "L1ME2", "L1MEd", "MLT1C", "MLT1J", "L1ME3A",
                "MLT1B", "L4", "L1MC", "MER20", "L1M", "L1MEe", "L1MC1",
                "L1MB8", "L1MA9", "Tigger1", "MER5A1", "L1PA7", "MER3", "L1PA16",
                "MLT1I", "L1PB1", "ERVL-E", "MLT1L", "L1MC3", "L1PA4", "MER58A",
                "L3b", "THE1D", "Alu", "L1PA5", "MLT1H", "L1PA3", "L1MD",
                "L1M1", "L1MD2", "L1MA8", "L1M2", "THE1C", "LTR33", "L1MA4",
                "MER33", "L1MB4", "MLT1J2", "MER2", "FAM", "MLT1A", "L1MDa",
                "L1PREC2", "AluSq/x", "MSTB", "MSTD", "L1MA3", "L1ME3", "AluSg1",
                "L1MCa", "L1M4c", "L1M3", "L1MB2", "L1MB5", "MER103", "L1M4b",
                "L1PA8", "L1MD3", "MADE1", "L1MA7", "LTR67", "LTR16C", "L1MCc"
        };

        public string[] histoneModification18NatFeatureName = new string[18] {
                "H3K4ac", "H3K9ac", "H3K14ac", "H3K18ac", "H3K23ac", "H3K27ac",
                "H3K36ac", "H4K5ac", "H4K8ac", "H4K12ac", "H4K16ac", "H4K91ac",
                "H2AK5ac", "H2AK9ac", "H2BK5ac", "H2BK12ac", "H2BK20ac", "H2BK120ac"
        };

        public string[] histoneModification20CellFeatureName = new string[20] {
                "H3K4me1", "H3K4me2", "H3K4me3", "H3K9me1", "H3K9me2", "H3K9me3",
                "H3K27me1", "H3K27me2", "H3K27me3", "H3K36me1", "H3K36me3", "H3K79me1",
                "H3K79me2", "H3K79me3", "H3R2me1", "H3R2me2", "H4K20me1", "H4K20me3",
                "H4R3me2", "H2BK5me1"
        };

        public Hashtable motifFeatureName6HT = new Hashtable();

        public FeatureName()
        {
            List<string> AAWGGR = new List<string>();
            AAWGGR.Add("AAAGGA");
            AAWGGR.Add("AAAGGG");
            AAWGGR.Add("AATGGA");
            AAWGGR.Add("AATGGG");
            motifFeatureName6HT.Add("AAWGGR", AAWGGR);

            List<string> CCDGGV = new List<string>();
            CCDGGV.Add("CCAGGA");
            CCDGGV.Add("CCAGGC");
            CCDGGV.Add("CCAGGG");
            CCDGGV.Add("CCGGGA");
            CCDGGV.Add("CCGGGC");
            CCDGGV.Add("CCGGGG");
            CCDGGV.Add("CCTGGA");
            CCDGGV.Add("CCTGGC");
            CCDGGV.Add("CCTGGG");
            motifFeatureName6HT.Add("CCDGGV", CCDGGV);

            List<string> CCCSGS = new List<string>();
            CCCSGS.Add("CCCCGC");
            CCCSGS.Add("CCCCGG");
            CCCSGS.Add("CCCGGC");
            CCCSGS.Add("CCCGGG");
            motifFeatureName6HT.Add("CCCSGS", CCCSGS);

            List<string> AAATKT = new List<string>();
            AAATKT.Add("AAATGT");
            AAATKT.Add("AAATTT");
            motifFeatureName6HT.Add("AAATKT", AAATKT);

            List<string> BCCCWG = new List<string>();
            BCCCWG.Add("CCCCAG");
            BCCCWG.Add("CCCCTG");
            BCCCWG.Add("GCCCAG");
            BCCCWG.Add("GCCCTG");
            BCCCWG.Add("TCCCAG");
            BCCCWG.Add("TCCCTG");
            motifFeatureName6HT.Add("BCCCWG", BCCCWG);

            List<string> GSCCCS = new List<string>();
            GSCCCS.Add("GCCCCC");
            GSCCCS.Add("GCCCCG");
            GSCCCS.Add("GGCCCC");
            GSCCCS.Add("GGCCCG");
            motifFeatureName6HT.Add("GSCCCS", GSCCCS);

            List<string> ATGVAA = new List<string>();
            ATGVAA.Add("ATGAAA");
            ATGVAA.Add("ATGCAA");
            ATGVAA.Add("ATGGAA");
            motifFeatureName6HT.Add("ATGVAA", ATGVAA);

            List<string> GGVCCH = new List<string>();
            GGVCCH.Add("GGACCA");
            GGVCCH.Add("GGACCC");
            GGVCCH.Add("GGACCT");
            GGVCCH.Add("GGCCCA");
            GGVCCH.Add("GGCCCC");
            GGVCCH.Add("GGCCCT");
            GGVCCH.Add("GGGCCA");
            GGVCCH.Add("GGGCCC");
            GGVCCH.Add("GGGCCT");
            motifFeatureName6HT.Add("GGVCCH", GGVCCH);

            List<string> CCGSSC = new List<string>();
            CCGSSC.Add("CCGCCC");
            CCGSSC.Add("CCGCGC");
            CCGSSC.Add("CCGGCC");
            CCGSSC.Add("CCGGGC");
            motifFeatureName6HT.Add("CCGSSC", CCGSSC);

            List<string> TGVAAA = new List<string>();
            TGVAAA.Add("TGAAAA");
            TGVAAA.Add("TGCAAA");
            TGVAAA.Add("TGGAAA");
            motifFeatureName6HT.Add("TGVAAA", TGVAAA);

            List<string> CCCWGH = new List<string>();
            CCCWGH.Add("CCCAGA");
            CCCWGH.Add("CCCAGC");
            CCCWGH.Add("CCCAGT");
            CCCWGH.Add("CCCTGA");
            CCCWGH.Add("CCCTGC");
            CCCWGH.Add("CCCTGT");
            motifFeatureName6HT.Add("CCCWGH", CCCWGH);

            List<string> CGSCCS = new List<string>();
            CGSCCS.Add("CGCCCC");
            CGSCCS.Add("CGCCCG");
            CGSCCS.Add("CGGCCC");
            CGSCCS.Add("CGGCCG");
            motifFeatureName6HT.Add("CGSCCS", CGSCCS);

            List<string> CWGAMA = new List<string>();
            CWGAMA.Add("CAGAAA");
            CWGAMA.Add("CAGACA");
            CWGAMA.Add("CTGAAA");
            CWGAMA.Add("CTGACA");
            motifFeatureName6HT.Add("CWGAMA", CWGAMA);

            List<string> GGSCTB = new List<string>();
            GGSCTB.Add("GGCCTC");
            GGSCTB.Add("GGCCTG");
            GGSCTB.Add("GGCCTT");
            GGSCTB.Add("GGGCTC");
            GGSCTB.Add("GGGCTG");
            GGSCTB.Add("GGGCTT");
            motifFeatureName6HT.Add("GGSCTB", GGSCTB);

            List<string> VGCGGG = new List<string>();
            VGCGGG.Add("AGCGGG");
            VGCGGG.Add("CGCGGG");
            VGCGGG.Add("GGCGGG");
            motifFeatureName6HT.Add("VGCGGG", VGCGGG);

            List<string> AATKAA = new List<string>();
            AATKAA.Add("AATGAA");
            AATKAA.Add("AATTAA");
            motifFeatureName6HT.Add("AATKAA", AATKAA);

            List<string> CCTGMV = new List<string>();
            CCTGMV.Add("CCTGAA");
            CCTGMV.Add("CCTGAC");
            CCTGMV.Add("CCTGAG");
            CCTGMV.Add("CCTGCA");
            CCTGMV.Add("CCTGCC");
            CCTGMV.Add("CCTGCG");
            motifFeatureName6HT.Add("CCTGMV", CCTGMV);

            List<string> GRGCSC = new List<string>();
            GRGCSC.Add("GAGCCC");
            GRGCSC.Add("GAGCGC");
            GRGCSC.Add("GGGCCC");
            GRGCSC.Add("GGGCGC");
            motifFeatureName6HT.Add("GRGCSC", GRGCSC);

            List<string> AAATGV = new List<string>();
            AAATGV.Add("AAATGA");
            AAATGV.Add("AAATGC");
            AAATGV.Add("AAATGG");
            motifFeatureName6HT.Add("AAATGV", AAATGV);

            List<string> GMCCCN = new List<string>();
            GMCCCN.Add("GACCCG");
            GMCCCN.Add("GACCCA");
            GMCCCN.Add("GACCCT");
            GMCCCN.Add("GACCCC");
            GMCCCN.Add("GCCCCG");
            GMCCCN.Add("GCCCCA");
            GMCCCN.Add("GCCCCT");
            GMCCCN.Add("GCCCCC");
            motifFeatureName6HT.Add("GMCCCN", GMCCCN);

            List<string> TCCSSG = new List<string>();
            TCCSSG.Add("TCCCCG");
            TCCSSG.Add("TCCCGG");
            TCCSSG.Add("TCCGCG");
            TCCSSG.Add("TCCGGG");
            motifFeatureName6HT.Add("TCCSSG", TCCSSG);

            List<string> TGRAAT = new List<string>();
            TGRAAT.Add("TGAAAT");
            TGRAAT.Add("TGGAAT");
            motifFeatureName6HT.Add("TGRAAT", TGRAAT);

            List<string> SCCWCR = new List<string>();
            SCCWCR.Add("CCCACA");
            SCCWCR.Add("CCCACG");
            SCCWCR.Add("CCCTCA");
            SCCWCR.Add("CCCTCG");
            SCCWCR.Add("GCCACA");
            SCCWCR.Add("GCCACG");
            SCCWCR.Add("GCCTCA");
            SCCWCR.Add("GCCTCG");
            motifFeatureName6HT.Add("SCCWCR", SCCWCR);

            List<string> KCCSGC = new List<string>();
            KCCSGC.Add("GCCCGC");
            KCCSGC.Add("GCCGGC");
            KCCSGC.Add("TCCCGC");
            KCCSGC.Add("TCCGGC");
            motifFeatureName6HT.Add("KCCSGC", KCCSGC);

            List<string> GVAAAT = new List<string>();
            GVAAAT.Add("GAAAAT");
            GVAAAT.Add("GCAAAT");
            GVAAAT.Add("GGAAAT");
            motifFeatureName6HT.Add("GVAAAT", GVAAAT);

            List<string> WGCCCH = new List<string>();
            WGCCCH.Add("AGCCCA");
            WGCCCH.Add("AGCCCC");
            WGCCCH.Add("AGCCCT");
            WGCCCH.Add("TGCCCA");
            WGCCCH.Add("TGCCCC");
            WGCCCH.Add("TGCCCT");
            motifFeatureName6HT.Add("WGCCCH", WGCCCH);

            List<string> CTCCSS = new List<string>();
            CTCCSS.Add("CTCCCC");
            CTCCSS.Add("CTCCCG");
            CTCCSS.Add("CTCCGC");
            CTCCSS.Add("CTCCGG");
            motifFeatureName6HT.Add("CTCCSS", CTCCSS);

            List<string> TRAATT = new List<string>();
            TRAATT.Add("TAAATT");
            TRAATT.Add("TGAATT");
            motifFeatureName6HT.Add("TRAATT", TRAATT);

            List<string> CKGSCM = new List<string>();
            CKGSCM.Add("CGGCCA");
            CKGSCM.Add("CGGCCC");
            CKGSCM.Add("CGGGCA");
            CKGSCM.Add("CGGGCC");
            CKGSCM.Add("CTGCCA");
            CKGSCM.Add("CTGCCC");
            CKGSCM.Add("CTGGCA");
            CKGSCM.Add("CTGGCC");
            motifFeatureName6HT.Add("CKGSCM", CKGSCM);

            List<string> SGMGCC = new List<string>();
            SGMGCC.Add("CGAGCC");
            SGMGCC.Add("CGCGCC");
            SGMGCC.Add("GGAGCC");
            SGMGCC.Add("GGCGCC");
            motifFeatureName6HT.Add("SGMGCC", SGMGCC);
        }
    }
}