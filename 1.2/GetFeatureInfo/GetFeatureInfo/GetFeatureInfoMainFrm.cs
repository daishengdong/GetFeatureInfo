using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;


// 程序很可能出 bug 的地方：
// 1. 对每个特征个数进行提取时，count 数没有清零
// 2. 输入输出流没有关闭
// 3. 文件路径写错
// 4. 线程执行的先后顺序
// 5. 存储结果的哈希表没有清空或置零
// 6. 一些散列表不恰当的置空
// 7. 哈希表由于键已存在而导致的重复添加

namespace GetFeatureInfo
{
    public partial class FrmGetFeatureInfo : Form
    {
        // 自定义的类私有成员变量
        private bool isSeqBtn = true;                                       // 是否是有关序列的按钮
        private bool isSaved = true;                                        // 计算结果是否已保存

        private SaveFileDialog sfd = new SaveFileDialog();                  // 保存计算结果另存为的对话框
        private ArrayList seqFileList = new ArrayList();                    // 保存提供序列的文件的路径
        private ArrayList feaFileList = new ArrayList();                    // 保存提供特征的文件的路径

        // 一下是与用户有关的记录变量
        private ArrayList userRepeatFeatureList = new ArrayList();              // 存放用户输入的重复的特征

        // 为去除重复设置的变量
        private ArrayList userUpperFeatureList = new ArrayList();               // 存放用户输入的大写的特征

        // 保存用户提供的特征
        private ArrayList userMotifFeatureList = new ArrayList();           // 用户提供的 motif 特征
        private ArrayList userRmskFeatureList = new ArrayList();            // 用户提供的 rmsk 特征
        private ArrayList userTfbsConsSitesFeatureList = new ArrayList();   // 用户提供的 tfbsConsSites 特征
        private ArrayList userHistoneModification18NatFeatureList = new ArrayList();     // 用户提供的 histone modification 特征
        private ArrayList userHistoneModification20CellFeatureList = new ArrayList();
            
        
        // 保存用户提供特征计算结果的哈希表
        private Hashtable userTfbsConsSitesHT = new Hashtable();            // 保存用户提供的 tfbsConsSites 特征出现次数的哈希表
        private Hashtable userRmskHT = new Hashtable();                     // 保存用户提供的 rmsk 特征出现次数的哈希表

        // 保存计算结果的哈希表
        private Hashtable tfbsConsSitesHT = new Hashtable();                // 保存 tfbsConsSites 特征出现次数的哈希表
        private Hashtable rmskHT = new Hashtable();                         // 保存 rmsk 特征出现次数的哈希表

        private Hashtable IUPACHT = new Hashtable();                        // 保存 IUPAC 对应值的哈希表

        private Hashtable blatNameChrHT = new Hashtable();                  // 序列名字为键、染色体信息为值的哈希表
        private Hashtable blatNameRecordHT = new Hashtable();               // 序列名字为键、整条 tempOut.psl 记录为值得哈希表
        private ArrayList blatTitleNameList = new ArrayList();              // 需要使用 blat 的序列的头名称

        private ChrLineIndex insChrLineIndex = new ChrLineIndex();          // 记录每个文件特定染色体所在行数的类

        // 系统的相关路径
        private static string rootPath = Application.StartupPath;           // 程序所在根目录
        private static string tempDirPath = rootPath + @"\temp\";           // 临时文件存放目录
        private static string featurePath = rootPath + @"\feature\";        // 特征文件存放目录
        private static string blatPath = rootPath + @"\blat\";              // blat 存放目录
        private static string titleFileName = tempDirPath + "titleFile";    // 存放标题的临时文件
        private static string seqFileName = tempDirPath + "seqFile";        // 存放序列本身的临时文件
        private static string feaFileName = tempDirPath + "feature";        // 存放特征的临时文件

        private static string pathOfBlat = blatPath + @"blat.exe";              // blat 可执行文件路径
        private static string tempOutFileName = tempDirPath + @"tempOut.psl";   // 存放临时输出索引的文件
        private static string entireFileName = rootPath + @"\entireFile.fa";    // 存放全部信息的临时文件

        // 一些全局变量，为了线程之间传递参数方便而设计
        private bool NA = false;                                            // 无法计算出起始位点信息
        private string TITLE;                                               // 存放 title 文件每行读出的 title，为线程传递参数
        private string SEQUENCE;                                            // 存放每行读出的序列
        private string TITLENAME;                                           // TITLE 包含的序列名称信息
        private int TITLESTART;                                             // TITLE 包含的起位点信息
        private int TITLEEND;                                               // TITLE 包含的结束位点信息
        private string TITLECHR;                                            // TITLE 包含的染色体信息
        private int TITLELENGTH;                                            // TITLE 包含的染色体长度

        // 特征计算所需的文件
        private static string pathOfEponine = featurePath + @"tfbsConsSites\eponine.txt";               // 存放 eponine 特征计算的文件
        private static string pathOftfbsConsSites = featurePath + @"tfbsConsSites\tfbsConsSites.txt";   // 存放 tfbsConsSites 特征计算的文件

        // 一些计算线程实体
        private Thread trdCompute;                                          // 主计算线程实体
        private Thread trdPreTreatment;                                     // 序列文件预处理进程实体
        private Thread trdMotifCompute;                                     // motif 特征计算线程实体
        private Thread trdTfbsConsSitesCompute;                             // tfbsConsSites 特征计算线程实体
        private Thread trdRmskCompute;                                      // rmsk 特征计算线程实体
        private Thread trdHistoneModificationCompute;                       // histone modification 特征计算线程实体

        private Thread trdUserMotifCompute;                                 // 用户 motif 特征计算线程实体
        private Thread trdUserTfbsConsSitesCompute;                         // 用户 tfbsConsSites 特征计算线程实体
        private Thread trdUserRmskCompute;                                  // 用户 rmsk 特征计算线程实体
        private Thread trdUserHistoneModificationCompute;                   // 用户 histone modification 特征计算线程实体

        // 线程是否被创建的标志位
        private bool isComputeThreadCreated = false;                        // 计算线程是否已被创建
        private bool isPreTreatmentThreadCreated = false;                   // 预处理线程是否已被创建
        private bool isMotifComputeThreadCreated = false;                   // motif 特征计算线程是否已被创建
        private bool isTfbsConsSitesComputeThreadCreated = false;           // tfbsConsSites 特征计算线程是否已被创建
        private bool isRmskComputeThreadCreated = false;                    // rmsk 特征计算线程是否已被创建
        private bool isHistoneModificationComputeThreadCreated = false;     // histone modification 特征计算线程是否已被创建

        private bool isUserMotifComputeThreadCreated = false;               // 用户 motif 特征计算线程是否已被创建
        private bool isUserTfbsConsSitesComputeThreadCreated = false;       // 用户 tfbsConsSites 特征计算线程是否已被创建
        private bool isUserRmskComputeThreadCreated = false;                // 用户 rmsk 特征计算线程是否已被创建
        private bool isUserHistoneModificationComputeThreadCreated = false; // 用户 histone modification 特征计算线程是否已被创建

        private FeatureName insFeatureName = new FeatureName();

        // 函数功能：窗体和控件的初始化
        public FrmGetFeatureInfo()
        {
            InitializeComponent();
        }

        // 退出程序 按钮对应的函数
        // 函数功能：退出应用程序
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 函数功能：返回模式 strPattern 在源序列 strSource 中出现的次数
        // 被 StartToCompute 函数调用
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

        // 函数功能：剖析出一条 eponine.txt、tfbsConsSites.txt 记录中的起始位点位置
        // 被 getEponineCount、getTfbsConsSitesCount 函数调用
        private int[] getStartEndSite(string record)
        {
            int[] StartEnd = new int[2];
            string[] splitStr = record.Split('\t');
            StartEnd[0] = Convert.ToInt32(splitStr[2]);
            StartEnd[1] = Convert.ToInt32(splitStr[3]);
            return StartEnd;
        }

        // 函数功能：返回 tmpOut.psl 文件中每行记录的最大匹配、序列总长度、染色体位置
        // 被 setblatNameRecordHT 函数调用
        private string[] getMaxPositiveNameLengthChr(string record)
        {
            string[] MaxPositiveNameLengthChr = new string[5];
            string[] splitStr = record.Split('\t');
            MaxPositiveNameLengthChr[0] = splitStr[0];
            MaxPositiveNameLengthChr[1] = splitStr[8];
            MaxPositiveNameLengthChr[2] = ">" + splitStr[9];          // 加上前缀 '>'
            MaxPositiveNameLengthChr[3] = splitStr[10];
            MaxPositiveNameLengthChr[4] = splitStr[13];
            return MaxPositiveNameLengthChr;
        }

        // 函数功能：通过扫描一趟输出文件，填充 blatNameRecordH 哈希表
        // 被 startToCompute 调用
        private void setblatNameRecordHT()
        {
            string[] MaxPositiveNameLengthChr;
            int Max;
            string Positive;
            string Name;
            int Length;
            string Chr;

            string record;
            StreamReader sr = new StreamReader(tempOutFileName, Encoding.Default);

            Hashtable tempNameMaxHT = new Hashtable();
            Hashtable tempNameChrHT = new Hashtable();

            // 初始化哈希表
            foreach (string blatTitleName in blatTitleNameList)
            {
                if (!tempNameMaxHT.Contains(blatTitleName))
                {
                    // 每个染色体的最大匹配数预置为 -1
                    tempNameMaxHT.Add(blatTitleName, -1);
                }

                if (!tempNameChrHT.Contains(blatTitleName))
                {
                    // 为了计算出没有提供染色体位置（只提供了染色体名称）的序列所在染色体位置而设置的哈希表，预置为空
                    tempNameChrHT.Add(blatTitleName, string.Empty);
                }

                if (!blatNameRecordHT.Contains(blatTitleName))
                {
                    blatNameRecordHT.Add(blatTitleName, string.Empty);
                }
            }

            while (sr.Peek() != -1)
            {
                record = sr.ReadLine().Trim();
                MaxPositiveNameLengthChr = getMaxPositiveNameLengthChr(record);
                Max = Convert.ToInt32(MaxPositiveNameLengthChr[0]);
                Positive = MaxPositiveNameLengthChr[1];
                Name = MaxPositiveNameLengthChr[2];
                Length = Convert.ToInt32(MaxPositiveNameLengthChr[3]);
                Chr = MaxPositiveNameLengthChr[4];

                // 须是正链
                if (Positive == "+")
                {
                    if ((string)blatNameChrHT[Name] == string.Empty)
                    {
                        if (Max > (int)tempNameMaxHT[Name] && ((double)Max / Length >= 0.8))
                        {
                            tempNameMaxHT[Name] = Max;
                            blatNameRecordHT[Name] = record;
                            tempNameChrHT[Name] = Chr;
                        }
                    }
                    else        // 如果用户提供有染色体位置，以用户提供的为准
                    {
                        if (Max > (int)tempNameMaxHT[Name] && (string)blatNameChrHT[Name] == Chr && ((double)Max / Length >= 0.8))
                        {
                            tempNameMaxHT[Name] = Max;
                            blatNameRecordHT[Name] = record;
                        }
                    }
                }
            }
            sr.Close();

            // 为没有提供染色体位置的序列添加染色体位置
            foreach (string blatTitleName in blatTitleNameList)
            {
                if ((string)blatNameChrHT[blatTitleName] == string.Empty)
                {
                    blatNameChrHT[blatNameChrHT] = tempNameChrHT[blatNameChrHT];
                }
            }
        }

        // 函数功能：设置 TITLENAME、TITLECHR、TITLESTART、TITLEEND 全局变量
        // 不需要再调用特定的函数进行计算
        // 被 StartToCompute 函数调用
        private void setNameChrLengthStartEnd()
        {
            string[] splitStr = TITLE.Split('\t');
            TITLELENGTH = SEQUENCE.Length;
            TITLENAME = splitStr[0];

            NA = false;

            // 只有这三个多选框被选中时计算染色体位置、起始位点才有意义
            if (cBoxMod2.Checked || cBoxMod3.Checked || cBoxMod4.Checked || userTfbsConsSitesFeatureList.Count > 0 || userRmskFeatureList.Count > 0 || userHistoneModification18NatFeatureList.Count > 0 || userHistoneModification20CellFeatureList.Count > 0)
            {
                if (splitStr.Length < 5)
                {
                    // 小于 5 调用 blat 计算出的哈希表结果
                    string record = (string)blatNameRecordHT[TITLENAME];

                    if (record == string.Empty)
                    {
                        NA = true;
                        return;
                    }

                    string[] tempSplitStr = record.Split('\t');
                    int Max, Start, End;
                    string Chr;
                    Max = Convert.ToInt32(tempSplitStr[0]);
                    Chr = tempSplitStr[13];
                    Start = Convert.ToInt32(tempSplitStr[15]);
                    End = Convert.ToInt32(tempSplitStr[16]);
                    TITLECHR = Chr;
                    TITLESTART = Start;
                    TITLEEND = End;
                }
                else
                {
                    TITLECHR = splitStr[1];
                    TITLESTART = Convert.ToInt32(splitStr[3]);
                    TITLEEND = Convert.ToInt32(splitStr[4]);
                }
            }
        }

        // 函数功能：返回 eponine.txt、tfbsConsSites.txt 文件中每行记录中是否是正链，是则返回 true
        // 被 getEponineCount、getTfbsConsSitesCount 函数调用
        private bool getIsTfbsConsSitesPositive(string record)
        {
            string[] splitStr = record.Split('\t');
            if (splitStr[6] == "+")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 函数功能：在 eponine.txt 文件中查找 Start、End 之间的特征
        // 被 tfbsConsSitesCompute 函数调用
        private int getEponineCount()
        {
            int lineIndex = 1;

            bool isPositive = false;            // eponine.txt 文件中的某条记录是否是正链
            int count = 0;                      // 特征出现的次数
            int[] StartEnd = new int[2];        // eponine.txt 文件中的某条记录的起始位点
            int thisStart, thisEnd;
            string record;                      // 每行读出的 eponine.txt 文件的某条记录

            StreamReader sr = new StreamReader(pathOfEponine, Encoding.Default);    // 开始读 eponine.txt 文件
            while (sr.Peek() != -1)
            {
                record = sr.ReadLine().Trim();

                if (lineIndex >= (int)insChrLineIndex.EponineLineIndex[TITLECHR])   // 预先已计算好相应染色体所在行数
                {
                    StartEnd = getStartEndSite(record);     // 找到后得出 record 中包含的起始位点信息

                    thisStart = StartEnd[0];
                    thisEnd = StartEnd[1];
                    isPositive = getIsTfbsConsSitesPositive(record);    // 是否是正链

                    if (thisStart >= TITLESTART && thisEnd <= TITLEEND && isPositive)       // 起始位点覆盖区域吻合且是正链
                    {
                        ++count;                            // 特征计数 + 1
                    }
                    if (thisEnd > TITLEEND)                 // 若越界直接跳出循环
                    {
                        break;
                    }
                }
                else
                {
                    ++lineIndex;                
                }
            }
            sr.Close();                                 // 关闭流
            return count;                               // 返回特征出现的次数
        }

        // 函数功能：返回一条 tfbsConsSites.txt 文件记录中的特征名
        // 被 getTfbsConsSitesCount 函数调用
        private string getTfbsConsSitesName(string record)
        {
            string[] splitStr = record.Split('\t');
            return splitStr[4];
        }

        // 函数功能：填充 tfbsConsSitesHT 哈希表
        // 这样每个文件只要扫描一次就够了，大大加快程序速度
        // 被 tfbsConsSitesCompute 调用
        private void setTfbsConsSitesHT()
        {
            int lineIndex = 1;

            bool isPositive = false;            // 是否是正链
            int[] StartEnd = new int[2];        // 每条 record 中的起始位点信息
            int thisStart, thisEnd;
            string record;                      // 读出的每条记录

            StreamReader sr = new StreamReader(pathOftfbsConsSites, Encoding.Default);      // 开始读 tfbsConsSites.txt 文件
            tfbsConsSitesHT["Eponine_TSS"] = getEponineCount();
            while (sr.Peek() != -1)
            {
                record = sr.ReadLine().Trim();

                if (lineIndex >= (int)insChrLineIndex.TfbsConsSitesLineIndex[TITLECHR])
                {
                    StartEnd = getStartEndSite(record); // 得出 record 中的起始位点信息
                    thisStart = StartEnd[0];
                    thisEnd = StartEnd[1];
                    isPositive = getIsTfbsConsSitesPositive(record);    // 是否是正链

                    if (thisStart >= TITLESTART && thisEnd <= TITLEEND && isPositive) // 起始位点覆盖区域吻合
                    {
                        if (tfbsConsSitesHT.Contains(getTfbsConsSitesName(record)))
                        {
                            tfbsConsSitesHT[getTfbsConsSitesName(record)] = (int)tfbsConsSitesHT[getTfbsConsSitesName(record)] + 1;
                        }
                    }
                    if (thisEnd > TITLEEND)             // 越界直接跳出循环
                    {
                        break;
                    }
                }
                else
                {
                    ++lineIndex;
                }
            }
            sr.Close();                             // 关闭流
        }

        // 函数功能：填充用户提供的 tfbsConsSites 对应计算的哈希表
        // 被 startToCompute 函数调用
        private void setUserTfbsConsSitesHT()
        {
            int lineIndex = 1;

            bool isPositive = false;            // 是否是正链
            int[] StartEnd = new int[2];        // 每条 record 中的起始位点信息
            int thisStart, thisEnd;
            string record;                      // 读出的每条记录

            StreamReader sr = new StreamReader(pathOftfbsConsSites, Encoding.Default);      // 开始读 tfbsConsSites.txt 文件
            if (userTfbsConsSitesHT.Contains("Eponine_TSS"))
            {
                userTfbsConsSitesHT["Eponine_TSS"] = getEponineCount();
            }
            while (sr.Peek() != -1)
            {
                record = sr.ReadLine().Trim();

                if (lineIndex >= (int)insChrLineIndex.TfbsConsSitesLineIndex[TITLECHR])
                {

                    StartEnd = getStartEndSite(record); // 得出 record 中的起始位点信息
                    thisStart = StartEnd[0];
                    thisEnd = StartEnd[1];
                    isPositive = getIsTfbsConsSitesPositive(record);    // 是否是正链

                    if (thisStart >= TITLESTART && thisEnd <= TITLEEND && isPositive) // 起始位点覆盖区域吻合
                    {
                        if (userTfbsConsSitesHT.Contains(getTfbsConsSitesName(record)))
                        {
                            userTfbsConsSitesHT[getTfbsConsSitesName(record)] = (int)userTfbsConsSitesHT[getTfbsConsSitesName(record)] + 1;
                        }
                    }
                    if (thisEnd > TITLEEND)             // 越界直接跳出循环
                    {
                        break;
                    }
                }
                else
                {
                    ++lineIndex;
                }
            }
            sr.Close();                             // 关闭流
        }

        // 函数功能：计算用户提供的 tfbsConsSites 特征
        //被 StartToCompute 函数调用
        private void userTfbsConsSitesCompute()
        {
            bool isEponineFirstLost = true;
            bool isTfbsConsSitesLost = true;
            // 如果文件丢失，置不可计算标志位为 true
            if (!File.Exists(pathOfEponine) && isEponineFirstLost)
            {
                MessageBox.Show("eponine.txt 文件丢失！\n请确保该文件存在于 feature 目录的 tfbsConsSites 子目录中！\n", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NA = true;
                isEponineFirstLost = false;
            }

            if (!File.Exists(pathOftfbsConsSites) && isTfbsConsSitesLost)
            {
                MessageBox.Show("tfbsConsSites.txt 文件丢失！\n请确保该文件存在于 feature 目录的 tfbsConsSites 子目录中！\n", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NA = true;
                isTfbsConsSitesLost = false;
            }

            if (!NA)
            {
                setUserTfbsConsSitesHT();
            }

            foreach (string feaName in userTfbsConsSitesFeatureList)
            {
                if (NA)             // 不可计算直接打印 NA
                {
                    rTxtBoxResult.Text += "NA\t";
                }
                else
                {
                    rTxtBoxResult.Text += string.Format("{0:F5}", (int)userTfbsConsSitesHT[feaName] * 10 / (double)TITLELENGTH) + "\t";     // 打印结果
                }
            }
        }

        // 函数功能：tfbsConsSites 特征的提取
        // 当相应多选框被选中时被 StartToCompute 函数调用
        private void tfbsConsSitesCompute()
        {
            bool isEponineFirstLost = true;
            bool isTfbsConsSitesLost = true;

            // 如果文件丢失，置不可计算标志位为 true
            if (!File.Exists(pathOfEponine) && isEponineFirstLost)
            {
                MessageBox.Show("eponine.txt 文件丢失！\n请确保该文件存在于 feature 目录的 tfbsConsSites 子目录中！\n", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NA = true;
                isEponineFirstLost = false;
            }

            if (!File.Exists(pathOftfbsConsSites) && isTfbsConsSitesLost)
            {
                MessageBox.Show("tfbsConsSites.txt 文件丢失！\n请确保该文件存在于 feature 目录的 tfbsConsSites 子目录中！\n", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NA = true;
                isTfbsConsSitesLost = false;
            }

            if (!NA)
            {
                setTfbsConsSitesHT();
            }

            foreach (string feaName in insFeatureName.tfbsConsSitesFeatureName)
            {
                if (NA)             // 不可计算直接打印 NA
                {
                    rTxtBoxResult.Text += "NA\t";
                }
                else
                {
                    rTxtBoxResult.Text += string.Format("{0:F5}", (int)tfbsConsSitesHT[feaName] * 10 / (double)TITLELENGTH) + "\t";     // 打印结果
                }
            }
        }

        // 函数功能：返回每条 chr*_rmsk.txt 记录的正负链情况
        // 被 rmskCompute 函数调用
        private bool getIsRmskPositive(string record)
        {
            string[] splitStr = record.Split('\t');
            if (splitStr[9] == "+")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 函数功能：返回每条 chr*_rmsk.txt 记录的 repName
        // 被 rmskCompute 函数调用
        private string getRmskRepName(string record)
        {
            string[] splitStr = record.Split('\t');
            return splitStr[10];
        }

        // 函数功能：返回每条 chr*_rmsk.txt 记录的起始位点位置
        // 被 rmskCompute 函数调用
        private int[] getRmskStartEnd(string record)
        {
            int[] StartEnd = new int[2];
            string[] splitStr = record.Split('\t');
            StartEnd[0] = Convert.ToInt32(splitStr[6]);
            StartEnd[1] = Convert.ToInt32(splitStr[7]);
            return StartEnd;
        }

        // 函数功能：填充 userRmskHT 哈希表
        // 被 userRmskCompute 函数调用
        private void setUserRmskHT(string pathOfFeature)
        {
            bool isPositive = false;
            int[] RecordStartEnd = new int[2];
            int recordStart, recordEnd;
            string record;

            StreamReader srFea = new StreamReader(pathOfFeature, Encoding.Default);

            while (srFea.Peek() != -1)
            {
                record = srFea.ReadLine().Trim();
                RecordStartEnd = getRmskStartEnd(record);
                recordStart = RecordStartEnd[0];
                recordEnd = RecordStartEnd[1];
                isPositive = getIsRmskPositive(record);

                if (recordStart >= TITLESTART && recordEnd <= TITLEEND && isPositive)
                {
                    if (userRmskHT.Contains(getRmskRepName(record)))
                    {
                        userRmskHT[getRmskRepName(record)] = (int)userRmskHT[getRmskRepName(record)] + 1;
                    }
                }
                if (recordEnd > TITLEEND)
                {
                    break;
                }
            }
            srFea.Close();
        }

        // 函数功能：填充 rmskHT 哈希表
        // 被 rmskCompute 函数调用
        private void setRmskHT(string pathOfFeature)
        {
            bool isPositive = false;
            int[] RecordStartEnd = new int[2];
            int recordStart, recordEnd;
            string record;

            StreamReader srFea = new StreamReader(pathOfFeature, Encoding.Default);

            while (srFea.Peek() != -1)
            {
                record = srFea.ReadLine().Trim();
                RecordStartEnd = getRmskStartEnd(record);
                recordStart = RecordStartEnd[0];
                recordEnd = RecordStartEnd[1];
                isPositive = getIsRmskPositive(record);

                if (recordStart >= TITLESTART && recordEnd <= TITLEEND && isPositive)
                {
                    if (rmskHT.Contains(getRmskRepName(record)))
                    {
                        rmskHT[getRmskRepName(record)] = (int)rmskHT[getRmskRepName(record)] + 1;
                    }
                }
                if (recordEnd > TITLEEND)
                {
                    break;
                }
            }
            srFea.Close();
        }

        // 函数功能：计算用户提供的 rmsk 特征
        // 被 StartToCompute 函数调用
        private void userRmskCompute()
        {
            bool isFirstLost = true;
            string pathOfFeature;

            pathOfFeature = featurePath + @"\rmsk\" + TITLECHR + "_rmsk.txt";

            // 文件丢失则置不可计算标志位为 true
            if (!File.Exists(pathOfFeature) && isFirstLost)
            {
                MessageBox.Show(TITLECHR + "_rmsk.txt" + " 文件丢失！\n请确保该文件存在于 feature 目录的 rmsk 子目录中！\n计算线程将终止", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NA = true;
                isFirstLost = false;
            }

            if (!NA)
            {
                setUserRmskHT(pathOfFeature);
            }

            foreach (string feaName in userRmskFeatureList)
            {
                if (NA)
                {
                    rTxtBoxResult.Text += "NA\t";
                }
                else
                {
                    rTxtBoxResult.Text += string.Format("{0:F5}", (int)userRmskHT[feaName] * 10 / (double)TITLELENGTH) + "\t";
                }
            }
        }

        // 函数功能：rmsk 特征的提取
        // 当相应多选框被选中时被 StartToCompute 函数调用
        private void rmskCompute()
        {
            bool isFirstLost = true;
            string pathOfFeature;

            pathOfFeature = featurePath + @"\rmsk\" + TITLECHR + "_rmsk.txt";

            // 文件丢失则置不可计算标志位为 true
            if (!File.Exists(pathOfFeature) && isFirstLost)
            {
                MessageBox.Show(TITLECHR + "_rmsk.txt" + " 文件丢失！\n请确保该文件存在于 feature 目录的 rmsk 子目录中！\n计算线程将终止", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NA = true;
                isFirstLost = false;
            }

            if (!NA)
            {
                setRmskHT(pathOfFeature);
            }

            foreach (string feaName in insFeatureName.rmskFeatureName)
            {
                if (NA)
                {
                    rTxtBoxResult.Text += "NA\t";
                }
                else
                {
                    rTxtBoxResult.Text += string.Format("{0:F5}", (int)rmskHT[feaName] * 10 / (double)TITLELENGTH) + "\t";
                }
            }
        }

        // 函数功能：返回 *.vstep 文件中每行记录的个数信息
        //  被 histoneModificationCompute 函数调用
        private int getHistoneModificationCount(string record)
        {
            string[] splitStr = record.Split('\t');
            return Convert.ToInt32(splitStr[1]);
        }

        // 函数功能：返回 *.vstep 文件中每行记录的起始位点位置
        // 被 histoneModificationCompute 函数调用
        private int[] getHistoneModificationStartEnd(string record)
        {
            int[] StartEnd = new int[2];
            string[] splitStr = record.Split('\t');
            StartEnd[0] = Convert.ToInt32(splitStr[0]);
            StartEnd[1] = StartEnd[0] + 200;
            return StartEnd;
        }

        // 函数功能：计算用户提供的 histone modification 特征
        // 被 StartToCompute 函数调用
        private void userHistoneModificationCompute()
        {
            int lineIndex;
            int count;
            int[] RecordStartEnd = new int[2];
            int recordStart, recordEnd;
            string record;
            string pathOfFeature;

            foreach (string feaName in userHistoneModification18NatFeatureList)
            {
                if (NA)
                {
                    rTxtBoxResult.Text += feaName + "\tNA\n";
                }
                else
                {
                    pathOfFeature = featurePath + @"histone modification\2008-nat ge-Combinatorial patterns of histone\CD4-" + feaName + @"-summary.vstep";

                    if (!File.Exists(pathOfFeature))
                    {
                        MessageBox.Show("CD4-" + feaName + "-summary.vstep" + " 文件丢失！\n请确保该文件存在于 histone modification 目录的 2008-nat ge-Combinatorial patterns of histone 子目录中！\n计算线程将终止", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        NA = true;
                    }

                    StreamReader srFea = new StreamReader(pathOfFeature, Encoding.Default);
                    count = 0;
                    lineIndex = 0;
                    while (srFea.Peek() != -1)
                    {
                        record = srFea.ReadLine().Trim();
                        ++lineIndex;
                        if (lineIndex <= (int)((Hashtable)insChrLineIndex.HistoneModification18NatFeatureNameHT[feaName])[TITLECHR])
                        {
                            continue;
                        }

                        RecordStartEnd = getHistoneModificationStartEnd(record);
                        recordStart = RecordStartEnd[0];
                        recordEnd = RecordStartEnd[1];

                        if (recordStart >= TITLESTART && recordEnd <= TITLEEND)
                        {
                            count += getHistoneModificationCount(record);
                        }
                        if (recordEnd > TITLEEND)
                        {
                            break;
                        }
                    }
                    srFea.Close();
                    rTxtBoxResult.Text += string.Format("{0:F5}", (double)count * 10 / TITLELENGTH) + "\t";
                }
            }

            foreach (string feaName in userHistoneModification20CellFeatureList)
            {
                if (NA)
                {
                    rTxtBoxResult.Text += feaName + "\tNA\n";
                }
                else
                {
                    pathOfFeature = featurePath + @"histone modification\2008-cell-High-resolution profiling of histone\" + feaName + @".vstep";

                    if (!File.Exists(pathOfFeature))
                    {
                        MessageBox.Show(feaName + ".vstep" + " 文件丢失！\n请确保该文件存在于 histone modification 目录的 2008-cell-High-resolution profiling of histone 子目录中！\n计算线程将终止", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        NA = true;
                    }

                    StreamReader srFea = new StreamReader(pathOfFeature, Encoding.Default);
                    count = 0;
                    lineIndex = 0;
                    while (srFea.Peek() != -1)
                    {
                        record = srFea.ReadLine().Trim();
                        ++lineIndex;
                        if (lineIndex <= (int)((Hashtable)insChrLineIndex.HistoneModification20CellFeatureNameHT[feaName])[TITLECHR])
                        {
                            continue;
                        }

                        RecordStartEnd = getHistoneModificationStartEnd(record);
                        recordStart = RecordStartEnd[0];
                        recordEnd = RecordStartEnd[1];

                        if (recordStart >= TITLESTART && recordEnd <= TITLEEND)
                        {
                            count += getHistoneModificationCount(record);
                        }
                        if (recordEnd > TITLEEND)
                        {
                            break;
                        }
                    }
                    srFea.Close();
                    rTxtBoxResult.Text += string.Format("{0:F5}", (double)count * 10 / TITLELENGTH) + "\t";
                }
            }
        }

        // 函数功能：histone modification 特征的提取
        // 当相应多选框被选中时被 StartToCompute 函数调用
        private void histoneModificationCompute()
        {
            int lineIndex;
            int count;
            int[] RecordStartEnd = new int[2];
            int recordStart, recordEnd;
            string record;
            string pathOfFeature;

            foreach (string feaName in insFeatureName.histoneModification18NatFeatureName)
            {
                if (NA)
                {
                    rTxtBoxResult.Text += feaName + "\tNA\n";
                }
                else
                {
                    pathOfFeature = featurePath + @"histone modification\2008-nat ge-Combinatorial patterns of histone\CD4-" + feaName + @"-summary.vstep";

                    if (!File.Exists(pathOfFeature))
                    {
                        MessageBox.Show("CD4-" + feaName + "-summary.vstep" + " 文件丢失！\n请确保该文件存在于 histone modification 目录的 2008-nat ge-Combinatorial patterns of histone 子目录中！\n计算线程将终止", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        NA = true;
                    }

                    StreamReader srFea = new StreamReader(pathOfFeature, Encoding.Default);
                    count = 0;
                    lineIndex = 0;
                    while (srFea.Peek() != -1)
                    {
                        record = srFea.ReadLine().Trim();
                        ++lineIndex;
                        if (lineIndex <= (int)((Hashtable)insChrLineIndex.HistoneModification18NatFeatureNameHT[feaName])[TITLECHR])
                        {
                            continue;
                        }

                        RecordStartEnd = getHistoneModificationStartEnd(record);
                        recordStart = RecordStartEnd[0];
                        recordEnd = RecordStartEnd[1];

                        if (recordStart >= TITLESTART && recordEnd <= TITLEEND)
                        {
                            count += getHistoneModificationCount(record);
                        }
                        if (recordEnd > TITLEEND)
                        {
                            break;
                        }
                    }
                    srFea.Close();
                    rTxtBoxResult.Text += string.Format("{0:F5}", (double)count * 10 / TITLELENGTH) + "\t";
                }
            }

            foreach (string feaName in insFeatureName.histoneModification20CellFeatureName)
            {
                if (NA)
                {
                    rTxtBoxResult.Text += feaName + "\tNA\n";
                }
                else
                {
                    pathOfFeature = featurePath + @"histone modification\2008-cell-High-resolution profiling of histone\" + feaName + @".vstep";

                    if (!File.Exists(pathOfFeature))
                    {
                        MessageBox.Show(feaName + ".vstep" + " 文件丢失！\n请确保该文件存在于 histone modification 目录的 2008-cell-High-resolution profiling of histone 子目录中！\n计算线程将终止", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        NA = true;
                    }

                    StreamReader srFea = new StreamReader(pathOfFeature, Encoding.Default);
                    count = 0;
                    lineIndex = 0;
                    while (srFea.Peek() != -1)
                    {
                        record = srFea.ReadLine().Trim();
                        ++lineIndex;
                        if (lineIndex <= (int)((Hashtable)insChrLineIndex.HistoneModification20CellFeatureNameHT[feaName])[TITLECHR])
                        {
                            continue;
                        }

                        RecordStartEnd = getHistoneModificationStartEnd(record);
                        recordStart = RecordStartEnd[0];
                        recordEnd = RecordStartEnd[1];

                        if (recordStart >= TITLESTART && recordEnd <= TITLEEND)
                        {
                            count += getHistoneModificationCount(record);
                        }
                        if (recordEnd > TITLEEND)
                        {
                            break;
                        }
                    }
                    srFea.Close();
                    rTxtBoxResult.Text += string.Format("{0:F5}", (double)count * 10 / TITLELENGTH) + "\t";
                }
            }
        }

        // 函数功能：计算用户提供的 motif 特征
        // 被 StartToCompute 函数调用
        private void userMotifCompute()
        {
            double rate;
            int count;
            int countC = 0;
            int countG = 0;
            int countCG = 0;
            string FEATURE;

            if (userMotifFeatureList.Contains("%G+C"))
            {
                countC = SubstringCount(SEQUENCE, "C");
                countG = SubstringCount(SEQUENCE, "G");
            }

            if (userMotifFeatureList.Contains("CpG ratio"))
            {
                countCG = SubstringCount(SEQUENCE, "CG");
            }

            foreach (string motifFeature in userMotifFeatureList)
            {
                FEATURE = motifFeature;

                if (FEATURE == "%G+C")
                {
                    rate = (double)(countC + countG) / (double)TITLELENGTH;
                    rTxtBoxResult.Text += rate.ToString("F3") + "\t";
                }
                else if (FEATURE == "CpG ratio")
                {

                    rate = TITLELENGTH * (double)(countCG) / (double)(countC * countG);
                    rTxtBoxResult.Text += rate.ToString("F3") + "\t";
                }
                else
                {
                    bool needRegex = false;
                    foreach (string iupac in insFeatureName.iupac)
                    {
                        if (FEATURE.Contains(iupac))
                        {
                            needRegex = true;
                            FEATURE = FEATURE.Replace(iupac, (string)IUPACHT[iupac]);
                        }
                    }

                    if (needRegex)
                    {
                        count = SubstringCount(SEQUENCE, FEATURE);      // 计算特征出现的次数
                    }
                    else
                    {
                        count = Count(SEQUENCE, FEATURE);
                    }

                    rTxtBoxResult.Text += string.Format("{0:F5}", (double)count / TITLELENGTH * 10) + "\t";
                }
            }
        }

        // 函数功能：计算 motif 特征
        // 被 StartToCompute 函数调用
        private void motifCompute()
        {
            double rate;
            int count;
            int countC;
            int countG;
            int countCG;
            string FEATURE;
            foreach (string feaName in insFeatureName.motifFeatureName)
            {
                FEATURE = feaName;
                countC = Count(SEQUENCE, "C");
                countG = Count(SEQUENCE, "G");
                countCG = Count(SEQUENCE, "CG");

                if (FEATURE == "%G+C")
                {
                    rate = (double)(countC + countG) / (double)TITLELENGTH;
                    rTxtBoxResult.Text += rate.ToString("F5") + "\t";
                }
                else if (FEATURE == "CpG ratio")
                {

                    rate = TITLELENGTH * (double)(countCG) / (double)(countC * countG);
                    rTxtBoxResult.Text += rate.ToString("F5") + "\t";
                }
                else
                {
                    count = Count(SEQUENCE, FEATURE);
                    rTxtBoxResult.Text += string.Format("{0:F5}", (double)count / TITLELENGTH * 10) + "\t";
                }
            }

            foreach (string feaName6 in insFeatureName.motifFeatureName6)
            {
                string tempFeature = feaName6;
                foreach (string iupa in insFeatureName.iupac)
                {
                    tempFeature = tempFeature.Replace(iupa, IUPACHT[iupa].ToString());
                }

                count = SubstringCount(SEQUENCE, tempFeature);
                rTxtBoxResult.Text += string.Format("{0:F5}", Convert.ToDouble((double)count / TITLELENGTH * 10)) + "\t";
            }

            //Hashtable feaName6ExpandedCount = new Hashtable();
            //foreach (string feaName6Expanded in insFeatureName.motifFeatureName6Expanded)
            //{
            //    if (!feaName6ExpandedCount.Contains(feaName6Expanded))
            //    {
            //        feaName6ExpandedCount.Add(feaName6Expanded, 0);
            //    }
            //}

            //foreach (string feaName6Expanded in insFeatureName.motifFeatureName6Expanded)
            //{
            //    count = Count(SEQUENCE, feaName6Expanded);
            //    feaName6ExpandedCount[feaName6Expanded] = count;
            //}

            //Hashtable feaName6Count = new Hashtable();
            //foreach (string feaName6 in insFeatureName.motifFeatureName6)
            //{
            //    feaName6Count.Add(feaName6, 0);
            //    foreach (string feaName6Expanded in (List<string>)insFeatureName.motifFeatureName6HT[feaName6])
            //    {
            //        feaName6Count[feaName6] = Convert.ToInt32(feaName6Count[feaName6]) + Convert.ToInt32(feaName6ExpandedCount[feaName6Expanded]);
            //    }
            //    rTxtBoxResult.Text += string.Format("{0:F5}", Convert.ToDouble(feaName6Count[feaName6]) / TITLELENGTH * 10) + "\t";
            //}
        }

        // 函数功能：在文本框和文件中提取出序列和特征
        // 通过调用 SubstringCount 函数实现信息的统计
        // 被 btnStart_Click 函数调用，用以创建计算线程
        private void StartToCompute()
        {
            // 下面是从文件中读取的序列，并与文件中的特征比对
            StreamReader srSeq = new StreamReader(seqFileName, Encoding.Default);
            StreamReader srTitle = new StreamReader(titleFileName, Encoding.Default);

            while (srSeq.Peek() != -1 && srTitle.Peek() != -1)
            {
                SEQUENCE = srSeq.ReadLine().Trim(); // 在 seqFileName(只保存了纯序列的文件)中读出一行，赋值给全局变量 SEQUENCE

                TITLE = srTitle.ReadLine().Trim();  // 在 titleFileName(只保存了纯头部的文件)中读出一行，赋值给全局变量 TITLE
                  
                setNameChrLengthStartEnd();         // 对全局 TITLENAME、TITLESTART、TITLEEND、TITLECHR 全局变量设定初始值，方便后面的计算线程传递参数

                if (SEQUENCE == string.Empty || TITLE == string.Empty)
                {
                    continue;
                }

                if (cBoxMod1.Checked)
                {
                    trdMotifCompute = new Thread(motifCompute);
                    trdMotifCompute.IsBackground = true;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    isMotifComputeThreadCreated = true;
                    trdMotifCompute.Start();
                    this.Text = "motif 特征计算中";
                    trdMotifCompute.Join();

                }

                if (userMotifFeatureList.Count > 0)
                {
                    trdUserMotifCompute = new Thread(userMotifCompute);
                    trdUserMotifCompute.IsBackground = true;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    isUserMotifComputeThreadCreated = true;
                    trdUserMotifCompute.Start();
                    this.Text = "motif 特征计算中";
                    trdUserMotifCompute.Join();
                }

                if (cBoxMod2.Checked)     // 需要计算这个特征，并且特征文件没有丢失
                {
                    // 初始化结果保存哈希表，否则结果会累加
                    foreach (string feaName in insFeatureName.tfbsConsSitesFeatureName)
                    {
                        tfbsConsSitesHT[feaName] = 0;
                    }

                    // 创建计算线程，开始计算
                    trdTfbsConsSitesCompute = new Thread(tfbsConsSitesCompute);
                    trdTfbsConsSitesCompute.IsBackground = true;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    isTfbsConsSitesComputeThreadCreated = true;
                    trdTfbsConsSitesCompute.Start();
                    this.Text = "tfbsConsSites 特征计算中";
                    trdTfbsConsSitesCompute.Join();
                }

                if (userTfbsConsSitesFeatureList.Count > 0)
                {
                    foreach (string feaName in userTfbsConsSitesFeatureList)
                    {
                        userTfbsConsSitesHT[feaName] = 0;
                    }

                    trdUserTfbsConsSitesCompute = new Thread(userTfbsConsSitesCompute);
                    trdUserTfbsConsSitesCompute.IsBackground = true;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    isUserTfbsConsSitesComputeThreadCreated = true;
                    trdUserTfbsConsSitesCompute.Start();
                    this.Text = "tfbsConsSites 特征计算中";
                    trdUserTfbsConsSitesCompute.Join();
                }

                if (cBoxMod3.Checked)
                {
                    // 初始化结果保存哈希表，否则结果会累加
                    foreach (string feaName in insFeatureName.rmskFeatureName)
                    {
                        rmskHT[feaName] = 0;
                    }

                    trdRmskCompute = new Thread(rmskCompute);
                    trdRmskCompute.IsBackground = true;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    isRmskComputeThreadCreated = true;
                    trdRmskCompute.Start();
                    this.Text = "rmsk 特征计算中";
                    trdRmskCompute.Join();
                }

                if (userRmskFeatureList.Count > 0)
                {
                    foreach (string feaName in userRmskFeatureList)
                    {
                        userRmskHT[feaName] = 0;
                    }

                    trdUserRmskCompute = new Thread(userRmskCompute);
                    trdUserRmskCompute.IsBackground = true;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    isUserRmskComputeThreadCreated = true;
                    trdUserRmskCompute.Start();
                    this.Text = "rmsk 特征计算中";
                    trdUserRmskCompute.Join();
                }

                if (cBoxMod4.Checked)
                {
                    trdHistoneModificationCompute = new Thread(histoneModificationCompute);
                    trdHistoneModificationCompute.IsBackground = true;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    isHistoneModificationComputeThreadCreated = true;
                    trdHistoneModificationCompute.Start();
                    this.Text = "histone modification 特征计算中";
                    trdHistoneModificationCompute.Join();
                }

                if (userHistoneModification18NatFeatureList.Count > 0 || userHistoneModification20CellFeatureList.Count > 0)
                {
                    trdUserHistoneModificationCompute = new Thread(userHistoneModificationCompute);
                    trdUserHistoneModificationCompute.IsBackground = true;
                    Control.CheckForIllegalCrossThreadCalls = false;
                    isUserHistoneModificationComputeThreadCreated = true;
                    trdUserHistoneModificationCompute.Start();
                    this.Text = "histone modification 特征计算中";
                    trdUserHistoneModificationCompute.Join();
                }

                rTxtBoxResult.Text += "\n";
            }
            srSeq.Close();
            srTitle.Close();
        }

        // 函数功能：对序列文件进行预处理
        // 把序列文件分为头 title 和序列本身 seq，两个指针同时向下读，方便进行处理
        // 处理后的文件存放在程序所在根目录下的 temp 文件夹
        // 被 btnStart_Click 事件调用
        // 此函数是本程序逻辑最容易发生错乱的地方，也是程序最耗时的地方
        private void Pretreatment()
        {
            bool seqNeedBlat = false;       // 以下的序列是否需要 blat
            bool titleNeedBlat = false;     // 头部是否需要 blat
            bool needBlat = false;          // 所有的序列是否需要用 bl需要 blat。只要有一条需要，则置为真

            bool isFirstSeq = true;         // 是不是第一条序列
            bool isTitle = true;            // 是不是头部
            bool isLastTitle = false;       // 记录当前的序列出现是上一次出现的是否是头部
            bool isFirstBlatTitle = true;   // 是否是第一个需要 blat 的头部（设置该标志位是为了防止整合后的需要 blat 的整体文件不是顶行的而导致用 blat 处理时出现问题）
            string record;
            string feature;

            StreamWriter swTitle = new StreamWriter(titleFileName, true);           // 记录序列头部的文件写入流
            StreamWriter swSeq = new StreamWriter(seqFileName, true);               // 记录序列本身的文件写入流
            StreamWriter swEntire = new StreamWriter(entireFileName, true);         // 记录需要 blat 的整体文件的写入流

            // 对序列文件进行处理，特征文件没必要进行处理
            if (seqFileList.Count > 0)
            {
                foreach (string obj in seqFileList)
                {
                    StreamReader srSeq = new StreamReader(obj, Encoding.Default);       // 依次读已添加的序列文件

                    while (srSeq.Peek() != -1)
                    {
                        record = srSeq.ReadLine().Trim();
                        if (record == string.Empty)
                        {
                            continue;
                        }

                        if (record[0] == '>')
                        {
                            isTitle = true;         // 第一个是 ">"，则是头部
                            titleNeedBlat = false;  // 预置该头部需要 blat 的标志位为否
                            string[] splitStr = record.Split('\t');
                            if (splitStr.Length < 5)        // 切割后的长度如果小于 5，则需要 blat
                            {
                                // 加入需要使用 blat 的 arraylist 中
                                if (splitStr.Length >= 2)
                                {
                                    if (!blatNameChrHT.Contains(splitStr[0]))       // 作这一步判断的原因是用户提供的序列很可能有重复，以下相同
                                    {
                                        blatNameChrHT.Add(splitStr[0], splitStr[1]);
                                    }
                                }
                                else
                                {
                                    if (!blatNameChrHT.Contains(splitStr[0]))
                                    {
                                        blatNameChrHT.Add(splitStr[0], string.Empty);
                                    }
                                }

                                blatTitleNameList.Add(splitStr[0]);                 // 记录下需要 blat 的序列的名字是为了后面填充哈希表时使用
                                needBlat = true;
                                titleNeedBlat = true;
                            }
                        }
                        else
                        {
                            isTitle = false;
                            seqNeedBlat = false;
                            if (titleNeedBlat)
                            {
                                seqNeedBlat = true;
                            }
                        }

                        if (isTitle)
                        {
                            if (titleNeedBlat)
                            {
                                if (isFirstBlatTitle)
                                {
                                    // 如果是头部且需要 blat 且是第一个，则直接写入相应文件，同时置是第一个头部的标志位为否
                                    swEntire.WriteLine(record);
                                    isFirstBlatTitle = false;
                                }
                                else
                                {
                                    // 否则要多加入一个换行符
                                    swEntire.WriteLine("\n" + record);
                                }
                            }
                            swTitle.WriteLine(record);
                            isLastTitle = true;
                        }
                        else
                        {
                            if (isLastTitle && !isFirstSeq)
                            {
                                swSeq.Write("\n" + record);
                            }
                            else
                            {
                                swSeq.Write(record);
                            }
                            if (seqNeedBlat)
                            {
                                swEntire.Write(record);
                            }
                            isFirstSeq = false;
                            isLastTitle = false;
                        }
                    }
                    srSeq.Close();
                }
            }

            // 对文本框中提供的序列进行处理
            if (rTxtBoxSequence.Lines.Length > 0)
            {
                foreach (string txtSequence in rTxtBoxSequence.Lines)
                {
                    record = txtSequence.Trim();
                    if (record == string.Empty)
                    {
                        continue;
                    }

                    if (record[0] == '>')
                    {
                        isTitle = true;
                        titleNeedBlat = false;
                        string[] splitStr = record.Split('\t');
                        if (splitStr.Length < 5)
                        {
                            // 加入需要使用 blat 的 arraylist 中
                            if (splitStr.Length >= 2)
                            {
                                if (!blatNameChrHT.Contains(splitStr[0]))
                                {
                                    blatNameChrHT.Add(splitStr[0], splitStr[1]);
                                }
                            }
                            else
                            {
                                if (!blatNameChrHT.Contains(splitStr[0]))
                                {
                                    blatNameChrHT.Add(splitStr[0], string.Empty);
                                }
                            }
                            blatTitleNameList.Add(splitStr[0]);
                            needBlat = true;
                            titleNeedBlat = true;
                        }
                    }
                    else
                    {
                        isTitle = false;
                        seqNeedBlat = false;
                        if (titleNeedBlat)
                        {
                            seqNeedBlat = true;
                        }
                    }

                    if (isTitle)
                    {
                        if (titleNeedBlat)
                        {
                            if (isFirstBlatTitle)
                            {
                                swEntire.WriteLine(record);
                                isFirstBlatTitle = false;
                            }
                            else
                            {
                                swEntire.WriteLine("\n" + record);
                            }
                        }

                        swTitle.WriteLine(record);
                        isLastTitle = true;
                    }
                    else
                    {
                        if (isLastTitle && !isFirstSeq)
                        {
                            swSeq.Write("\n" + record);
                        }
                        else
                        {
                            swSeq.Write(record);
                        }
                        if (seqNeedBlat)
                        {
                            swEntire.Write(record);
                        }
                        isFirstSeq = false;
                        isLastTitle = false;
                    }
                }
            }

            swTitle.Write("\n");
            swTitle.Flush();
            swTitle.Close();

            swSeq.Write("\n");
            swSeq.Flush();
            swSeq.Close();

            // 在所有需要使用 blat 的序列文件后面追加一个换行符
            // 否则使用 blat 时会出错
            swEntire.Write("\n");
            swEntire.Flush();
            swEntire.Close();

            // 把文件中的特征全部提取到一个 ArrayList 中
            if (feaFileList.Count > 0)
            {
                foreach (string obj in feaFileList)
                {
                    StreamReader srFea = new StreamReader(obj, Encoding.Default);

                    while (srFea.Peek() != -1)
                    {
                        feature = srFea.ReadLine().Trim();
                        if (feature == string.Empty)
                        {
                            continue;
                        }

                        // 如果与用户之前输入的没有重复
                        if (!userUpperFeatureList.Contains(feature.ToUpper()))
                        {
                            userUpperFeatureList.Add(feature.ToUpper());

                            // 如果 tfbsConsSites 特征中已包含这个特征
                            if (insFeatureName.tfbsConsSitesFeatureName.Contains(feature))
                            {
                                // 如果第二个复选框没有被选中
                                if (!cBoxMod2.Checked)
                                {
                                    // 加入到用户提供的 fbsConsSites 序列中
                                    userTfbsConsSitesFeatureList.Add(feature);
                                }
                                else
                                {
                                    // 如果被选中，则必然是重复的
                                    userRepeatFeatureList.Add(feature);
                                }

                                // 后面的比较不需要做了，直接跳过
                                continue;
                            }

                            if (insFeatureName.rmskFeatureName.Contains(feature))
                            {
                                if (!cBoxMod3.Checked)
                                {
                                    userRmskFeatureList.Add(feature);
                                }
                                else
                                {
                                    userRepeatFeatureList.Add(feature);
                                }

                                continue;
                            }

                            if (insFeatureName.histoneModification18NatFeatureName.Contains(feature))
                            {
                                if (!cBoxMod4.Checked)
                                {
                                    userHistoneModification18NatFeatureList.Add(feature);
                                }
                                else
                                {
                                    userRepeatFeatureList.Add(feature);
                                }

                                continue;
                            }

                            if (insFeatureName.histoneModification20CellFeatureName.Contains(feature))
                            {
                                if (!cBoxMod4.Checked)
                                {
                                    userHistoneModification20CellFeatureList.Add(feature);
                                }
                                else
                                {
                                    userRepeatFeatureList.Add(feature);
                                }

                                continue;
                            }

                            // 剩下来的全部归入 motif 特征
                            if (insFeatureName.motifFeatureName.Contains(feature, StringComparer.OrdinalIgnoreCase))
                            {
                                // 如果默认的已经有了，但复选框 1 没有选中
                                if (!cBoxMod1.Checked)
                                {
                                    // 加入到用户的 motif 特征中去
                                    userMotifFeatureList.Add(feature);
                                }
                                else
                                {
                                    // 否则直接加入到重复序列中去
                                    userRepeatFeatureList.Add(feature);
                                }

                                continue;
                            }

                            // 是以上所有情况之外的，直接加入到用户自定义的 motif 特征中去
                            userMotifFeatureList.Add(feature);
                        }
                        else
                        {
                            // 如果与用户之前输入的有冲突，直接放入重复序列中去
                            userRepeatFeatureList.Add(feature);
                        }
                    }
                    srFea.Close();
                }
            }

            // 把文本框中的特征全部提取到一个 ArrayList 中
            if (rTxtBoxFeature.Lines.Length > 0)
            {
                foreach (string txtFeature in rTxtBoxFeature.Lines)
                {
                    feature = txtFeature.Trim();
                    if (feature == string.Empty)
                    {
                        continue;
                    }

                    // 如果与用户之前输入的没有重复
                    if (!userUpperFeatureList.Contains(feature.ToUpper()))
                    {
                        userUpperFeatureList.Add(feature.ToUpper());


                        // 如果 tfbsConsSites 特征中已包含这个特征
                        if (insFeatureName.tfbsConsSitesFeatureName.Contains(feature))
                        {
                            // 如果第二个复选框没有被选中
                            if (!cBoxMod2.Checked)
                            {
                                // 加入到用户提供的 fbsConsSites 序列中
                                userTfbsConsSitesFeatureList.Add(feature);
                            }
                            else
                            {
                                // 如果被选中，则必然是重复的
                                userRepeatFeatureList.Add(feature);
                            }

                            // 后面的比较不需要做了，直接跳过
                            continue;
                        }

                        if (insFeatureName.rmskFeatureName.Contains(feature))
                        {
                            if (!cBoxMod3.Checked)
                            {
                                userRmskFeatureList.Add(feature);
                            }
                            else
                            {
                                userRepeatFeatureList.Add(feature);
                            }

                            continue;
                        }

                        if (insFeatureName.histoneModification18NatFeatureName.Contains(feature))
                        {
                            if (!cBoxMod4.Checked)
                            {
                                userHistoneModification18NatFeatureList.Add(feature);
                            }
                            else
                            {
                                userRepeatFeatureList.Add(feature);
                            }

                            continue;
                        }

                        if (insFeatureName.histoneModification20CellFeatureName.Contains(feature))
                        {
                            if (!cBoxMod4.Checked)
                            {
                                userHistoneModification20CellFeatureList.Add(feature);
                            }
                            else
                            {
                                userRepeatFeatureList.Add(feature);
                            }

                            continue;
                        }

                        // 剩下来的全部归入 motif 特征
                        if (insFeatureName.motifFeatureName.Contains(feature, StringComparer.OrdinalIgnoreCase))
                        {
                            // 如果默认的已经有了，但复选框 1 没有选中
                            if (!cBoxMod1.Checked)
                            {
                                // 加入到用户的 motif 特征中去
                                userMotifFeatureList.Add(feature);
                            }
                            else
                            {
                                // 否则直接加入到重复序列中去
                                userRepeatFeatureList.Add(feature);
                            }

                            continue;
                        }

                        // 是以上所有情况之外的，直接加入到用户自定义的 motif 特征中去
                        userMotifFeatureList.Add(feature);
                    }
                    else
                    {
                        // 如果与用户之前输入的有冲突，直接放入重复序列中去
                        userRepeatFeatureList.Add(feature);
                    }
                }
            }

            int index = 0;
            if (cBoxMod1.Checked)
            {
                foreach (string feaToWrite in insFeatureName.motifFeatureName)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
                foreach (string feaToWrite in insFeatureName.motifFeatureName6)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }

            if (userMotifFeatureList.Count > 0)
            {
                foreach (string feaToWrite in userMotifFeatureList)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }

            if (cBoxMod2.Checked)
            {
                foreach (string feaToWrite in insFeatureName.tfbsConsSitesFeatureName)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }

            if (userTfbsConsSitesFeatureList.Count > 0)
            {
                foreach (string feaToWrite in userTfbsConsSitesFeatureList)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }

            if (cBoxMod3.Checked)
            {
                foreach (string feaToWrite in insFeatureName.rmskFeatureName)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }

            if (userRmskFeatureList.Count > 0)
            {
                foreach(string feaToWrite in userRmskFeatureList)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }

            if (cBoxMod4.Checked)
            {
                foreach (string feaToWrite in insFeatureName.histoneModification18NatFeatureName)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }

                foreach (string feaToWrite in insFeatureName.histoneModification20CellFeatureName)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }

            if (userHistoneModification18NatFeatureList.Count > 0)
            {
                foreach(string feaToWrite in userHistoneModification18NatFeatureList)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }

            if (userHistoneModification20CellFeatureList.Count > 0)
            {
                foreach (string feaToWrite in userHistoneModification20CellFeatureList)
                {
                    ++index;
                    rTxtBoxResult.Text += index.ToString().PadRight(7) + "\t";
                    rTxtBoxFeatureName.Text += index.ToString().PadRight(feaToWrite.Length) + "\t";
                }
            }
            rTxtBoxFeatureName.Text += "\n";
            rTxtBoxResult.Text += "\n";

            if (cBoxMod1.Checked)
            {
                foreach (string feaToWrite in insFeatureName.motifFeatureName)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
                foreach (string feaToWrite in insFeatureName.motifFeatureName6)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if (userMotifFeatureList.Count > 0)
            {
                foreach (string feaToWrite in userMotifFeatureList)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if (cBoxMod2.Checked)
            {
                foreach (string feaToWrite in insFeatureName.tfbsConsSitesFeatureName)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if (userTfbsConsSitesFeatureList.Count > 0)
            {
                foreach (string feaToWrite in userTfbsConsSitesFeatureList)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if (cBoxMod3.Checked)
            {
                foreach (string feaToWrite in insFeatureName.rmskFeatureName)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if (userRmskFeatureList.Count > 0)
            {
                foreach (string feaToWrite in userRmskFeatureList)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if (cBoxMod4.Checked)
            {
                foreach (string feaToWrite in insFeatureName.histoneModification18NatFeatureName)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }

                foreach (string feaToWrite in insFeatureName.histoneModification20CellFeatureName)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if (userHistoneModification18NatFeatureList.Count > 0)
            {
                foreach (string feaToWrite in userHistoneModification18NatFeatureList)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if (userHistoneModification20CellFeatureList.Count > 0)
            {
                foreach (string feaToWrite in userHistoneModification20CellFeatureList)
                {
                    rTxtBoxFeatureName.Text += feaToWrite + "\t";
                }
            }

            if ((cBoxMod2.Checked || cBoxMod3.Checked || cBoxMod4.Checked || userTfbsConsSitesFeatureList.Count > 0 || userRmskFeatureList.Count > 0 || userHistoneModification18NatFeatureList.Count > 0 || userHistoneModification20CellFeatureList.Count > 0) && needBlat)
            {
                // 调用 blat 处理 entireFile，得到四个文件，合并之后留做后用
                if (!Directory.Exists(blatPath))
                {
                    MessageBox.Show("blat 可执行文件丢失！\n请确保该文件存在于 blat 目录中！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 执行 blat 命令，得到索引文件 tempOut*.psl
                int i;
                Process blat = new Process();
                blat.StartInfo.FileName = pathOfBlat;

                for (i = 1; i <= 4; ++i)
                {
                    string args = @"hg18_" + i.ToString() + @".2bit entireFile.fa -noHead -ooc=11.ooc temp\tempOut" + i.ToString() + ".psl";   // 启动参数
                    blat.StartInfo.Arguments = args;
                    blat.StartInfo.CreateNoWindow = true;
                    blat.StartInfo.UseShellExecute = false;
                    blat.Start();                           // 启动 blat
                    blat.WaitForExit();                     // 等待执行完成
                }

                // 再将四个临时输出文件合并为 tempOut.psl
                for (i = 1; i <= 4; ++i)
                {
                    string tempOutSplitPath = tempDirPath + "tempOut" + i.ToString() + ".psl";
                    StreamReader srTempOutSplit = new StreamReader(tempOutSplitPath);
                    File.AppendAllText(tempOutFileName, srTempOutSplit.ReadToEnd());
                    srTempOutSplit.Close();
                }

                setblatNameRecordHT();              // 填充以需要 blat 的序列名字为键，最大的记录为值的哈希表 
            }

            // 初始化用户提供的 tfbsConsSites 特征出现次数哈希表
            userTfbsConsSitesHT.Clear();
            if (userTfbsConsSitesFeatureList.Count > 0)
            {
                foreach (string userTfbsConsSitesFeature in userTfbsConsSitesFeatureList)
                {
                    userTfbsConsSitesHT.Add(userTfbsConsSitesFeature, 0);
                }
            }
            
            userRmskHT.Clear();
            if (userRmskFeatureList.Count > 0)
            {
                foreach (string userRmskFeature in userRmskFeatureList)
                {
                    userRmskHT.Add(userRmskFeature, 0);
                }
            }
        }

        // 开始计算 按钮对应的函数
        // 函数功能：为一些细节初始化，创建 StartToCompute 线程开始进行计算
        private void btnStart_Click(object sender, EventArgs e)
        {
            // 结果已保存标志位置为否
            isSaved = false;

            if ((seqFileList.Count == 0 && rTxtBoxSequence.Lines.Length == 0) || (feaFileList.Count == 0 && rTxtBoxFeature.Lines.Length == 0 && !cBoxMod1.Checked && !cBoxMod2.Checked && !cBoxMod3.Checked && !cBoxMod4.Checked))
            {
                MessageBox.Show("序列或特征为空！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 创建临时目录
            if (!Directory.Exists(tempDirPath))
            {
                Directory.CreateDirectory(tempDirPath);
            }

            // 删除中间文件
            if (File.Exists(entireFileName))
            {
                File.Delete(entireFileName);
            }

            // 缩小到实际长度，减小占用的内存
            seqFileList.TrimToSize();
            feaFileList.TrimToSize();

            // 以下散列表、哈希表先置为空，在经过预处理之后会被填充
            userMotifFeatureList.Clear();
            userTfbsConsSitesFeatureList.Clear();
            userRmskFeatureList.Clear();
            userHistoneModification18NatFeatureList.Clear();
            userHistoneModification20CellFeatureList.Clear();
            userUpperFeatureList.Clear();
            userTfbsConsSitesHT.Clear();
            userRmskHT.Clear();
            userRepeatFeatureList.Clear();

            blatTitleNameList.Clear();
            blatNameChrHT.Clear();
            blatNameRecordHT.Clear();

            // 先对文件进行预处理
            trdPreTreatment = new Thread(Pretreatment);
            trdPreTreatment.IsBackground = true;
            Control.CheckForIllegalCrossThreadCalls = false;            // 取消线程安全保护模式
            isPreTreatmentThreadCreated = true;
            trdPreTreatment.Start();
            this.Text = "数据预处理中";
            trdPreTreatment.Join();                                     // 等待预处理线程的完成

            // 预处理之后以下散列表缩小到实际长度，减小占用的内存
            userMotifFeatureList.TrimToSize();
            userTfbsConsSitesFeatureList.TrimToSize();
            userRmskFeatureList.TrimToSize();
            userHistoneModification18NatFeatureList.TrimToSize();
            userHistoneModification20CellFeatureList.TrimToSize();
            userRepeatFeatureList.TrimToSize();

            // 开始计算后以下表已无存在的意义，释放掉内存
            blatTitleNameList.Clear();
            userUpperFeatureList.Clear();

            trdCompute = new Thread(StartToCompute);                    // 创建一个计算线程

            Stopwatch sw = new Stopwatch();
            sw.Start();

            trdCompute.IsBackground = true;                             // 后台运行
            Control.CheckForIllegalCrossThreadCalls = false;            // 取消线线程安全保护模式
            isComputeThreadCreated = true;
            trdCompute.Start();                                         // 开始执行线程
            trdCompute.Join();
            sw.Stop();

            MessageBox.Show("计算完毕！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            rTxtBoxFeatureName.Text += "\n" + "计算共耗时：" + sw.ElapsedMilliseconds.ToString() +" 毫秒";
            this.Text = "DNA 序列特征提取 V1.2";
        }

        // 清空结果 按钮对应的函数
        // 函数功能：把显示计算结果的文本框里的内容清空
        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                rTxtBoxResult.Clear();
                rTxtBoxFeatureName.Clear();

                foreach (string feaName in insFeatureName.tfbsConsSitesFeatureName)
                {
                    tfbsConsSitesHT[feaName] = 0;
                }

                foreach (string feaName in insFeatureName.rmskFeatureName)
                {
                    rmskHT[feaName] = 0;
                }

                userTfbsConsSitesHT.Clear();
                userRmskHT.Clear();

                userTfbsConsSitesFeatureList.Clear();
                userMotifFeatureList.Clear();
                userTfbsConsSitesFeatureList.Clear();
                userRmskFeatureList.Clear();
                userHistoneModification18NatFeatureList.Clear();
                userHistoneModification20CellFeatureList.Clear();
                userUpperFeatureList.Clear();
                userRepeatFeatureList.Clear();

                blatTitleNameList.Clear();
                blatNameChrHT.Clear();
                blatNameRecordHT.Clear();

                this.Text = "DNA 序列特征提取 V1.2";

                if (Directory.Exists(tempDirPath))
                {
                    Directory.Delete(tempDirPath, true);
                }

                if (File.Exists(entireFileName))
                {
                    File.Delete(entireFileName);
                }
            }
            catch (Exception ex)
            {
            }
        }

        // 保存结果 按钮对应的函数
        // 函数功能：把显示计算结果的文本框里的内容保存到文件
        private void btnSavaToFile_Click(object sender, EventArgs e)
        {
            bool isAnySaved = false;
            DialogResult dr;
            string[] DataToWrite;
            string fileInfoMsg = string.Empty;

            if (!File.Exists(titleFileName))
            {
                MessageBox.Show("一些中间计算结果文件丢失！\n请重新运行程序！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 保存序列的名字
            sfd.Title = "计算结果的序列名文件保存为";
            sfd.FileName = "resultSequence.txt";
            dr = sfd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                isAnySaved = true;
                fileInfoMsg += "序列名文件：\t" + sfd.FileName + "\n";

                StreamWriter sw = new StreamWriter(sfd.FileName);
                StreamReader sr = new StreamReader(titleFileName, Encoding.Default);
                while (sr.Peek() != -1)
                {
                    DataToWrite = sr.ReadLine().Split('\t');
                    sw.WriteLine(DataToWrite[0]);
                }
                
                sw.Flush();
                sw.Close();
                sr.Close();
            }

            // 保存特征的名字
            sfd.Title = "计算结果的特征名文件保存为";
            sfd.FileName = "resultFeature.txt";
            dr = sfd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                isAnySaved = true;
                fileInfoMsg += "特征名文件：\t" + sfd.FileName + "\n";

                StreamWriter sw = new StreamWriter(sfd.FileName);

                foreach (string featureName in rTxtBoxFeatureName.Lines)
                {
                    sw.WriteLine(featureName);
                }

                if (userRepeatFeatureList.Count > 0)
                {
                    sw.WriteLine("repeat features are:");
                    foreach (string feaToWrite in userRepeatFeatureList)
                    {
                        sw.Write(feaToWrite + "\t");
                    }
                }
                sw.Flush();
                sw.Close();
            }

            // 保存矩阵
            sfd.Title = "计算结果的矩阵保存为";
            sfd.FileName = "resultMatrix.txt";
            dr = sfd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                isAnySaved = true;
                fileInfoMsg += "计算结果矩阵：\t" + sfd.FileName + "\n";

                StreamWriter sw = new StreamWriter(sfd.FileName);

                sw.Write(rTxtBoxResult.Text);
                sw.Flush();
                sw.Close();
            }

            if (isAnySaved)
            {
                isSaved = true;
                MessageBox.Show("计算结果已成功保存！\n\n已保存的文件为：\n\n" + fileInfoMsg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                isSaved = false;
                MessageBox.Show("计算结果未保存！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // 使用文件 按钮对应的函数
        // 函数功能：实现通过文件提供序列和特征
        // 将文件的路径存储到私有成员变量 seqFileList 和 feaFileList 中
        private void btnUsingFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();                  // 打开文件对话框

            ofd.AddExtension = true;
            ofd.CheckFileExists = true;
            ofd.Filter = "fa 文件(.fa)|*.fa|所有文件(*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.Multiselect = true;
            ofd.RestoreDirectory = true;
            ofd.ShowReadOnly = true;
            ofd.Title = "选择文件";

            ofd.ShowDialog();
            string[] files = ofd.FileNames;

            if ((string)(sender as Button).Tag == "su")
            {
                isSeqBtn = true;
            }
            else if ((string)(sender as Button).Tag == "fu")
            {
                isSeqBtn = false;
            }

            foreach (string fileName in files)
            {
                if (isSeqBtn)
                {
                    if (!seqFileList.Contains(fileName))
                    {
                        seqFileList.Add(fileName);
                    }
                }
                else
                {
                    if (!feaFileList.Contains(fileName))
                    {
                        feaFileList.Add(fileName);
                    }
                }
            }
        }

        // 窗体载入时对应的函数
        // 函数功能：初始化相关成员变量
        private void FrmGetFeatureInfo_Load(object sender, EventArgs e)
        {
            try
            {
                string MESSAGE;
                MESSAGE = "-------------------------------------------------------\n";
                MESSAGE += "		Developed by: \n";
                MESSAGE += "   Kun Yang(杨昆), Shengdong Dai(戴胜东)\n";
                MESSAGE += "School of Computer Science and Technology\n";
                MESSAGE += "	Hangzhou Dianzi University\n";
                MESSAGE += "yangkun@hdu.edu.cn; daishengdong@gmail.com\n";
                MESSAGE += "-------------------------------------------------------";
                MessageBox.Show(MESSAGE, "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);

                NA = false;

                sfd.AddExtension = true;
                sfd.CheckFileExists = false;
                sfd.Filter = "txt 文件(.txt)|*.txt|所有文件(*.*)|*.*";
                sfd.FilterIndex = 1;
                sfd.OverwritePrompt = true;
                sfd.RestoreDirectory = true;

                // 初始化哈希表
                foreach (string feaName in insFeatureName.tfbsConsSitesFeatureName)
                {
                    tfbsConsSitesHT.Add(feaName, 0);
                }

                foreach (string feaName in insFeatureName.rmskFeatureName)
                {
                    rmskHT.Add(feaName, 0);
                }

                foreach (string iupac in insFeatureName.iupac)
                {
                    switch (iupac)
                    {
                        case "M":
                        case "m":
                            IUPACHT.Add(iupac, "[A|C]");
                            break;
                        case "R":
                        case "r":
                            IUPACHT.Add(iupac, "[A|G]");
                            break;
                        case "W":
                        case "w":
                            IUPACHT.Add(iupac, "[A|T]");
                            break;
                        case "S":
                        case "s":
                            IUPACHT.Add(iupac, "[C|G]");
                            break;
                        case "Y":
                        case "y":
                            IUPACHT.Add(iupac, "[C|T]");
                            break;
                        case "K":
                        case "k":
                            IUPACHT.Add(iupac, "[G|T]");
                            break;
                        case "V":
                        case "v":
                            IUPACHT.Add(iupac, "[A|C|G]");
                            break;
                        case "H":
                        case "h":
                            IUPACHT.Add(iupac, "[A|C|T]");
                            break;
                        case "D":
                        case "d":
                            IUPACHT.Add(iupac, "[A|G|T]");
                            break;
                        case "B":
                        case "b":
                            IUPACHT.Add(iupac, "[C|G|T]");
                            break;
                        case "X":
                        case "x":
                            IUPACHT.Add(iupac, "[G|A|T|C]");
                            break;
                        case "N":
                        case "n":
                            IUPACHT.Add(iupac, "[G|A|T|C]");
                            break;
                    }
                }

                if (Directory.Exists(tempDirPath))
                {
                    Directory.Delete(tempDirPath, true);
                }

                if (File.Exists(entireFileName))
                {
                    File.Delete(entireFileName);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("程序初始化失败！\n原因：临时文件被其他进程占用而无法删除！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // 程序关闭时调用的函数
        // 函数功能：对用户进行相关信息的提醒
        // 主要是提供一个更好的交互接口
        // 并做一些善后的工作
        private void FrmGetFeatureInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (isComputeThreadCreated && trdCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("程序计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdCompute.Abort();
                    }
                }

                if (isPreTreatmentThreadCreated && trdPreTreatment.IsAlive)
                {
                    DialogResult result = MessageBox.Show("程序预处理线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdPreTreatment.Abort();
                    }
                }

                if (isMotifComputeThreadCreated && trdMotifCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("motif 特征计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdMotifCompute.Abort();
                    }
                }

                if (isUserMotifComputeThreadCreated && trdUserMotifCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("motif 特征计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdUserMotifCompute.Abort();
                    }
                }

                if (isTfbsConsSitesComputeThreadCreated && trdTfbsConsSitesCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("tfbsConsSites 特征计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdTfbsConsSitesCompute.Abort();
                    }
                }

                if (isUserTfbsConsSitesComputeThreadCreated && trdUserTfbsConsSitesCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("tfbsConsSites 特征计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdUserTfbsConsSitesCompute.Abort();
                    }
                }

                if (isRmskComputeThreadCreated && trdRmskCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("rmsk 特征计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdRmskCompute.Abort();
                    }
                }

                if (isUserRmskComputeThreadCreated && trdUserRmskCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("rmsk 特征计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdUserRmskCompute.Abort();
                    }
                }

                if (isHistoneModificationComputeThreadCreated && trdHistoneModificationCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("histone modification 特征计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdHistoneModificationCompute.Abort();
                    }
                }

                if (isUserHistoneModificationComputeThreadCreated && trdUserHistoneModificationCompute.IsAlive)
                {
                    DialogResult result = MessageBox.Show("histone modification 特征计算线程仍在运行中\n是否强制退出？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                    else
                    {
                        trdUserHistoneModificationCompute.Abort();
                    }
                }

                if (!isSaved && rTxtBoxResult.Lines.Length > 0)
                {
                    DialogResult result = MessageBox.Show("计算结果尚未保存\n确定退出吗", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                if (Directory.Exists(tempDirPath))
                {
                    Directory.Delete(tempDirPath, true);
                }

                if (File.Exists(entireFileName))
                {
                    File.Delete(entireFileName);
                }
            }
            catch (Exception ex)
            {
            }
        }

        // 显示详情 按钮对应的函数
        // 函数功能：把私有成员变量 seqFileList 和 feaFileList 里保存的已添加的文件信息显示给用户
        private void btnViewFile_Click(object sender, EventArgs e)
        {
            if ((string)(sender as Button).Tag == "sv")
            {
                isSeqBtn = true;
            }
            else if ((string)(sender as Button).Tag == "fv")
            {
                isSeqBtn = false;
            }

            string msg = "";
            if (isSeqBtn)
            {
                foreach (var obj in seqFileList)
                {
                    msg += (string)obj + "\n";
                }
            }
            else
            {
                foreach (var obj in feaFileList)
                {
                    msg += (string)obj + "\n";
                }
            }
            if (msg.Trim() == string.Empty)
            {
                MessageBox.Show("尚未添加任何文件！", "已添加文件", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(msg.Trim(), "当前已添加文件", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        // 复位程序 按钮对应的函数
        // 函数功能：对相关成员变量进行归置
        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                rTxtBoxSequence.Clear();
                rTxtBoxFeature.Clear();
                rTxtBoxResult.Clear();
                rTxtBoxFeatureName.Clear();

                seqFileList.Clear();
                feaFileList.Clear();

                userMotifFeatureList.Clear();
                userTfbsConsSitesFeatureList.Clear();
                userRmskFeatureList.Clear();
                userHistoneModification18NatFeatureList.Clear();
                userHistoneModification20CellFeatureList.Clear();
                userUpperFeatureList.Clear();
                userRepeatFeatureList.Clear();

                blatTitleNameList.Clear();
                blatNameChrHT.Clear();
                blatNameRecordHT.Clear();

                this.Text = "DNA 序列特征提取 V1.2";

                cBoxMod1.Checked = false;
                cBoxMod2.Checked = false;
                cBoxMod3.Checked = false;
                cBoxMod4.Checked = false;

                foreach (string feaName in insFeatureName.tfbsConsSitesFeatureName)
                {
                    tfbsConsSitesHT[feaName] = 0;
                }

                foreach (string feaName in insFeatureName.rmskFeatureName)
                {
                    rmskHT[feaName] = 0;
                }

                userTfbsConsSitesHT.Clear();
                userRmskHT.Clear();

                if (Directory.Exists(tempDirPath))
                {
                    Directory.Delete(tempDirPath, true);
                }

                if (File.Exists(entireFileName))
                {
                    File.Delete(entireFileName);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}