using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace GetFeatureInfo
{
    class ClassTitle
    {
        private string name;
        private string chr;
        private int length;
        private int start;
        private int end;
        private bool flagNA;

        public ClassTitle(string name, int length)
        {
            this.name = name;
            this.length = length;
            this.chr = string.Empty;
            this.flagNA = true;
        }

        public ClassTitle(string name, string chr, int length, int start, int end)
        {
            this.name = name;
            this.chr = chr;
            this.length = length;
            this.start = start;
            this.end = end;
            this.flagNA = false;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Chr
        {
            get
            {
                return chr;
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
        }

        public int Start
        {
            get
            {
                return start;
            }
        }

        public int End
        {
            get
            {
                return end;
            }
        }

        public bool FlagNA
        {
            get
            {
                return flagNA;
            }
        }
    }

    class ClassSequence
    {
        private int index;
        private ClassTitle title;
        private string sequence;
        private Hashtable motifCountHT = new Hashtable();
        private Hashtable tfbsConsSitesCountHT = new Hashtable();
        private Hashtable rmskCountHT = new Hashtable();
        private Hashtable histoneModificationCountHT = new Hashtable();

        private Hashtable userMotifCountHT = new Hashtable();
        private Hashtable userTfbsConsSitesCountHT = new Hashtable();
        private Hashtable userRmskCountHT = new Hashtable();
        private Hashtable userHistoneModificationCountHT = new Hashtable();

        public ClassSequence(int index, ClassTitle title, string seq)
        {
            this.index = index;
            this.title = title;
            this.sequence = seq;
        }

        public int Index
        {
            get
            {
                return index;
            }
        }

        public ClassTitle Title
        {
            get
            {
                return title;
            }
        }

        public string Sequence
        {
            get
            {
                return sequence;
            }
        }

        public Hashtable MotifCountHT
        {
            get
            {
                return motifCountHT;
            }
            set
            {
                motifCountHT = value;
            }
        }

        public Hashtable TfbsConsSitesCountHT
        {
            get
            {
                return tfbsConsSitesCountHT;
            }
            set
            {
                tfbsConsSitesCountHT = value;
            }
        }

        public Hashtable RmskCountHT
        {
            get
            {
                return rmskCountHT;
            }
            set
            {
                rmskCountHT = value;
            }
        }

        public Hashtable HistoneModificationCountHT
        {
            get
            {
                return histoneModificationCountHT;
            }
            set
            {
                histoneModificationCountHT = value;
            }
        }

        public Hashtable UserMotifCountHT
        {
            get
            {
                return userMotifCountHT;
            }
            set
            {
                userMotifCountHT = value;
            }
        }

        public Hashtable UserTfbsConsSitesCountHT
        {
            get
            {
                return userTfbsConsSitesCountHT;
            }
            set
            {
                userTfbsConsSitesCountHT = value;
            }
        }

        public Hashtable UserRmskCountHT
        {
            get
            {
                return userRmskCountHT;
            }
            set
            {
                userRmskCountHT = value;
            }
        }

        public Hashtable UserHistoneModificationCountHT
        {
            get
            {
                return userHistoneModificationCountHT;
            }
            set
            {
                userHistoneModificationCountHT = value;
            }
        }
    }

    class ClassSequenceList
    {
        // 核心数据结构
        private List<ClassSequence> sequenceList = new List<ClassSequence>();

        // 行数、特征名的类实体
        private ClassFeatureNameList insClassFeatureNameList = new ClassFeatureNameList();
        private ClassChrLineIndex insClassChrLineIndex = new ClassChrLineIndex();

        // 哪些特征计算过的标志位
        private bool isMotifComputed = false;
        private bool isUserMotifComputed = false;
        private bool isTfbsConsSitesComputed = false;
        private bool isUserTfbsConsSitesComputed = false;
        private bool isRmskComputed = false;
        private bool isUserRmskComputed = false;
        private bool isHistoneModificationComputed = false;
        private bool isUserHistoneModificationComputed = false;
        
        // 用户提供的特征
        private List<string> userMotifFeatureList = new List<string>();           // 用户提供的 motif 特征
        private List<string> userRmskFeatureList = new List<string>();            // 用户提供的 rmsk 特征
        private List<string> userTfbsConsSitesFeatureList = new List<string>();   // 用户提供的 tfbsConsSites 特征
        private List<string> userHistoneModification18NatFeatureList = new List<string>();     // 用户提供的 histone modification 特征
        private List<string> userHistoneModification20CellFeatureList = new List<string>();
        private List<string> userHistoneModificationFeatureList = new List<string>();

        // 用户提供的重复的特征
        private List<string> userRepeatFeatureList = new List<string>();

        // 记录了有哪些各个序列都位于哪些染色体上
        private List<string> chrList = new List<string>();

        public void Clear()
        {
            sequenceList.Clear();
            userMotifFeatureList.Clear();
            userRmskFeatureList.Clear();
            userTfbsConsSitesFeatureList.Clear();
            userHistoneModification18NatFeatureList.Clear();
            userHistoneModification20CellFeatureList.Clear();
            userRepeatFeatureList.Clear();
            chrList.Clear();

            isMotifComputed = false;
            isUserMotifComputed = false;
            isTfbsConsSitesComputed = false;
            isUserTfbsConsSitesComputed = false;
            isRmskComputed = false;
            isUserRmskComputed = false;
            isHistoneModificationComputed = false;
            isUserHistoneModificationComputed = false;
        }

        public void getChrList()
        {
            foreach (ClassSequence insClassSequence in sequenceList)
            {
                if (!chrList.Contains(insClassSequence.Title.Chr) && insClassSequence.Title.Chr != string.Empty)
                {
                    chrList.Add(insClassSequence.Title.Chr);
                }
            }
        }

        // 函数功能：返回模式 strPattern 在源序列 strSource 中出现的次数
        // 被 computeUserMotif 函数调用
        private int SubstringCount(string strSource, string strPattern)
        {
            // 利用正则表达式实现
            // 忽略大小写
            return Regex.Matches(strSource, strPattern, RegexOptions.IgnoreCase).Count;
        }

        private int Count(string p_strSource, string p_strPattern)
        {
            string strSource = p_strSource.ToUpper();
            string strPattern = p_strPattern.ToUpper();
            int count = 0;
            int index = strSource.IndexOf(strPattern, 0);
            while (index >= 0 && index < strSource.Length)
            {
                ++count;
                index = strSource.IndexOf(strPattern, index + strPattern.Length);
            }
            return count;
        }

        public void computeMotif(object obj)
        {
            ManualResetEvent mre = (ManualResetEvent)obj;

            isMotifComputed = true;

            Hashtable temp_motifFeatureName6ExpandedCountHT = new Hashtable();
            Hashtable temp_motifFeatureName6LastIndexHT = new Hashtable();
            Hashtable temp_motifFeatureName4LastIndexHT = new Hashtable();
            Hashtable temp_motifFeatureName3LastIndexHT = new Hashtable();

            foreach (ClassSequence insClassSequence in sequenceList)
            {
                int countC = 0, countG = 0, countCG = 0;
                int lastCountCG = -1;
                int FeatureName2LastIndex = -1;

                insClassSequence.MotifCountHT.Clear();
                temp_motifFeatureName4LastIndexHT.Clear();
                temp_motifFeatureName3LastIndexHT.Clear();
                foreach (string feaName in insClassFeatureNameList.motifFeatureName4)
                {
                    temp_motifFeatureName4LastIndexHT.Add(feaName, -1);
                }

                foreach (string feaName in insClassFeatureNameList.motifFeatureName3)
                {
                    temp_motifFeatureName3LastIndexHT.Add(feaName, -1);
                }

                foreach (string feaName in insClassFeatureNameList.motifFeatureName)
                {
                    insClassSequence.MotifCountHT.Add(feaName, 0);
                }

                temp_motifFeatureName6LastIndexHT.Clear();
                foreach (string feaName in insClassFeatureNameList.motifFeatureName6)
                {
                    temp_motifFeatureName6LastIndexHT.Add(feaName, -1);
                }

                for (int i = 0; i < insClassSequence.Title.Length; ++i)
                {
                    if (insClassSequence.Sequence[i] == 'C' || insClassSequence.Sequence[i] == 'c')
                    {
                        ++countC;
                    }

                    if (insClassSequence.Sequence[i] == 'G' || insClassSequence.Sequence[i] == 'g')
                    {
                        ++countG;
                    }

                    // 六元组
                    if (i <= insClassSequence.Title.Length - 6)
                    {
                        string getCurSubStr = insClassSequence.Sequence.Substring(i, 6).ToUpper();
                        if (insClassFeatureNameList.motifFeatureName6Expanded.Contains(getCurSubStr))
                        {
                            foreach (string feaName in insClassFeatureNameList.motifFeatureName6)
                            {
                                bool flag = false;
                                foreach (string expandedName in (List<string>)insClassFeatureNameList.motifFeatureName6HT[feaName])
                                {
                                    if (expandedName == getCurSubStr)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    int j = Convert.ToInt32(temp_motifFeatureName6LastIndexHT[feaName]);

                                    if (i == 0 || j == -1)
                                    {
                                        insClassSequence.MotifCountHT[feaName] = Convert.ToInt32(insClassSequence.MotifCountHT[feaName]) + 1;
                                        temp_motifFeatureName6LastIndexHT[feaName] = i;
                                    }
                                    else if ( i >= j + 6 )
                                    {
                                        insClassSequence.MotifCountHT[feaName] = Convert.ToInt32(insClassSequence.MotifCountHT[feaName]) + 1;
                                        temp_motifFeatureName6LastIndexHT[feaName] = i;
                                    }
                                }
                            }
                        }
                    }

                    // 四元组
                    if (i <= insClassSequence.Title.Length - 4)
                    {
                        string getCurSubStr = insClassSequence.Sequence.Substring(i, 4).ToUpper();
                        if (insClassFeatureNameList.motifFeatureName4.Contains(getCurSubStr))
                        {
                            int j = Convert.ToInt32(temp_motifFeatureName4LastIndexHT[getCurSubStr]);
                            if (i == 0 || j == -1)
                            {
                                insClassSequence.MotifCountHT[getCurSubStr] = Convert.ToInt32(insClassSequence.MotifCountHT[getCurSubStr]) + 1;
                                temp_motifFeatureName4LastIndexHT[getCurSubStr] = i;
                            }
                            else if (i >= j + 4)
                            {
                                insClassSequence.MotifCountHT[getCurSubStr] = Convert.ToInt32(insClassSequence.MotifCountHT[getCurSubStr]) + 1;
                                temp_motifFeatureName4LastIndexHT[getCurSubStr] = i;
                            }
                        }
                    }

                    // 三元组
                    if (i <= insClassSequence.Title.Length - 3)
                    {
                        string getCurSubStr = insClassSequence.Sequence.Substring(i, 3).ToUpper();
                        if (insClassFeatureNameList.motifFeatureName3.Contains(getCurSubStr))
                        {
                            int j = Convert.ToInt32(temp_motifFeatureName3LastIndexHT[getCurSubStr]);
                            if (i == 0 || j == -1)
                            {
                                insClassSequence.MotifCountHT[getCurSubStr] = Convert.ToInt32(insClassSequence.MotifCountHT[getCurSubStr]) + 1;
                                temp_motifFeatureName3LastIndexHT[getCurSubStr] = i;
                            }
                            else if (i >= j + 3)
                            {
                                insClassSequence.MotifCountHT[getCurSubStr] = Convert.ToInt32(insClassSequence.MotifCountHT[getCurSubStr]) + 1;
                                temp_motifFeatureName3LastIndexHT[getCurSubStr] = i;
                            }
                        }
                    }

                    // 二元组
                    if (i <= insClassSequence.Title.Length - 2)
                    {
                        string getCurSubStr = insClassSequence.Sequence.Substring(i, 2).ToUpper();
                        if (insClassFeatureNameList.motifFeatureName2 == getCurSubStr)
                        {
                            int j = FeatureName2LastIndex;
                            if (i == 0 || j == -1)
                            {
                                insClassSequence.MotifCountHT[getCurSubStr] = Convert.ToInt32(insClassSequence.MotifCountHT[getCurSubStr]) + 1;
                                FeatureName2LastIndex = i;
                            }
                            else if (i >= j + 2)
                            {
                                insClassSequence.MotifCountHT[getCurSubStr] = Convert.ToInt32(insClassSequence.MotifCountHT[getCurSubStr]) + 1;
                                FeatureName2LastIndex = i;
                            }
                        }

                        // 把 "CG" 组合出现的次数统计一并放入这里计算
                        if (string.Compare(getCurSubStr, "CG", true) == 0)
                        {
                            int j = lastCountCG;
                            if (i == 0 || j == -1)
                            {
                                ++countCG;
                                lastCountCG = i;
                            }
                            else if (i >= j + 2)
                            {
                                ++countCG;
                                lastCountCG = i;
                            }
                        }
                    }
                }

                // 再计算两个“率”
                double rate = (double)(countC + countG) / (double)insClassSequence.Title.Length;
                insClassSequence.MotifCountHT["%G+C"] = rate;
                rate = insClassSequence.Title.Length * (double)(countCG) / (double)(countC * countG);
                insClassSequence.MotifCountHT["CpG ratio"] = rate;
            }

            mre.Set();
        }

        public void computeUserMotif(object obj)
        {
            ManualResetEvent mre = (ManualResetEvent)obj;

            isUserMotifComputed = true;

            foreach (ClassSequence insClassSequence in sequenceList)
            {
                int countC = 0, countG = 0, countCG = 0, count;

                insClassSequence.UserMotifCountHT.Clear();
                foreach (string feaName in userMotifFeatureList)
                {
                    if (feaName == "%G+C")
                    {
                        countC = Count(insClassSequence.Sequence, "C");
                        countG = Count(insClassSequence.Sequence, "G");
                        double rate = (double)(countC + countG) / (double)insClassSequence.Title.Length;
                        insClassSequence.UserMotifCountHT.Add("%G+C", rate);
                    }
                    else if (feaName == "CpG ratio")
                    {
                        countCG = Count(insClassSequence.Sequence, "CG");
                        countC = Count(insClassSequence.Sequence, "C");
                        countG = Count(insClassSequence.Sequence, "G");
                        double rate = insClassSequence.Title.Length * (double)(countCG) / (double)(countC * countG);
                        insClassSequence.UserMotifCountHT.Add("CpG ratio", rate);
                    }
                    else
                    {
                        bool needRegex = false;

                        string tempFeaName = feaName;
                        foreach (string iupac in insClassFeatureNameList.iupacList)
                        {
                            if (tempFeaName.Contains(iupac))
                            {
                                needRegex = true;
                                tempFeaName = tempFeaName.Replace(iupac, insClassFeatureNameList.IUPACHT[iupac].ToString());
                            }
                        }

                        if (needRegex)
                        {
                            count = SubstringCount(insClassSequence.Sequence, tempFeaName);
                        }
                        else
                        {
                            count = Count(insClassSequence.Sequence, feaName);
                        }
                        insClassSequence.UserMotifCountHT.Add(feaName, count);
                    }
                }
            }

            mre.Set();
        }

        private string[] getTfbsConsSitesChrStartEndNamePositive(string record, bool flagE)
        {
            string[] splitStr = record.Split('\t');
            if (flagE)
            {
                string[] ChrStartEndPositive = new string[4];
                ChrStartEndPositive[0] = splitStr[1];
                ChrStartEndPositive[1] = splitStr[2];
                ChrStartEndPositive[2] = splitStr[3];
                ChrStartEndPositive[3] = splitStr[6];

                return ChrStartEndPositive;
            }
            else
            {
                string[] ChrStartEndNamePositive = new string[5];
                ChrStartEndNamePositive[0] = splitStr[1];
                ChrStartEndNamePositive[1] = splitStr[2];
                ChrStartEndNamePositive[2] = splitStr[3];
                ChrStartEndNamePositive[3] = splitStr[4];
                ChrStartEndNamePositive[4] = splitStr[6];

                return ChrStartEndNamePositive;
            }
        }

        // 函数功能：在 eponine.txt 文件中查找 Start、End 之间的特征
        // 被 tfbsConsSitesCompute 函数调用
        private void setEponineHT()
        {
            int lineIndex = 0;

            string[] ChrStartEndPositive = new string[4];
            string chr, positive;               // 某条记录中的染色体、特征名、正负链情况
            int recordStart, recordEnd;
            string record = string.Empty;       // 读出的每条记录

            // 表示取当前保存 chr 块的散列表的第 chrListIndex 项
            int chrListIndex = 0;
            StreamReader sr = new StreamReader(ClassPath.pathOfEponine, Encoding.Default);    // 开始读 eponine.txt 文件

            while (chrListIndex < chrList.Count && sr.Peek() != -1)
            {
                // 当前的染色体块是 curTargetChr
                string curTargetChr = chrList[chrListIndex].ToString();

                if (lineIndex >= (int)insClassChrLineIndex.eponineLineIndex[curTargetChr])
                {
                    ChrStartEndPositive = getTfbsConsSitesChrStartEndNamePositive(record, true);
                    chr = ChrStartEndPositive[0];

                    while (chr == curTargetChr)
                    {
                        ChrStartEndPositive = getTfbsConsSitesChrStartEndNamePositive(record, true);
                        recordStart = Convert.ToInt32(ChrStartEndPositive[1]);
                        recordEnd = Convert.ToInt32(ChrStartEndPositive[2]);
                        positive = ChrStartEndPositive[3];

                        foreach (ClassSequence insClassSequence in sequenceList)
                        {
                            if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                            {
                                if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End && positive == "+")
                                {
                                    insClassSequence.TfbsConsSitesCountHT["Eponine_TSS"] = Convert.ToInt32(insClassSequence.TfbsConsSitesCountHT["Eponine_TSS"]) + 1;
                                }
                            }
                        }

                        if (sr.Peek() != -1)
                        {
                            ++lineIndex;
                            record = sr.ReadLine().Trim();
                        }
                        else
                        {
                            break;
                        }
                    }
                    ++chrListIndex;
                }
                else
                {
                    record = sr.ReadLine().Trim(); ;
                    ++lineIndex;
                }
            }
            sr.Close();
        }

        // 函数功能：填充 tfbsConsSitesHT 哈希表
        // 这样每个文件只要扫描一次就够了，大大加快程序速度
        // 被 tfbsConsSitesCompute 调用
        private void setTfbsConsSitesHT()
        {
            int lineIndex = 0;

            string[] ChrStartEndNamePositive = new string[5];
            string chr, feaName, positive;         // 某条记录中的染色体、特征名、正负链情况
            int recordStart, recordEnd;
            string record = string.Empty;                      // 读出的每条记录

            // 表示取当前保存 chr 块的散列表的第 chrListIndex 项
            int chrListIndex = 0;
            StreamReader sr = new StreamReader(ClassPath.pathOftfbsConsSites, Encoding.Default);      // 开始读 tfbsConsSites.txt 文件

            while (chrListIndex < chrList.Count && sr.Peek() != -1)
            {
                // 当前的染色体块是 curTargetChr
                string curTargetChr = chrList[chrListIndex].ToString();

                if (lineIndex >= (int)insClassChrLineIndex.tfbsConsSitesLineIndex[curTargetChr])
                {
                    ChrStartEndNamePositive = getTfbsConsSitesChrStartEndNamePositive(record, false);
                    chr = ChrStartEndNamePositive[0];
                    while (chr == curTargetChr)     // 循环的条件是染色体块能对上
                    {
                        ChrStartEndNamePositive = getTfbsConsSitesChrStartEndNamePositive(record, false);
                        recordStart = Convert.ToInt32(ChrStartEndNamePositive[1]);
                        recordEnd = Convert.ToInt32(ChrStartEndNamePositive[2]);
                        chr = ChrStartEndNamePositive[0];
                        feaName = ChrStartEndNamePositive[3];
                        positive = ChrStartEndNamePositive[4];

                        foreach (ClassSequence insClassSequence in sequenceList)
                        {
                            if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                            {
                                if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End && positive == "+")
                                {
                                    if (insClassSequence.TfbsConsSitesCountHT.Contains(feaName))
                                    {
                                        insClassSequence.TfbsConsSitesCountHT[feaName] = Convert.ToInt32(insClassSequence.TfbsConsSitesCountHT[feaName]) + 1;
                                    }
                                }
                            }
                        }

                        if (sr.Peek() != -1)
                        {
                            ++lineIndex;
                            record = sr.ReadLine().Trim();
                        }
                        else
                        {
                            break;
                        }
                    }
                    ++chrListIndex;
                }
                else
                {
                    record = sr.ReadLine().Trim(); ;
                    ++lineIndex;
                }
            }
            sr.Close();
        }

        public void computeTfbsConsSites(object obj)
        {
            ManualResetEvent mre = (ManualResetEvent)obj;

            isTfbsConsSitesComputed = true;

            // 该标志位表示文件丢失
            // 如果文件丢失，直接全部都置为 NA
            bool isFileEponineLost = false;
            bool isFileTfbsConsSitesLost = false;

            List<string> lostTfbsConsSitesFileList = new List<string>();
            // 如果文件丢失，置不可计算标志位为 NA
            if (!File.Exists(ClassPath.pathOfEponine))
            {
                isFileEponineLost = true;
                lostTfbsConsSitesFileList.Add("eponine.txt");
            }

            if (!File.Exists(ClassPath.pathOftfbsConsSites))
            {
                isFileTfbsConsSitesLost = true;
                lostTfbsConsSitesFileList.Add("tfbsConsSites.txt");
            }

            // 先初始化哈希表
            foreach (ClassSequence insClassSequence in sequenceList)
            {
                insClassSequence.TfbsConsSitesCountHT.Clear();
                foreach (string feaName in insClassFeatureNameList.tfbsConsSitesFeatureName)
                {
                    if (insClassSequence.Title.FlagNA)
                    {
                        // 如果本身信息就残缺不全，直接置为 NA
                        insClassSequence.TfbsConsSitesCountHT.Add(feaName, "NA");
                    }
                    else
                    {
                        // 否则分情况
                        if (feaName == "Eponine_TSS")
                        {
                            if (isFileEponineLost)
                            {
                                insClassSequence.TfbsConsSitesCountHT.Add(feaName, "NA");
                            }
                            else
                            {
                                insClassSequence.TfbsConsSitesCountHT.Add(feaName, 0);
                            }
                        }
                        else
                        {
                            if (isFileTfbsConsSitesLost)
                            {
                                insClassSequence.TfbsConsSitesCountHT.Add(feaName, "NA");
                            }
                            else
                            {
                                insClassSequence.TfbsConsSitesCountHT.Add(feaName, 0);
                            }
                        }
                    }
                }
            }

            if (!isFileEponineLost)
            {
                setEponineHT();
            }
            if (!isFileTfbsConsSitesLost)
            {
                setTfbsConsSitesHT();
            }

            if (lostTfbsConsSitesFileList.Count > 0)
            {
                string errorMsg = "计算 tfbsConsSites 特征时出现异常\n";
                errorMsg += "下列文件丢失：\n";
                foreach (string lostFile in lostTfbsConsSitesFileList)
                {
                    errorMsg += lostFile + "\n";
                }
                errorMsg += "请确保缺失的文件存在于 feature 目录的 tfbsConsSites 子目录中！";
                MessageBox.Show(errorMsg, "异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            mre.Set();
        }

        private void setUserEponineHT()
        {
            int lineIndex = 0;

            string[] ChrStartEndPositive = new string[4];
            string chr, positive;               // 某条记录中的染色体、特征名、正负链情况
            int recordStart, recordEnd;
            string record = string.Empty;       // 读出的每条记录

            // 表示取当前保存 chr 块的散列表的第 chrListIndex 项
            int chrListIndex = 0;
            StreamReader sr = new StreamReader(ClassPath.pathOfEponine, Encoding.Default);    // 开始读 eponine.txt 文件

            while (chrListIndex < chrList.Count && sr.Peek() != -1)
            {
                // 当前的染色体块是 curTargetChr
                string curTargetChr = chrList[chrListIndex].ToString();

                if (lineIndex >= (int)insClassChrLineIndex.eponineLineIndex[curTargetChr])
                {
                    ChrStartEndPositive = getTfbsConsSitesChrStartEndNamePositive(record, true);
                    chr = ChrStartEndPositive[0];

                    while (chr == curTargetChr)
                    {
                        ChrStartEndPositive = getTfbsConsSitesChrStartEndNamePositive(record, true);
                        recordStart = Convert.ToInt32(ChrStartEndPositive[1]);
                        recordEnd = Convert.ToInt32(ChrStartEndPositive[2]);
                        positive = ChrStartEndPositive[3];

                        foreach (ClassSequence insClassSequence in sequenceList)
                        {
                            if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                            {
                                if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End && positive == "+")
                                {
                                    insClassSequence.UserTfbsConsSitesCountHT["Eponine_TSS"] = Convert.ToInt32(insClassSequence.UserTfbsConsSitesCountHT["Eponine_TSS"]) + 1;
                                }
                            }
                        }

                        if (sr.Peek() != -1)
                        {
                            ++lineIndex;
                            record = sr.ReadLine().Trim();
                        }
                        else
                        {
                            break;
                        }
                    }
                    ++chrListIndex;
                }
                else
                {
                    record = sr.ReadLine().Trim(); ;
                    ++lineIndex;
                }
            }
            sr.Close();
        }

        private void setUserTfbsConsSitesHT()
        {
            int lineIndex = 0;

            string[] ChrStartEndNamePositive = new string[5];
            string chr, feaName, positive;         // 某条记录中的染色体、特征名、正负链情况
            int recordStart, recordEnd;
            string record = string.Empty;                      // 读出的每条记录

            // 表示取当前保存 chr 块的散列表的第 chrListIndex 项
            int chrListIndex = 0;
            StreamReader sr = new StreamReader(ClassPath.pathOftfbsConsSites, Encoding.Default);      // 开始读 tfbsConsSites.txt 文件

            while (chrListIndex < chrList.Count && sr.Peek() != -1)
            {
                // 当前的染色体块是 curTargetChr
                string curTargetChr = chrList[chrListIndex].ToString();

                if (lineIndex >= (int)insClassChrLineIndex.tfbsConsSitesLineIndex[curTargetChr])
                {
                    ChrStartEndNamePositive = getTfbsConsSitesChrStartEndNamePositive(record, false);
                    chr = ChrStartEndNamePositive[0];
                    while (chr == curTargetChr)     // 循环的条件是染色体块能对上
                    {
                        ChrStartEndNamePositive = getTfbsConsSitesChrStartEndNamePositive(record, false);
                        recordStart = Convert.ToInt32(ChrStartEndNamePositive[1]);
                        recordEnd = Convert.ToInt32(ChrStartEndNamePositive[2]);
                        chr = ChrStartEndNamePositive[0];
                        feaName = ChrStartEndNamePositive[3];
                        positive = ChrStartEndNamePositive[4];

                        foreach (ClassSequence insClassSequence in sequenceList)
                        {
                            if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                            {
                                if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End && positive == "+")
                                {
                                    if (insClassSequence.UserTfbsConsSitesCountHT.Contains(feaName))
                                    {
                                        insClassSequence.UserTfbsConsSitesCountHT[feaName] = Convert.ToInt32(insClassSequence.UserTfbsConsSitesCountHT[feaName]) + 1;
                                    }
                                }
                            }
                        }

                        if (sr.Peek() != -1)
                        {
                            ++lineIndex;
                            record = sr.ReadLine().Trim();
                        }
                        else
                        {
                            break;
                        }
                    }
                    ++chrListIndex;
                }
                else
                {
                    record = sr.ReadLine().Trim(); ;
                    ++lineIndex;
                }
            }
            sr.Close();
        }

        public void computeUserTfbsConsSites(object obj)
        {
            ManualResetEvent mre = (ManualResetEvent)obj;

            isUserTfbsConsSitesComputed = true;

            // 该标志位表示文件丢失
            // 如果文件丢失，直接全部都置为 NA
            bool isFileEponineLost = false;
            bool isFileTfbsConsSitesLost = false;

            List<string> lostTfbsConsSitesFileList = new List<string>();
            // 如果文件丢失，置不可计算标志位为 NA
            if (!File.Exists(ClassPath.pathOfEponine))
            {
                isFileEponineLost = true;
                lostTfbsConsSitesFileList.Add("eponine.txt");
            }

            if (!File.Exists(ClassPath.pathOftfbsConsSites))
            {
                isFileTfbsConsSitesLost = true;
                lostTfbsConsSitesFileList.Add("tfbsConsSites.txt");
            }

            // 先初始化哈希表
            foreach (ClassSequence insClassSequence in sequenceList)
            {
                insClassSequence.UserTfbsConsSitesCountHT.Clear();
                foreach (string feaName in userTfbsConsSitesFeatureList)
                {
                    if (insClassSequence.Title.FlagNA)
                    {
                        // 如果本身信息就残缺不全，直接置为 NA
                        insClassSequence.UserTfbsConsSitesCountHT.Add(feaName, "NA");
                    }
                    else
                    {
                        // 否则分情况
                        if (feaName == "Eponine_TSS")
                        {
                            if (isFileEponineLost)
                            {
                                insClassSequence.UserTfbsConsSitesCountHT.Add(feaName, "NA");
                            }
                            else
                            {
                                insClassSequence.UserTfbsConsSitesCountHT.Add(feaName, 0);
                            }
                        }
                        else
                        {
                            if (isFileTfbsConsSitesLost)
                            {
                                insClassSequence.UserTfbsConsSitesCountHT.Add(feaName, "NA");
                            }
                            else
                            {
                                insClassSequence.UserTfbsConsSitesCountHT.Add(feaName, 0);
                            }
                        }
                    }
                }
            }

            if (!isFileEponineLost && userTfbsConsSitesFeatureList.Contains("Eponine_TSS"))
            {
                setUserEponineHT();
            }
            if (!isFileTfbsConsSitesLost)
            {
                setUserTfbsConsSitesHT();
            }

            if (lostTfbsConsSitesFileList.Count > 0)
            {
                string errorMsg = "计算 tfbsConsSites 特征时出现异常\n";
                errorMsg += "下列文件丢失：\n";
                foreach (string lostFile in lostTfbsConsSitesFileList)
                {
                    errorMsg += lostFile + "\n";
                }
                errorMsg += "请确保缺失的文件存在于 feature 目录的 tfbsConsSites 子目录中！";
                MessageBox.Show(errorMsg, "异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            mre.Set();
        }

        // 函数功能：返回每条 chr*_rmsk.txt 记录的起始位点位置、正负链、特征名
        // 被 rmskCompute 函数调用
        private string[] getRmskStartEndPositiveName(string record)
        {
            string[] StartEndPositiveName = new string[4];
            string[] splitStr = record.Split('\t');
            StartEndPositiveName[0] = splitStr[6];
            StartEndPositiveName[1] = splitStr[7];
            StartEndPositiveName[2] = splitStr[9];
            StartEndPositiveName[3] = splitStr[10];
            return StartEndPositiveName;
        }

        // 函数功能：填充 rmskHT 哈希表
        // 被 rmskCompute 函数调用
        private void setRmskHT(List<string> lostRmskFileList)
        {
            string[] StartEndPositiveName = new string[4];
            string Positive, feaName;
            int recordStart, recordEnd;
            string record;

            int chrListIndex = 0;
            while (chrListIndex < chrList.Count)
            {
                string curTargetChr = chrList[chrListIndex].ToString();
                // 如果该染色体的 rmsk 特征文件丢失，直接进行下一次循环，并打开下一个文件
                if (!lostRmskFileList.Contains(curTargetChr))
                {
                    string pathOfFeature = ClassPath.featurePath + @"\rmsk\" + curTargetChr + "_rmsk.txt";
                    StreamReader sr = new StreamReader(pathOfFeature, Encoding.Default);

                    while (sr.Peek() != -1)
                    {
                        record = sr.ReadLine().Trim();

                        StartEndPositiveName = getRmskStartEndPositiveName(record);
                        recordStart = Convert.ToInt32(StartEndPositiveName[0]);
                        recordEnd = Convert.ToInt32(StartEndPositiveName[1]);
                        Positive = StartEndPositiveName[2];
                        feaName = StartEndPositiveName[3];

                        foreach (ClassSequence insClassSequence in sequenceList)
                        {
                            if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                            {
                                if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End && Positive == "+")
                                {
                                    if (insClassSequence.RmskCountHT.Contains(feaName))
                                    {
                                        insClassSequence.RmskCountHT[feaName] = Convert.ToInt32(insClassSequence.RmskCountHT[feaName]) + 1;
                                    }
                                }
                            }
                        }
                    }
                    sr.Close();
                }
                ++chrListIndex;
            }
        }

        public void computeRmsk(object obj)
        {
            ManualResetEvent mre = (ManualResetEvent)obj;

            isRmskComputed = true;

            // 记录丢失了的文件名
            List<string> lostRmskFileList = new List<string>();
            foreach (ClassSequence insClassSequence in sequenceList)
            {
                insClassSequence.RmskCountHT.Clear();
                if (insClassSequence.Title.FlagNA)
                {
                    foreach (string feaName in insClassFeatureNameList.rmskFeatureName)
                    {
                        insClassSequence.RmskCountHT.Add(feaName, "NA");
                    }
                }
                else
                {
                    if (!lostRmskFileList.Contains(insClassSequence.Title.Chr))
                    {
                        // 不在丢失的文件列表中
                        string pathOfFeature = ClassPath.pathOfRmskFolder + insClassSequence.Title.Chr + @"_rmsk.txt";
                        if (!File.Exists(pathOfFeature))
                        {
                            // 首先判断是不是丢失
                            // 如果特征文件丢失，把该丢失的文件信息添加到散列表中，同时置为 NA
                            foreach (string feaName in insClassFeatureNameList.rmskFeatureName)
                            {
                                insClassSequence.RmskCountHT.Add(feaName, "NA");
                            }
                            lostRmskFileList.Add(insClassSequence.Title.Chr);
                        }
                        else
                        {
                            // 如果没丢失，先初始化结果哈希表
                            // 再逐个填充哈希表
                            foreach (string feaName in insClassFeatureNameList.rmskFeatureName)
                            {
                                insClassSequence.RmskCountHT.Add(feaName, 0);
                            }
                        }
                    }
                    else
                    {
                        // 如果文件丢失，直接置为 NA
                        foreach (string feaName in insClassFeatureNameList.rmskFeatureName)
                        {
                            insClassSequence.RmskCountHT.Add(feaName, "NA");
                        }
                    }
                }
            }

            setRmskHT(lostRmskFileList);

            if (lostRmskFileList.Count > 0)
            {
                string errorMsg = "计算 rmsk 特征时出现异常\n";
                errorMsg += "下列文件丢失：\n";
                foreach (string lostFile in lostRmskFileList)
                {
                    errorMsg += lostFile + "_rmsk.txt\n";
                }
                errorMsg += "请确保缺失的文件存在于 feature 目录的 rmsk 子目录中！";
                MessageBox.Show(errorMsg, "异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            mre.Set();
        }

        private void setUserRmskHT(List<string> lostRmskFileList)
        {
            string[] StartEndPositiveName = new string[4];
            string Positive, feaName;
            int recordStart, recordEnd;
            string record;

            int chrListIndex = 0;
            while (chrListIndex < chrList.Count)
            {
                string curTargetChr = chrList[chrListIndex].ToString();
                // 如果该染色体的 rmsk 特征文件丢失，直接进行下一次循环，并打开下一个文件
                if (!lostRmskFileList.Contains(curTargetChr))
                {
                    string pathOfFeature = ClassPath.featurePath + @"\rmsk\" + curTargetChr + "_rmsk.txt";
                    StreamReader sr = new StreamReader(pathOfFeature, Encoding.Default);

                    while (sr.Peek() != -1)
                    {
                        record = sr.ReadLine().Trim();

                        StartEndPositiveName = getRmskStartEndPositiveName(record);
                        recordStart = Convert.ToInt32(StartEndPositiveName[0]);
                        recordEnd = Convert.ToInt32(StartEndPositiveName[1]);
                        Positive = StartEndPositiveName[2];
                        feaName = StartEndPositiveName[3];

                        foreach (ClassSequence insClassSequence in sequenceList)
                        {
                            if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                            {
                                if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End && Positive == "+")
                                {
                                    if (insClassSequence.UserRmskCountHT.Contains(feaName))
                                    {
                                        insClassSequence.UserRmskCountHT[feaName] = Convert.ToInt32(insClassSequence.UserRmskCountHT[feaName]) + 1;
                                    }
                                }
                            }
                        }
                    }
                    sr.Close();
                }
                ++chrListIndex;
            }
        }

        public void computeUserRmsk(object obj)
        {
            ManualResetEvent mre = (ManualResetEvent)obj;

            isUserRmskComputed = true;

            // 记录丢失了的文件名
            List<string> lostRmskFileList = new List<string>();
            foreach (ClassSequence insClassSequence in sequenceList)
            {
                insClassSequence.UserRmskCountHT.Clear();
                if (insClassSequence.Title.FlagNA)
                {
                    foreach (string feaName in userRmskFeatureList)
                    {
                        insClassSequence.UserRmskCountHT.Add(feaName, "NA");
                    }
                }
                else
                {
                    if (!lostRmskFileList.Contains(insClassSequence.Title.Chr))
                    {
                        // 不在丢失的文件列表中
                        string pathOfFeature = ClassPath.pathOfRmskFolder + insClassSequence.Title.Chr + @"_rmsk.txt";
                        if (!File.Exists(pathOfFeature))
                        {
                            // 首先判断是不是丢失
                            // 如果特征文件丢失，把该丢失的文件信息添加到散列表中，同时置为 NA
                            foreach (string feaName in userRmskFeatureList)
                            {
                                insClassSequence.UserRmskCountHT.Add(feaName, "NA");
                            }
                            lostRmskFileList.Add(insClassSequence.Title.Chr);
                        }
                        else
                        {
                            // 如果没丢失，先初始化结果哈希表
                            // 再逐个填充哈希表
                            foreach (string feaName in userRmskFeatureList)
                            {
                                insClassSequence.UserRmskCountHT.Add(feaName, 0);
                            }
                        }
                    }
                    else
                    {
                        // 如果文件丢失，直接置为 NA
                        foreach (string feaName in userRmskFeatureList)
                        {
                            insClassSequence.UserRmskCountHT.Add(feaName, "NA");
                        }
                    }
                }
            }

            setUserRmskHT(lostRmskFileList);

            if (lostRmskFileList.Count > 0)
            {
                string errorMsg = "计算 rmsk 特征时出现异常\n";
                errorMsg += "下列文件丢失：\n";
                foreach (string lostFile in lostRmskFileList)
                {
                    errorMsg += lostFile + "_rmsk.txt\n";
                }
                errorMsg += "请确保缺失的文件存在于 feature 目录的 rmsk 子目录中！";
                MessageBox.Show(errorMsg, "异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            mre.Set();
        }

        private bool getHistoneModificationIsTrack(string record)
        {
            string[] splitStr = record.Split(' ');
            if (splitStr[0] == "track")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 函数功能：返回一条 *.vstep 文件中每行记录是否是头
        // 即，是否是形如 variableStep chrom=chr1 span=199 的记录
        // 主要就是看按 tab 键切开之后是否是 3 项
        private bool getHistoneModificationIsTitle(string record)
        {
            string[] splitStr = record.Split(' ');
            if (splitStr.Length == 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 函数功能：返回一条 *.vstep 文件中每行记录是头的记录的染色体信息
        // 即，形如 variableStep chrom=chr1 span=199 的记录中包含的 chr1
        private string getHistoneModificationChr(string record)
        {
            string[] splitStr = record.Split(' ');
            string title = splitStr[1];
            string[] splitStr2 = title.Split('=');
            return splitStr2[1];
        }

        // 函数功能：返回 *.vstep 文件中每行记录的起始位点位置、个数信息
        // 被 histoneModificationCompute 函数调用
        private int[] getHistoneModificationStartEndCount(string record)
        {
            int[] StartEndCount = new int[3];
            string[] splitStr = record.Split('\t');
            StartEndCount[0] = Convert.ToInt32(splitStr[0]);
            StartEndCount[1] = StartEndCount[0] + 200;
            StartEndCount[2] = Convert.ToInt32(splitStr[1]);
            return StartEndCount;
        }

        private void setHistoneModificationHT(List<string> lostHistoneModificationFileList)
        {
            bool isTitle = false;
            bool isTrack = false;
            int[] RecordStartEndCount = new int[3];
            int recordStart, recordEnd, Count;
            string record = string.Empty;
            string curChrSquare;

            foreach (string feaName in insClassFeatureNameList.histoneModification18NatFeatureName)
            {
                if (!lostHistoneModificationFileList.Contains(feaName))
                {
                    string pathOfFeature = ClassPath.pathOfHistoneModification18NatFolder + @"CD4-" + feaName + @"-summary.vstep";
                    StreamReader sr = new StreamReader(pathOfFeature, Encoding.Default);

                    int lineIndex = 0;
                    int chrListIndex = 0;

                    while (chrListIndex < chrList.Count && sr.Peek() != -1)
                    {
                        string curTargetChr = chrList[chrListIndex].ToString();
                        curChrSquare = curTargetChr;

                        if (lineIndex >= (int)((Hashtable)insClassChrLineIndex.histoneModification18NatFeatureNameHT[feaName])[curTargetChr])
                        {
                            while (curTargetChr == curChrSquare)
                            {
                                isTrack = getHistoneModificationIsTrack(record);
                                isTitle = getHistoneModificationIsTitle(record);
                                if (isTrack)
                                {
                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    continue;
                                }

                                if (isTitle)
                                {
                                    curChrSquare = getHistoneModificationChr(record);
                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    RecordStartEndCount = getHistoneModificationStartEndCount(record);
                                    recordStart = RecordStartEndCount[0];
                                    recordEnd = RecordStartEndCount[1];
                                    Count = RecordStartEndCount[2];

                                    foreach (ClassSequence insClassSequence in sequenceList)
                                    {
                                        if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                                        {
                                            if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End)
                                            {
                                                insClassSequence.HistoneModificationCountHT[feaName] = Convert.ToInt32(insClassSequence.HistoneModificationCountHT[feaName]) + Count;
                                            }
                                        }
                                    }

                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            ++chrListIndex;
                        }
                        else
                        {
                            record = sr.ReadLine().Trim(); ;
                            ++lineIndex;
                        }
                    }
                }
            }

            foreach (string feaName in insClassFeatureNameList.histoneModification20CellFeatureName)
            {
                if (!lostHistoneModificationFileList.Contains(feaName))
                {
                    string pathOfFeature = ClassPath.pathOfHistoneModification20CellFolder + feaName + @".vstep";
                    StreamReader sr = new StreamReader(pathOfFeature, Encoding.Default);

                    int lineIndex = 0;
                    int chrListIndex = 0;

                    while (chrListIndex < chrList.Count && sr.Peek() != -1)
                    {
                        string curTargetChr = chrList[chrListIndex].ToString();
                        curChrSquare = curTargetChr;

                        if (lineIndex >= (int)((Hashtable)insClassChrLineIndex.histoneModification20CellFeatureNameHT[feaName])[curTargetChr])
                        {
                            while (curTargetChr == curChrSquare)
                            {
                                isTrack = getHistoneModificationIsTrack(record);
                                isTitle = getHistoneModificationIsTitle(record);
                                if (isTrack)
                                {
                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    continue;
                                }

                                if (isTitle)
                                {
                                    curChrSquare = getHistoneModificationChr(record);
                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    RecordStartEndCount = getHistoneModificationStartEndCount(record);
                                    recordStart = RecordStartEndCount[0];
                                    recordEnd = RecordStartEndCount[1];
                                    Count = RecordStartEndCount[2];

                                    foreach (ClassSequence insClassSequence in sequenceList)
                                    {
                                        if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                                        {
                                            if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End)
                                            {
                                                insClassSequence.HistoneModificationCountHT[feaName] = Convert.ToInt32(insClassSequence.HistoneModificationCountHT[feaName]) + Count;
                                            }
                                        }
                                    }

                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            ++chrListIndex;
                        }
                        else
                        {
                            record = sr.ReadLine().Trim(); ;
                            ++lineIndex;
                        }
                    }
                }
            }
        }

        public void computeHistoneModification(object obj)
        {
            ManualResetEvent mre = (ManualResetEvent)obj;

            isHistoneModificationComputed = true;

            List<string> lostHistoneModificationFileList = new List<string>();
            foreach (ClassSequence insClassSequence in sequenceList)
            {
                insClassSequence.HistoneModificationCountHT.Clear();
                if (insClassSequence.Title.FlagNA)
                {
                    foreach (string feaName in insClassFeatureNameList.histoneModification18NatFeatureName)
                    {
                        insClassSequence.HistoneModificationCountHT.Add(feaName, "NA");
                    }

                    foreach (string feaName in insClassFeatureNameList.histoneModification20CellFeatureName)
                    {
                        insClassSequence.HistoneModificationCountHT.Add(feaName, "NA");
                    }
                }
                else
                {
                    foreach (string feaName in insClassFeatureNameList.histoneModification18NatFeatureName)
                    {
                        if (!lostHistoneModificationFileList.Contains(feaName))
                        {
                            string pathOfFeature = ClassPath.pathOfHistoneModification18NatFolder + @"CD4-" + feaName + @"-summary.vstep";
                            if (!File.Exists(pathOfFeature))
                            {
                                insClassSequence.HistoneModificationCountHT.Add(feaName, "NA");
                                lostHistoneModificationFileList.Add(feaName);
                            }
                            else
                            {
                                insClassSequence.HistoneModificationCountHT.Add(feaName, "0");
                            }
                        }
                        else
                        {
                            insClassSequence.HistoneModificationCountHT.Add(feaName, "NA");
                        }
                    }

                    foreach (string feaName in insClassFeatureNameList.histoneModification20CellFeatureName)
                    {
                        if (!lostHistoneModificationFileList.Contains(feaName))
                        {
                            string pathOfFeature = ClassPath.pathOfHistoneModification20CellFolder + feaName + @".vstep";
                            if (!File.Exists(pathOfFeature))
                            {
                                insClassSequence.HistoneModificationCountHT.Add(feaName, "NA");
                                lostHistoneModificationFileList.Add(feaName);
                            }
                            else
                            {
                                insClassSequence.HistoneModificationCountHT.Add(feaName, "0");
                            }
                        }
                        else
                        {
                            insClassSequence.HistoneModificationCountHT.Add(feaName, "NA");
                        }
                    }
                }
            }

            setHistoneModificationHT(lostHistoneModificationFileList);

            if (lostHistoneModificationFileList.Count > 0)
            {
                string errorMsg = "计算 histone modification 特征时出现异常\n";
                errorMsg += "下列文件丢失：\n";
                foreach (string lostFile in lostHistoneModificationFileList)
                {
                    if (insClassFeatureNameList.histoneModification18NatFeatureName.Contains(lostFile))
                    {
                        errorMsg += "CD4-" + lostFile + "-summary.vstep";
                    }
                    else
                    {
                        errorMsg += lostFile + ".vstep";
                    }
                }
                errorMsg += "请确保缺失的文件存在于 feature 目录的 histone modification 子目录中的相应子目录中！";
                MessageBox.Show(errorMsg, "异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            mre.Set();
        }

        private void setUserHistoneModificationHT(List<string> lostHistoneModificationFileList)
        {
            bool isTrack = false;
            bool isTitle = false;
            int[] RecordStartEndCount = new int[3];
            int recordStart, recordEnd, Count;
            string record = string.Empty;
            string curChrSquare;

            foreach (string feaName in userHistoneModification18NatFeatureList)
            {
                if (!lostHistoneModificationFileList.Contains(feaName))
                {
                    string pathOfFeature = ClassPath.pathOfHistoneModification18NatFolder + "CD4-" + feaName + "-summary.vstep";
                    StreamReader sr = new StreamReader(pathOfFeature, Encoding.Default);

                    int lineIndex = 0;
                    int chrListIndex = 0;

                    while (chrListIndex < chrList.Count && sr.Peek() != -1)
                    {
                        string curTargetChr = chrList[chrListIndex].ToString();
                        curChrSquare = curTargetChr;

                        if (lineIndex >= (int)((Hashtable)insClassChrLineIndex.histoneModification18NatFeatureNameHT[feaName])[curTargetChr])
                        {
                            while (curTargetChr == curChrSquare)
                            {
                                isTrack = getHistoneModificationIsTrack(record);
                                isTitle = getHistoneModificationIsTitle(record);
                                if (isTrack)
                                {
                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    continue;
                                }

                                if (isTitle)
                                {
                                    curChrSquare = getHistoneModificationChr(record);
                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {

                                    RecordStartEndCount = getHistoneModificationStartEndCount(record);
                                    recordStart = RecordStartEndCount[0];
                                    recordEnd = RecordStartEndCount[1];
                                    Count = RecordStartEndCount[2];

                                    foreach (ClassSequence insClassSequence in sequenceList)
                                    {
                                        if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                                        {
                                            if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End)
                                            {
                                                insClassSequence.UserHistoneModificationCountHT[feaName] = Convert.ToInt32(insClassSequence.UserHistoneModificationCountHT[feaName]) + Count;
                                            }
                                        }
                                    }

                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            ++chrListIndex;
                        }
                        else
                        {
                            record = sr.ReadLine().Trim(); ;
                            ++lineIndex;
                        }
                    }
                }
            }

            foreach (string feaName in userHistoneModification20CellFeatureList)
            {
                if (!lostHistoneModificationFileList.Contains(feaName))
                {
                    string pathOfFeature = ClassPath.pathOfHistoneModification20CellFolder + feaName + @".vstep";
                    StreamReader sr = new StreamReader(pathOfFeature, Encoding.Default);

                    int lineIndex = 0;
                    int chrListIndex = 0;

                    while (chrListIndex < chrList.Count && sr.Peek() != -1)
                    {
                        string curTargetChr = chrList[chrListIndex].ToString();
                        curChrSquare = curTargetChr;

                        if (lineIndex >= (int)((Hashtable)insClassChrLineIndex.histoneModification20CellFeatureNameHT[feaName])[curTargetChr])
                        {
                            while (curTargetChr == curChrSquare)
                            {
                                isTrack = getHistoneModificationIsTrack(record);
                                isTitle = getHistoneModificationIsTitle(record);
                                if (isTrack)
                                {
                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    continue;
                                }
                                if (isTitle)
                                {
                                    curChrSquare = getHistoneModificationChr(record);
                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    RecordStartEndCount = getHistoneModificationStartEndCount(record);
                                    recordStart = RecordStartEndCount[0];
                                    recordEnd = RecordStartEndCount[1];
                                    Count = RecordStartEndCount[2];

                                    foreach (ClassSequence insClassSequence in sequenceList)
                                    {
                                        if (insClassSequence.Title.Chr == curTargetChr && !insClassSequence.Title.FlagNA)
                                        {
                                            if (recordStart >= insClassSequence.Title.Start && recordEnd <= insClassSequence.Title.End)
                                            {
                                                insClassSequence.UserHistoneModificationCountHT[feaName] = Convert.ToInt32(insClassSequence.UserHistoneModificationCountHT[feaName]) + Count;
                                            }
                                        }
                                    }

                                    if (sr.Peek() != -1)
                                    {
                                        ++lineIndex;
                                        record = sr.ReadLine().Trim();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            ++chrListIndex;
                        }
                        else
                        {
                            record = sr.ReadLine().Trim(); ;
                            ++lineIndex;
                        }
                    }
                }
            }
        }

        public void computeUserHistoneModification(object obj)
        {
            ManualResetEvent mre = (ManualResetEvent)obj;

            isUserHistoneModificationComputed = true;

            List<string> lostHistoneModificationFileList = new List<string>();
            foreach (ClassSequence insClassSequence in sequenceList)
            {
                insClassSequence.UserHistoneModificationCountHT.Clear();
                if (insClassSequence.Title.FlagNA)
                {
                    foreach (string feaName in userHistoneModification18NatFeatureList)
                    {
                        insClassSequence.UserHistoneModificationCountHT.Add(feaName, "NA");
                    }

                    foreach (string feaName in userHistoneModification20CellFeatureList)
                    {
                        insClassSequence.UserHistoneModificationCountHT.Add(feaName, "NA");
                    }
                }
                else
                {
                    foreach (string feaName in userHistoneModification18NatFeatureList)
                    {
                        if (!lostHistoneModificationFileList.Contains(feaName))
                        {
                            string pathOfFeature = ClassPath.pathOfHistoneModification18NatFolder + @"CD4-" + feaName + @"-summary.vstep";
                            if (!File.Exists(pathOfFeature))
                            {
                                insClassSequence.UserHistoneModificationCountHT.Add(feaName, "NA");
                                lostHistoneModificationFileList.Add(feaName);
                            }
                            else
                            {
                                insClassSequence.UserHistoneModificationCountHT.Add(feaName, "0");
                            }
                        }
                        else
                        {
                            insClassSequence.UserHistoneModificationCountHT.Add(feaName, "NA");
                        }
                    }

                    foreach (string feaName in userHistoneModification20CellFeatureList)
                    {
                        if (!lostHistoneModificationFileList.Contains(feaName))
                        {
                            string pathOfFeature = ClassPath.pathOfHistoneModification20CellFolder + feaName + @".vstep";
                            if (!File.Exists(pathOfFeature))
                            {
                                insClassSequence.UserHistoneModificationCountHT.Add(feaName, "NA");
                                lostHistoneModificationFileList.Add(feaName);
                            }
                            else
                            {
                                insClassSequence.UserHistoneModificationCountHT.Add(feaName, "0");
                            }
                        }
                        else
                        {
                            insClassSequence.UserHistoneModificationCountHT.Add(feaName, "NA");
                        }
                    }
                }
            }

            setUserHistoneModificationHT(lostHistoneModificationFileList);

            if (lostHistoneModificationFileList.Count > 0)
            {
                string errorMsg = "计算 histone modification 特征时出现异常\n";
                errorMsg += "下列文件丢失：\n";
                foreach (string lostFile in lostHistoneModificationFileList)
                {
                    if (insClassFeatureNameList.histoneModification18NatFeatureName.Contains(lostFile))
                    {
                        errorMsg += "CD4-" + lostFile + "-summary.vstep";
                    }
                    else
                    {
                        errorMsg += lostFile + ".vstep";
                    }
                }
                errorMsg += "请确保缺失的文件存在于 feature 目录的 histone modification 子目录中的相应子目录中！";
                MessageBox.Show(errorMsg, "异常", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            mre.Set();
        }

        public List<ClassSequence> SequenceList
        {
            get
            {
                return sequenceList;
            }
            set
            {
                sequenceList = value;
            }
        }

        public List<string> UserMotifFeatureList
        {
            get
            {
                return userMotifFeatureList;
            }
            set
            {
                userMotifFeatureList = value;
            }
        }

        public List<string> UserRmskFeatureList
        {
            get
            {
                return userRmskFeatureList;
            }
            set
            {
                userRmskFeatureList = value;
            }
        }

        public List<string> UserTfbsConsSitesFeatureList
        {
            get
            {
                return userTfbsConsSitesFeatureList;
            }
            set
            {
                userTfbsConsSitesFeatureList = value;
            }
        }

        public List<string> UserHistoneModificationFeatureList
        {
            get
            {
                return userHistoneModificationFeatureList;
            }
            set
            {
                userHistoneModificationFeatureList = value;
            }
        }

        public List<string> UserHistoneModification18NatFeatureList
        {
            get
            {
                return userHistoneModification18NatFeatureList;
            }
            set
            {
                userHistoneModification18NatFeatureList = value;
            }
        }

        public List<string> UserHistoneModification20CellFeatureList
        {
            get
            {
                return userHistoneModification20CellFeatureList;
            }
            set
            {
                userHistoneModification20CellFeatureList = value;
            }
        }

        public List<string> UserRepeatFeatureList
        {
            get
            {
                return userRepeatFeatureList;
            }
            set
            {
                userRepeatFeatureList = value;
            }
        }

        public bool IsMotifComputed
        {
            get
            {
                return isMotifComputed;
            }
        }

        public bool IsUserMotifComputed
        {
            get
            {
                return isUserMotifComputed;
            }
        }

        public bool IsTfbsConsSitesComputed
        {
            get
            {
                return isTfbsConsSitesComputed;
            }
        }

        public bool IsUserTfbsConsSitesComputed
        {
            get
            {
                return isUserTfbsConsSitesComputed;
            }
        }

        public bool IsRmskComputed
        {
            get
            {
                return isRmskComputed;
            }
        }

        public bool IsUserRmskComputed
        {
            get
            {
                return isUserRmskComputed;
            }
        }

        public bool IsHistoneModificationComputed
        {
            get
            {
                return isHistoneModificationComputed;
            }
        }

        public bool IsUserHistoneModificationComputed
        {
            get
            {
                return isUserHistoneModificationComputed;
            }
        }
    }
}