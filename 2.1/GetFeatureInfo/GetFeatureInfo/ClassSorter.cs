using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetFeatureInfo
{
    class ClassSorter
    {
        // 把串起来的序列按各个序列的输入顺序排序
        // 其实就是还原
        public static void sortByIndex(ClassSequenceList insClassSequenceList)
        {
            int i, j;
            ClassSequence temp;
            bool exchange;

            for (i = 0; i < insClassSequenceList.SequenceList.Count; ++i)
            {
                exchange = false;

                for (j = insClassSequenceList.SequenceList.Count - 2; j >= i; --j)
                {
                    if (((ClassSequence)insClassSequenceList.SequenceList[j + 1]).Index < ((ClassSequence)insClassSequenceList.SequenceList[j]).Index)
                    {
                        temp = (ClassSequence)insClassSequenceList.SequenceList[j + 1];
                        insClassSequenceList.SequenceList[j + 1] = insClassSequenceList.SequenceList[j];
                        insClassSequenceList.SequenceList[j] = temp;

                        exchange = true;
                    }
                }

                // 没发生交换，证明已排序完毕，提前结束循环
                if (!exchange)
                {
                    break;
                }
            }
        }

        // 把串起来的序列按染色体的字典序排序
        public static void sortByChr(ClassSequenceList insClassSequenceList)
        {
            int i, j;
            ClassSequence temp;
            bool exchange;

            for (i = 0; i < insClassSequenceList.SequenceList.Count; ++i)
            {
                exchange = false;

                for (j = insClassSequenceList.SequenceList.Count - 2; j >= i; --j)
                {
                    if (string.Compare(((ClassSequence)insClassSequenceList.SequenceList[j + 1]).Title.Chr, ((ClassSequence)insClassSequenceList.SequenceList[j]).Title.Chr, true) == -1)
                    {
                        temp = (ClassSequence)insClassSequenceList.SequenceList[j + 1];
                        insClassSequenceList.SequenceList[j + 1] = insClassSequenceList.SequenceList[j];
                        insClassSequenceList.SequenceList[j] = temp;

                        exchange = true;
                    }
                }

                // 没发生交换，证明已排序完毕，提前结束循环
                if (!exchange)
                {
                    break;
                }
            }
        }
    }
}
