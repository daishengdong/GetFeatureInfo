using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GetFeatureInfo
{
    class ClassPath
    {
        // 系统的相关路径
        public static string rootPath = Application.StartupPath;           // 程序所在根目录
        public static string tempDirPath = rootPath + @"\temp\";           // 临时文件存放目录
        public static string featurePath = rootPath + @"\feature\";        // 特征文件存放目录
        public static string blatPath = rootPath + @"\blat\";              // blat 存放目录
        public static string titleFileName = tempDirPath + "titleFile";    // 存放标题的临时文件
        public static string seqFileName = tempDirPath + "seqFile";        // 存放序列本身的临时文件
        public static string feaFileName = tempDirPath + "feature";        // 存放特征的临时文件

        public static string pathOfBlat = blatPath + @"blat.exe";              // blat 可执行文件路径
        public static string tempOutFileName = tempDirPath + @"tempOut.psl";   // 存放临时输出索引的文件
        public static string entireFileName = rootPath + @"\entireFile.fa";    // 存放全部信息的临时文件

        // 特征计算所需的文件
        public static string pathOfEponine = featurePath + @"tfbsConsSites\eponine.txt";               // 存放 eponine 特征计算的文件
        public static string pathOftfbsConsSites = featurePath + @"tfbsConsSites\tfbsConsSites.txt";   // 存放 tfbsConsSites 特征计算的文件
        public static string pathOfRmskFolder = featurePath + @"\rmsk\";
        public static string pathOfHistoneModification18NatFolder = featurePath + @"histone modification\2008-nat ge-Combinatorial patterns of histone\";
        public static string pathOfHistoneModification20CellFolder = featurePath + @"histone modification\2008-cell-High-resolution profiling of histone\";
    }
}