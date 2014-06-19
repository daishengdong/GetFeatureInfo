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
        private bool isSeqBtn;  // 是否是有关序列的按钮
        private bool isSaved;   // 计算结果是否已保存
        private bool checkED;   // 是否需要计算除 motif 之外的特征    

        // 行数、特征名的类实体
        private ClassFeatureNameList insClassFeatureName = new ClassFeatureNameList();
        private ClassChrLineIndex insClassChrLineIndex = new ClassChrLineIndex();                 

        private SaveFileDialog sfd = new SaveFileDialog();                  // 保存计算结果另存为的对话框
        private List<string> seqFileList = new List<string>();                    // 保存提供序列的文件的路径
        private List<string> feaFileList = new List<string>();                    // 保存提供特征的文件的路径

        // 为去除重复设置的变量
        private List<string> userUpperFeatureList = new List<string>();               // 存放用户输入的大写的特征

        private Hashtable blatNameChrHT = new Hashtable();                  // 序列名字为键、染色体信息为值的哈希表
        private Hashtable blatNameRecordHT = new Hashtable();               // 序列名字为键、整条 tempOut.psl 记录为值得哈希表
        private List<string> blatTitleNameList = new List<string>();              // 需要使用 blat 的序列的头名称

        // 一些计算线程实体
        private Thread trdCompute;                                          // 主计算线程实体
        private Thread trdPreTreatment;                                     // 序列文件预处理进程实体

        // 线程是否被创建的标志位
        private bool isComputeThreadCreated = false;                        // 计算线程是否已被创建
        private bool isPreTreatmentThreadCreated = false;                   // 预处理线程是否已被创建

        // ClassSequenceList 类实例，主要是将用户提供的序列头部即序列保存并串接起来
        // 其内部提供了方法，可以对序列特征进行计算
        private ClassSequenceList insClassSequenceList = new ClassSequenceList();

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
        // 这是调用 blat 处理数据的核心函数
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
            StreamReader sr = new StreamReader(ClassPath.tempOutFileName, Encoding.Default);

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
        }

        // 函数功能：在文本框和文件中提取出序列和特征
        // 通过调用 SubstringCount 函数实现信息的统计
        // 被 btnStart_Click 函数调用，用以创建计算线程
        private void StartToCompute()
        {
            List<ManualResetEvent> mreList = new List<ManualResetEvent>();

            if (cBoxMod1.Checked)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(insClassSequenceList.computeMotif), mre);
                mreList.Add(mre);
            }

            if (insClassSequenceList.UserMotifFeatureList.Count > 0)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(insClassSequenceList.computeUserMotif), mre);
                mreList.Add(mre);
            }

            if (cBoxMod2.Checked)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(insClassSequenceList.computeTfbsConsSites), mre);
                mreList.Add(mre);
            }

            if (insClassSequenceList.UserTfbsConsSitesFeatureList.Count > 0)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(insClassSequenceList.computeUserTfbsConsSites), mre);
                mreList.Add(mre);
            }

            if (cBoxMod3.Checked)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(insClassSequenceList.computeRmsk), mre);
                mreList.Add(mre);
            }

            if (insClassSequenceList.UserRmskFeatureList.Count > 0)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(insClassSequenceList.computeUserRmsk), mre);
                mreList.Add(mre);
            }

            if (cBoxMod4.Checked)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(insClassSequenceList.computeHistoneModification), mre);
                mreList.Add(mre);
            }

            if (insClassSequenceList.UserHistoneModification18NatFeatureList.Count > 0 || insClassSequenceList.UserHistoneModification20CellFeatureList.Count > 0)
            {
                ManualResetEvent mre = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback(insClassSequenceList.computeUserHistoneModification), mre);
                mreList.Add(mre);
            }

            WaitHandle.WaitAll(mreList.ToArray());
        }

        // 函数功能：对序列文件进行预处理
        // 把序列文件分为头 title 和序列本身 seq，两个指针同时向下读，方便进行处理
        // 处理后的文件存放在程序所在根目录下的 temp 文件夹
        // 被 btnStart_Click 事件调用
        // 此函数是本程序逻辑最容易发生错乱的地方，也是程序最耗时的地方
        private void Pretreatment()
        {
            checkED = false;

            bool seqNeedBlat = false;       // 以下的序列是否需要 blat
            bool titleNeedBlat = false;     // 头部是否需要 blat
            bool needBlat = false;          // 所有的序列是否需要用 bl需要 blat。只要有一条需要，则置为真

            bool isFirstSeq = true;         // 是不是第一条序列
            bool isTitle = true;            // 是不是头部
            bool isLastTitle = false;       // 记录当前的序列出现时上一次出现的是否是头部
            bool isFirstBlatTitle = true;   // 是否是第一个需要 blat 的头部（设置该标志位是为了防止整合后的需要 blat 的整体文件不是顶行的而导致用 blat 处理时出现问题）
            string record;
            string feature;

            StreamWriter swTitle = new StreamWriter(ClassPath.titleFileName, true);           // 记录序列头部的文件写入流
            StreamWriter swSeq = new StreamWriter(ClassPath.seqFileName, true);               // 记录序列本身的文件写入流
            StreamWriter swEntire = new StreamWriter(ClassPath.entireFileName, true);         // 记录需要 blat 的整体文件的写入流

            string titleToWrite = string.Empty;
            string seqToWrite = string.Empty;
            string entireToWrite = string.Empty;

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
                                    entireToWrite += record + "\n";
                                    isFirstBlatTitle = false;
                                }
                                else
                                {
                                    // 否则要多加入一个换行符
                                    entireToWrite += "\n" + record + "\n";
                                }
                            }
                            titleToWrite += record + "\n";
                            isLastTitle = true;
                        }
                        else
                        {
                            if (isLastTitle && !isFirstSeq)
                            {
                                seqToWrite += "\n" + record;
                            }
                            else
                            {
                                seqToWrite += record;
                            }
                            if (seqNeedBlat)
                            {
                                entireToWrite += record;
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
                                entireToWrite += record + "\n";
                                isFirstBlatTitle = false;
                            }
                            else
                            {
                                entireToWrite += "\n" + record + "\n";
                            }
                        }

                        titleToWrite += record + "\n";
                        isLastTitle = true;
                    }
                    else
                    {
                        if (isLastTitle && !isFirstSeq)
                        {
                            seqToWrite += "\n" + record;
                        }
                        else
                        {
                            seqToWrite += record;
                        }
                        if (seqNeedBlat)
                        {
                            entireToWrite += record;
                        }
                        isFirstSeq = false;
                        isLastTitle = false;
                    }
                }
            }

            titleToWrite += "\n";
            swTitle.Write(titleToWrite);
            swTitle.Flush();
            swTitle.Close();

            seqToWrite += "\n\n";
            swSeq.Write(seqToWrite);
            swSeq.Flush();
            swSeq.Close();

            // 在所有需要使用 blat 的序列文件后面追加一个换行符
            // 否则使用 blat 时会出错
            entireToWrite += "\n\n";
            swEntire.Write(entireToWrite);
            swEntire.Flush();
            swEntire.Close();

            // 把文件中的特征全部提取到一个 List 中
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
                            if (insClassFeatureName.tfbsConsSitesFeatureName.Contains(feature))
                            {
                                // 如果第二个复选框没有被选中
                                if (!cBoxMod2.Checked)
                                {
                                    // 加入到用户提供的 fbsConsSites 序列中
                                    insClassSequenceList.UserTfbsConsSitesFeatureList.Add(feature);
                                }
                                else
                                {
                                    // 如果被选中，则必然是重复的
                                    insClassSequenceList.UserRepeatFeatureList.Add(feature);
                                }

                                // 后面的比较不需要做了，直接跳过
                                continue;
                            }

                            if (insClassFeatureName.rmskFeatureName.Contains(feature))
                            {
                                if (!cBoxMod3.Checked)
                                {
                                    insClassSequenceList.UserRmskFeatureList.Add(feature);
                                }
                                else
                                {
                                    insClassSequenceList.UserRepeatFeatureList.Add(feature);
                                }

                                continue;
                            }

                            if (insClassFeatureName.histoneModification18NatFeatureName.Contains(feature))
                            {
                                if (!cBoxMod4.Checked)
                                {
                                    insClassSequenceList.UserHistoneModification18NatFeatureList.Add(feature);
                                }
                                else
                                {
                                    insClassSequenceList.UserRepeatFeatureList.Add(feature);
                                }

                                continue;
                            }

                            if (insClassFeatureName.histoneModification20CellFeatureName.Contains(feature))
                            {
                                if (!cBoxMod4.Checked)
                                {
                                    insClassSequenceList.UserHistoneModification20CellFeatureList.Add(feature);
                                }
                                else
                                {
                                    insClassSequenceList.UserRepeatFeatureList.Add(feature);
                                }

                                continue;
                            }

                            // 剩下来的全部归入 motif 特征
                            if (insClassFeatureName.motifFeatureName.Contains(feature, StringComparer.OrdinalIgnoreCase))
                            {
                                // 如果默认的已经有了，但复选框 1 没有选中
                                if (!cBoxMod1.Checked)
                                {
                                    // 加入到用户的 motif 特征中去
                                    insClassSequenceList.UserMotifFeatureList.Add(feature);
                                }
                                else
                                {
                                    // 否则直接加入到重复序列中去
                                    insClassSequenceList.UserRepeatFeatureList.Add(feature);
                                }

                                continue;
                            }

                            // 是以上所有情况之外的，一律加入到用户自定义的 motif 特征中去
                            insClassSequenceList.UserMotifFeatureList.Add(feature);
                        }
                        else
                        {
                            // 如果与用户之前输入的有冲突，直接放入重复序列中去
                            insClassSequenceList.UserRepeatFeatureList.Add(feature);
                        }
                    }
                    srFea.Close();
                }
            }

            // 把文本框中的特征全部提取到一个 List 中
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
                        if (insClassFeatureName.tfbsConsSitesFeatureName.Contains(feature))
                        {
                            // 如果第二个复选框没有被选中
                            if (!cBoxMod2.Checked)
                            {
                                // 加入到用户提供的 fbsConsSites 序列中
                                insClassSequenceList.UserTfbsConsSitesFeatureList.Add(feature);
                            }
                            else
                            {
                                // 如果被选中，则必然是重复的
                                insClassSequenceList.UserRepeatFeatureList.Add(feature);
                            }

                            // 后面的比较不需要做了，直接跳过
                            continue;
                        }

                        if (insClassFeatureName.rmskFeatureName.Contains(feature))
                        {
                            if (!cBoxMod3.Checked)
                            {
                                insClassSequenceList.UserRmskFeatureList.Add(feature);
                            }
                            else
                            {
                                insClassSequenceList.UserRepeatFeatureList.Add(feature);
                            }

                            continue;
                        }

                        if (insClassFeatureName.histoneModification18NatFeatureName.Contains(feature))
                        {
                            if (!cBoxMod4.Checked)
                            {
                                insClassSequenceList.UserHistoneModification18NatFeatureList.Add(feature);
                            }
                            else
                            {
                                insClassSequenceList.UserRepeatFeatureList.Add(feature);
                            }

                            continue;
                        }

                        if (insClassFeatureName.histoneModification20CellFeatureName.Contains(feature))
                        {
                            if (!cBoxMod4.Checked)
                            {
                                insClassSequenceList.UserHistoneModification20CellFeatureList.Add(feature);
                            }
                            else
                            {
                                insClassSequenceList.UserRepeatFeatureList.Add(feature);
                            }

                            continue;
                        }

                        // 剩下来的全部归入 motif 特征
                        if (insClassFeatureName.motifFeatureName.Contains(feature, StringComparer.OrdinalIgnoreCase))
                        {
                            // 如果默认的已经有了，但复选框 1 没有选中
                            if (!cBoxMod1.Checked)
                            {
                                // 加入到用户的 motif 特征中去
                                insClassSequenceList.UserMotifFeatureList.Add(feature);
                            }
                            else
                            {
                                // 否则直接加入到重复序列中去
                                insClassSequenceList.UserRepeatFeatureList.Add(feature);
                            }

                            continue;
                        }

                        // 是以上所有情况之外的，一律加入到用户自定义的 motif 特征中去
                        insClassSequenceList.UserMotifFeatureList.Add(feature);
                    }
                    else
                    {
                        // 如果与用户之前输入的有冲突，直接放入重复序列中去
                        insClassSequenceList.UserRepeatFeatureList.Add(feature);
                    }
                }
            }

            if (cBoxMod2.Checked || cBoxMod3.Checked || cBoxMod4.Checked || insClassSequenceList.UserTfbsConsSitesFeatureList.Count > 0 || insClassSequenceList.UserRmskFeatureList.Count > 0 || insClassSequenceList.UserHistoneModification18NatFeatureList.Count > 0 || insClassSequenceList.UserHistoneModification20CellFeatureList.Count > 0)
            {
                checkED = true;
                if (needBlat)
                {
                    // 调用 blat 处理 entireFile，得到四个文件，合并之后留做后用
                    if (!Directory.Exists(ClassPath.blatPath))
                    {
                        MessageBox.Show("blat 可执行文件丢失！\n请确保该文件存在于 blat 目录中！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 执行 blat 命令，得到索引文件 tempOut*.psl
                    int i;
                    Process blat = new Process();
                    blat.StartInfo.FileName = ClassPath.pathOfBlat;

                    for (i = 1; i <= 4; ++i)
                    {
                        string args = @"hg18_" + i.ToString() + @".2bit entireFile.fa -noHead -ooc=11.ooc temp\tempOut" + i.ToString() + @".psl";   // 启动参数
                        blat.StartInfo.Arguments = args;
                        blat.StartInfo.CreateNoWindow = true;
                        blat.StartInfo.UseShellExecute = false;
                        blat.Start();                           // 启动 blat
                        blat.WaitForExit();                     // 等待执行完成
                    }

                    // 再将四个临时输出文件合并为 tempOut.psl
                    for (i = 1; i <= 4; ++i)
                    {
                        string tempOutSplitPath = ClassPath.tempDirPath + @"tempOut" + i.ToString() + @".psl";
                        StreamReader srTempOutSplit = new StreamReader(tempOutSplitPath);
                        File.AppendAllText(ClassPath.tempOutFileName, srTempOutSplit.ReadToEnd());
                        srTempOutSplit.Close();
                    }

                    setblatNameRecordHT();              // 填充以需要 blat 的序列名字为键，最大的记录为值的哈希表 
                }
            }

            // 再把已写入文件的 titile、seq 信息写回内存
            // 保存到序列的散列表中
            // 这一步在填充好 blat 计算的名字和记录对应的哈希表之后立即做
            StreamReader srTitleToGet = new StreamReader(ClassPath.titleFileName, Encoding.Default);
            StreamReader srSeqToGet = new StreamReader(ClassPath.seqFileName, Encoding.Default);

            insClassSequenceList.SequenceList.Clear();
            int indexMem = -1;
            while (srTitleToGet.Peek() != -1 && srSeqToGet.Peek() != -1)
            {
                ++indexMem;
                string title = srTitleToGet.ReadLine().Trim();
                string seq = srSeqToGet.ReadLine().Trim();
                
                if (title == string.Empty || seq == string.Empty)
                {
                    continue;
                }

                string[] splitStr = title.Split('\t');
                string titleName = splitStr[0].Trim();
                int length = seq.Length;

                if (checkED)
                {
                    // 如果需要算 motif 以外的其他
                    // 加入长度信息
                    if (needBlat)
                    {
                        if (blatTitleNameList.Contains(titleName))
                        {
                            string blatNameRecord = (string)blatNameRecordHT[titleName];
                            if (blatNameRecord != string.Empty)
                            {
                                // 不为空能找到的话
                                // 把最大的记录取出来并剥离信息构造类
                                string[] tempSplitStr = blatNameRecord.Split('\t');
                                string chr = tempSplitStr[13];
                                int start = Convert.ToInt32(tempSplitStr[15]);
                                int end = Convert.ToInt32(tempSplitStr[16]);

                                ClassTitle insTitleClass = new ClassTitle(titleName, chr, length, start, end);
                                ClassSequence seqClass = new ClassSequence(indexMem, insTitleClass, seq);
                                // 加入散列表中
                                insClassSequenceList.SequenceList.Add(seqClass);
                            }
                            else
                            {
                                // 为空的话证明 blat 没找到
                                // 这个序列在算的时候必然为 NA，因为起始位点信息为空
                                ClassTitle insTitleClass = new ClassTitle(titleName, length);
                                ClassSequence seqClass = new ClassSequence(indexMem, insTitleClass, seq);
                                // 加入散列表中
                                insClassSequenceList.SequenceList.Add(seqClass);
                            }
                        }
                        else
                        {
                            // 如果需要 blat 的名字列表中没有的话
                            // 则用户提供的该序列必为完整的
                            // 否则就一定会被收入 blatTitleNameList 中
                            string chr = splitStr[1].Trim();
                            int start = Convert.ToInt32(splitStr[3].Trim());
                            int end = Convert.ToInt32(splitStr[4].Trim());

                            ClassTitle insTitleClass = new ClassTitle(titleName, chr, length, start, end);
                            ClassSequence seqClass = new ClassSequence(indexMem, insTitleClass, seq);
                            // 加入散列表中
                            insClassSequenceList.SequenceList.Add(seqClass);
                        }
                    }
                    else
                    {
                        // 如果不需要 blat，证明用户已经提供了完整的信息
                        string chr = splitStr[1].Trim();
                        int start = Convert.ToInt32(splitStr[3].Trim());
                        int end = Convert.ToInt32(splitStr[4].Trim());

                        ClassTitle insTitleClass = new ClassTitle(titleName, chr, length, start, end);
                        ClassSequence seqClass = new ClassSequence(indexMem, insTitleClass, seq);
                        // 加入散列表中
                        insClassSequenceList.SequenceList.Add(seqClass);
                    }
                }
                else
                {
                    // 只需要算 motif 的话不需要额外的信息
                    // 名字、序列、序列长度就行了
                    ClassTitle insTitleClass = new ClassTitle(titleName, length);
                    ClassSequence seqClass = new ClassSequence(indexMem, insTitleClass, seq);
                    // 加入散列表中
                    insClassSequenceList.SequenceList.Add(seqClass);
                }
            }
            srTitleToGet.Close();
            srSeqToGet.Close();

            if (checkED)
            {
                // 只要需要计算 motif 以外的特征
                // 则下面的两个步骤不可或缺
                ClassSorter.sortByChr(insClassSequenceList);
                insClassSequenceList.getChrList();
            }
        }

        // 输出结果函数
        private void Output(long ElapsedMilliseconds)
        {
            // 输出之前按序号排序
            // 就是还原成用户输入的顺序
            if (checkED)
            {
                ClassSorter.sortByIndex(insClassSequenceList);
            }

            StringBuilder bufferFeatureNameTitile = new StringBuilder();
            StringBuilder bufferFeatureName = new StringBuilder();
            StringBuilder bufferResultTitle = new StringBuilder();
            StringBuilder bufferResult = new StringBuilder();

            int index = 0;

            // 输出特征头部预处理
            if (insClassSequenceList.IsMotifComputed)
            {
                foreach (string feaName in insClassFeatureName.motifFeatureName)
                {
                    ++index;
                    bufferFeatureNameTitile.Append(index.ToString().PadRight(feaName.Length) + "\t");
                    bufferResultTitle.Append(index.ToString().PadRight(7) + "\t");
                    bufferFeatureName.Append(feaName + "\t");
                }
            }

            if (insClassSequenceList.IsUserMotifComputed)
            {
                foreach (string feaName in insClassSequenceList.UserMotifFeatureList)
                {
                    ++index;
                    bufferFeatureNameTitile.Append(index.ToString().PadRight(feaName.Length) + "\t");
                    bufferResultTitle.Append(index.ToString().PadRight(7) + "\t");
                    bufferFeatureName.Append(feaName + "\t");
                }
            }

            if (insClassSequenceList.IsTfbsConsSitesComputed)
            {
                foreach (string feaName in insClassFeatureName.tfbsConsSitesFeatureName)
                {
                    ++index;
                    bufferFeatureNameTitile.Append(index.ToString().PadRight(feaName.Length) + "\t");
                    bufferResultTitle.Append(index.ToString().PadRight(7) + "\t");
                    bufferFeatureName.Append(feaName + "\t");
                }
            }

            if (insClassSequenceList.IsUserTfbsConsSitesComputed)
            {
                foreach (string feaName in insClassSequenceList.UserTfbsConsSitesFeatureList)
                {
                    ++index;
                    bufferFeatureNameTitile.Append(index.ToString().PadRight(feaName.Length) + "\t");
                    bufferResultTitle.Append(index.ToString().PadRight(7) + "\t");
                    bufferFeatureName.Append(feaName + "\t");
                }
            }

            if (insClassSequenceList.IsRmskComputed)
            {
                foreach (string feaName in insClassFeatureName.rmskFeatureName)
                {
                    ++index;
                    bufferFeatureNameTitile.Append(index.ToString().PadRight(feaName.Length) + "\t");
                    bufferResultTitle.Append(index.ToString().PadRight(7) + "\t");
                    bufferFeatureName.Append(feaName + "\t");
                }
            }

            if (insClassSequenceList.IsUserRmskComputed)
            {
                foreach (string feaName in insClassSequenceList.UserRmskFeatureList)
                {
                    ++index;
                    bufferFeatureNameTitile.Append(index.ToString().PadRight(feaName.Length) + "\t");
                    bufferResultTitle.Append(index.ToString().PadRight(7) + "\t");
                    bufferFeatureName.Append(feaName + "\t");
                }
            }

            if (insClassSequenceList.IsHistoneModificationComputed)
            {
                foreach (string feaName in insClassFeatureName.histoneModificationFeatureName)
                {
                    ++index;
                    bufferFeatureNameTitile.Append(index.ToString().PadRight(feaName.Length) + "\t");
                    bufferResultTitle.Append(index.ToString().PadRight(7) + "\t");
                    bufferFeatureName.Append(feaName + "\t");
                }
            }

            if (insClassSequenceList.IsUserHistoneModificationComputed)
            {
                foreach (string feaName in insClassSequenceList.UserHistoneModificationFeatureList)
                {
                    ++index;
                    bufferFeatureNameTitile.Append(index.ToString().PadRight(feaName.Length) + "\t");
                    bufferResultTitle.Append(index.ToString().PadRight(7) + "\t");
                    bufferFeatureName.Append(feaName + "\t");
                }
            }

            foreach (ClassSequence insClassSequence in insClassSequenceList.SequenceList)
            {
                if (insClassSequenceList.IsMotifComputed)
                {
                    foreach (string feaName in insClassFeatureName.motifFeatureName)
                    {
                        if (feaName == "%G+C" || feaName == "CpG ratio")
                        {
                            bufferResult.Append(string.Format("{0:F5}", insClassSequence.MotifCountHT[feaName]) + "\t");
                        }
                        else
                        {
                            bufferResult.Append(string.Format("{0:F5}", Convert.ToInt32(insClassSequence.MotifCountHT[feaName]) * 10 / (double)insClassSequence.Title.Length) + "\t");
                        }
                    }
                }

                if (insClassSequenceList.IsUserMotifComputed)
                {
                    foreach (string feaName in insClassSequenceList.UserMotifFeatureList)
                    {
                        if (feaName == "%G+C" || feaName == "CpG ratio")
                        {
                            bufferResult.Append(string.Format("{0:F5}", insClassSequence.UserMotifCountHT[feaName]) + "\t");
                        }
                        else
                        {
                            bufferResult.Append(string.Format("{0:F5}", Convert.ToInt32(insClassSequence.UserMotifCountHT[feaName]) * 10 / (double)insClassSequence.Title.Length) + "\t");
                        }
                    }
                }

                if (insClassSequenceList.IsTfbsConsSitesComputed)
                {
                    foreach (string feaName in insClassFeatureName.tfbsConsSitesFeatureName)
                    {
                        if (insClassSequence.TfbsConsSitesCountHT[feaName].ToString() == "NA")
                        {
                            bufferResult.Append(string.Format("{0:F5}", "NA") + "\t");
                        }
                        else
                        {
                            bufferResult.Append(string.Format("{0:F5}", Convert.ToInt32(insClassSequence.TfbsConsSitesCountHT[feaName]) * 10 / (double)insClassSequence.Title.Length) + "\t");
                        }
                    }
                }

                if (insClassSequenceList.IsUserTfbsConsSitesComputed)
                {
                    foreach (string feaName in insClassSequenceList.UserTfbsConsSitesFeatureList)
                    {
                        if (insClassSequence.UserTfbsConsSitesCountHT[feaName].ToString() == "NA")
                        {
                            bufferResult.Append(string.Format("{0:F5}", "NA") + "\t");
                        }
                        else
                        {
                            bufferResult.Append(string.Format("{0:F5}", Convert.ToInt32(insClassSequence.UserTfbsConsSitesCountHT[feaName]) * 10 / (double)insClassSequence.Title.Length) + "\t");
                        }
                    }
                }

                if (insClassSequenceList.IsRmskComputed)
                {
                    foreach (string feaName in insClassFeatureName.rmskFeatureName)
                    {
                        if (insClassSequence.RmskCountHT[feaName].ToString() == "NA")
                        {
                            bufferResult.Append(string.Format("{0:F5}", "NA") + "\t");
                        }
                        else
                        {
                            bufferResult.Append(string.Format("{0:F5}", Convert.ToInt32(insClassSequence.RmskCountHT[feaName]) * 10 / (double)insClassSequence.Title.Length) + "\t");
                        }
                    }
                }

                if (insClassSequenceList.IsUserRmskComputed)
                {
                    foreach (string feaName in insClassSequenceList.UserRmskFeatureList)
                    {
                        if (insClassSequence.UserRmskCountHT[feaName].ToString() == "NA")
                        {
                            bufferResult.Append(string.Format("{0:F5}", "NA") + "\t");
                        }
                        else
                        {
                            bufferResult.Append(string.Format("{0:F5}", Convert.ToInt32(insClassSequence.UserRmskCountHT[feaName]) * 10 / (double)insClassSequence.Title.Length) + "\t");
                        }
                    }
                }

                if (insClassSequenceList.IsHistoneModificationComputed)
                {
                    foreach (string feaName in insClassFeatureName.histoneModificationFeatureName)
                    {
                        if (insClassSequence.HistoneModificationCountHT[feaName].ToString() == "NA")
                        {
                            bufferResult.Append(string.Format("{0:F5}", "NA") + "\t");
                        }
                        else
                        {
                            bufferResult.Append(string.Format("{0:F5}", Convert.ToInt32(insClassSequence.HistoneModificationCountHT[feaName]) * 10 / (double)insClassSequence.Title.Length) + "\t");
                        }
                    }
                }

                if (insClassSequenceList.IsUserHistoneModificationComputed)
                {
                    foreach (string feaName in insClassSequenceList.UserHistoneModificationFeatureList)
                    {
                        if (insClassSequence.UserHistoneModificationCountHT[feaName].ToString() == "NA")
                        {
                            bufferResult.Append(string.Format("{0:F5}", "NA") + "\t");
                        }
                        else
                        {
                            bufferResult.Append(string.Format("{0:F5}", Convert.ToInt32(insClassSequence.UserHistoneModificationCountHT[feaName]) * 10 / (double)insClassSequence.Title.Length) + "\t");
                        }
                    }
                }

                bufferResult.AppendLine();
            }

            bufferFeatureNameTitile.AppendLine();
            bufferFeatureName.AppendLine();
            bufferFeatureName.Append("计算共耗时：" + ElapsedMilliseconds.ToString() + " 毫秒");
            rTxtBoxFeatureName.Text += bufferFeatureNameTitile.ToString();
            rTxtBoxFeatureName.Text += bufferFeatureName.ToString();

            bufferResultTitle.AppendLine();
            rTxtBoxResult.Text += bufferResultTitle.ToString();
            rTxtBoxResult.Text += bufferResult.ToString();
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
            if (!Directory.Exists(ClassPath.tempDirPath))
            {
                Directory.CreateDirectory(ClassPath.tempDirPath);
            }

            // 删除中间文件
            if (File.Exists(ClassPath.entireFileName))
            {
                File.Delete(ClassPath.entireFileName);
            }

            // 以下散列表、哈希表先置为空，在经过预处理之后会被填充
            insClassSequenceList.Clear();

            userUpperFeatureList.Clear();

            blatTitleNameList.Clear();
            blatNameChrHT.Clear();
            blatNameRecordHT.Clear();

            // 先对文件进行预处理
            trdPreTreatment = new Thread(new ThreadStart(Pretreatment));
            trdPreTreatment.IsBackground = true;
            Control.CheckForIllegalCrossThreadCalls = false;            // 取消线程安全保护模式
            isPreTreatmentThreadCreated = true;
            trdPreTreatment.Start();
            this.Text = "数据预处理中";
            trdPreTreatment.Join();                                     // 等待预处理线程的完成

            // 开始计算后以下表已无存在的意义，释放掉内存
            blatTitleNameList.Clear();
            blatNameChrHT.Clear();
            blatNameRecordHT.Clear();
            userUpperFeatureList.Clear();

            trdCompute = new Thread(new ThreadStart(StartToCompute));   // 创建一个计算线程
            trdCompute.IsBackground = true;                             // 后台运行
            isComputeThreadCreated = true;

            Stopwatch sw = new Stopwatch();

            sw.Start();

            trdCompute.Start();                                         // 开始执行线程
            this.Text = "计算中 ...";
            trdCompute.Join();
            sw.Stop();

            MessageBox.Show("计算完毕！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Text = "DNA 序列特征提取 V2.1";
            Output(sw.ElapsedMilliseconds);
        }

        // 清空结果 按钮对应的函数
        // 函数功能：把显示计算结果的文本框里的内容清空
        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                insClassSequenceList.Clear();

                rTxtBoxResult.Clear();
                rTxtBoxFeatureName.Clear();

                blatTitleNameList.Clear();
                blatNameChrHT.Clear();
                blatNameRecordHT.Clear();

                this.Text = "DNA 序列特征提取 V2.1";

                if (Directory.Exists(ClassPath.tempDirPath))
                {
                    Directory.Delete(ClassPath.tempDirPath, true);
                }

                if (File.Exists(ClassPath.entireFileName))
                {
                    File.Delete(ClassPath.entireFileName);
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
            string fileInfoMsg = string.Empty;

            if (!File.Exists(ClassPath.titleFileName))
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

                StringBuilder dataToWrite = new StringBuilder();

                foreach (ClassSequence insClassSequence in insClassSequenceList.SequenceList)
                {
                    dataToWrite.AppendLine(insClassSequence.Title.Name);
                }

                sw.Write(dataToWrite);
                sw.Flush();
                sw.Close();
            }

            // 保存特征的名字
            sfd.Title = "计算结果的特征名文件保存为";
            sfd.FileName = "resultFeature.txt";
            dr = sfd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                isAnySaved = true;
                fileInfoMsg += "特征名文件：\t" + sfd.FileName + "\n";

                StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.Default);

                sw.WriteLine(rTxtBoxFeatureName.Text);

                if (insClassSequenceList.UserRepeatFeatureList.Count > 0)
                {
                    sw.WriteLine("repeat features are:");
                    foreach (string feaToWrite in insClassSequenceList.UserRepeatFeatureList)
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

                sfd.AddExtension = true;
                sfd.CheckFileExists = false;
                sfd.Filter = "txt 文件(.txt)|*.txt|所有文件(*.*)|*.*";
                sfd.FilterIndex = 1;
                sfd.OverwritePrompt = true;
                sfd.RestoreDirectory = true;

                if (Directory.Exists(ClassPath.tempDirPath))
                {
                    Directory.Delete(ClassPath.tempDirPath, true);
                }

                if (File.Exists(ClassPath.entireFileName))
                {
                    File.Delete(ClassPath.entireFileName);
                }
            }
            catch (Exception ex)
            {
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

                if (!isSaved && rTxtBoxResult.Lines.Length > 0)
                {
                    DialogResult result = MessageBox.Show("计算结果尚未保存\n确定退出吗", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                if (Directory.Exists(ClassPath.tempDirPath))
                {
                    Directory.Delete(ClassPath.tempDirPath, true);
                }

                if (File.Exists(ClassPath.entireFileName))
                {
                    File.Delete(ClassPath.entireFileName);
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
                insClassSequenceList.Clear();

                rTxtBoxSequence.Clear();
                rTxtBoxFeature.Clear();
                rTxtBoxResult.Clear();
                rTxtBoxFeatureName.Clear();

                seqFileList.Clear();
                feaFileList.Clear();

                blatTitleNameList.Clear();
                blatNameChrHT.Clear();
                blatNameRecordHT.Clear();

                this.Text = "DNA 序列特征提取 V2.1";

                cBoxMod1.Checked = false;
                cBoxMod2.Checked = false;
                cBoxMod3.Checked = false;
                cBoxMod4.Checked = false;

                if (Directory.Exists(ClassPath.tempDirPath))
                {
                    Directory.Delete(ClassPath.tempDirPath, true);
                }

                if (File.Exists(ClassPath.entireFileName))
                {
                    File.Delete(ClassPath.entireFileName);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}