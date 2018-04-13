using AlgorithmDemo.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDemo
{
    public static class PackHelper
    {
        /// <summary>
        /// 物品的摆放方式
        /// </summary>
        public enum PutType
        {
            /// <summary>
            /// 第一种：常规摆放，长对长，宽对宽，高对高
            /// </summary>
            Normal = 0,

            /// <summary>
            /// 第二种：商品正常摆放（正面朝上）情况下，横向向右旋转90度得到
            /// </summary>
            Normal_Right_Rotate_90 = 1,

            /// <summary>
            /// 第三种：商品不介意放倒（平躺）的话，在商品正常摆放（正面朝上）情况下，向左倾斜90度
            /// </summary>
            Normal_Left_Rotate_90 = 2,

            /// <summary>
            /// 第四种：商品不介意放倒（平躺）的话，在商品处于第三种方案摆放姿态的情况下，向右旋转90度
            /// </summary>
            Normal_Left_Rotate_Right_90 = 3
        }

        private static List<Box> mboxList = new List<Box>();
        /// <summary>
        /// 装箱，返回箱子集合（每次装箱子，准备一个商品，一个'存放容器'（待存放商品的容器空间））
        /// </summary>
        /// <param name="goodsList"></param>
        /// <param name="boxList"></param>
        /// <returns></returns>
        public static bool Pack2Boxes(List<Goods> goodsList, ref List<Box> boxList, PutType PutType)
        {
            mboxList = boxList;

            // 先按长度大小从大到小排序，先装大件商品
            //SortVolume(ref goodsList);

            if (goodsList == null || goodsList.Count <= 0) return false;

            //  1. 根据商品去找剩余容器空间
            //return PackByYield(goodsList, ref boxList);

            //  2. 根据容器去拿取商品，匹配和容器容积大小最接近的商品
            return PackByRecursion(ref goodsList, ref boxList, PutType);
        }

        /// <summary>
        /// 递归，按空间去找商品方式
        /// </summary>
        /// <param name="goodsList"></param>
        /// <param name="boxList"></param>
        /// <returns></returns>
        private static bool PackByRecursion(ref List<Goods> goodsList, ref List<Box> boxList, PutType PutType)
        {
            Box box = CreateBox(2, 2, 2);
            boxList.Add(box);

            bool bPackSuccess = PackGoods2Panel(ref goodsList, box.BoxPackPanel, PutType);
            goodsList.RemoveAll(x => !x.isCanPack);

            if (goodsList != null && goodsList.Where(x => x.isPacked == false).ToList().Count > 0)
            {
                Pack2Boxes(goodsList, ref boxList, PutType);
            }

            return bPackSuccess;
        }

        /// <summary>
        /// 迭代计算
        /// </summary>
        /// <param name="goodsList"></param>
        /// <param name="boxList"></param>
        /// <returns></returns>
        private static bool PackByYield(List<Goods> goodsList, ref List<Box> boxList)
        {
            Goods good = null;
            Box box = GetCanPackBox();

            while (goodsList.Count > 0)
            {
                for (int i = 0; i < goodsList.Count; i++)
                {
                    good = goodsList[i];

                    // 根据商品去找剩余可用容器空间
                    if (box.Status == 0 && box.BoxPackPanel.RemainVolumn >= good.Volume)
                    {
                        box.BoxPackPanel.GoodsList.Add(good);
                        goodsList.Remove(good);
                        i = 0;
                    }
                    else
                    {
                        box.Status = 1;
                        boxList.Add(box);
                        box = GetCanPackBox();
                    }
                }
            }

            if (box != null && box.BoxPackPanel != null && box.BoxPackPanel.GoodsList != null && box.BoxPackPanel.GoodsList.Count > 0)
            {
                boxList.Add(box);

            }

            return true;
        }

        /// <summary>
        /// 获取当前未装满的箱子
        /// </summary>
        /// <returns></returns>
        private static Box GetCanPackBox()
        {
            if (mboxList == null || mboxList.Count <= 0)
                return CreateBox(2, 2, 2);

            var box = mboxList.Where(x => x.Status == 0).FirstOrDefault();
            return box == null ? CreateBox(2, 2, 2) : box;
        }

        /// <summary>
        /// 过滤不能装入大箱子的商品
        /// </summary>
        /// <param name="goodsList"></param>
        public static void FilterGoodsList(ref List<Goods> goodsList, PackPanel panel)
        {
            if (goodsList == null || goodsList.Count <= 0 || panel == null) return;

            foreach (var item in goodsList)
            {
                if (CheckGoodsPackIn(item, panel)) continue;

                item.isCanPack = false;
            }

            //goodsList = goodsList.Where(x => x.isCanPack).ToList();
        }

        /// <summary>
        /// 将所有商品集合初始化为可装箱
        /// </summary>
        /// <param name="goodsList"></param>
        public static void SetGoodsUnPacked(List<Goods> goodsList)
        {
            if (goodsList == null || goodsList.Count <= 0) return;

            foreach (var good in goodsList)
            {
                good.isPacked = false;
            }
        }

        /// <summary>
        /// 将商品放入容器中
        /// </summary>
        /// <param name="goods"></param>
        /// <param name="packPanel"></param>
        /// <returns></returns>
        private static bool PackGoods2Panel(ref List<Goods> goodsList, PackPanel packPanel, PutType PutType)
        {
            if (packPanel == null) return false;

            if (packPanel.Volume <= 0) return false;

            if (goodsList == null || goodsList.Count <= 0) return false;

            Goods suitGoods = GetMostSuitGoodsByPanel(ref packPanel, goodsList);

            if (suitGoods == null || !suitGoods.isCanPack)
            {
                // 没有找到匹配的商品或容器放不下该商品
                packPanel.Status = 1;
                return false;
            }

            // 找到合适的商品，则将该商品放入该容器, 同时将该商品从集合中移除
            packPanel.GoodsList.Add(suitGoods);
            //goodsList.Remove(suitGoods);
            suitGoods.isPacked = true;

            // 计算该容器的剩余空间（摆放问题，需考虑四种剩余空间方案）
            packPanel.RemainPanelList = CalculateRemainPanelByPanel(packPanel, PutType);

            // 计算零点位置
            suitGoods.GoodZeroPosition = packPanel.GoodsZeroPosition;

            if (packPanel.RemainPanelList == null || packPanel.RemainPanelList.Count <= 0) return true;

            // 如果存在超出范围，则该方案不可用
            if (packPanel.RemainPanelList.Where(x => x.Volume < 0).Count() > 0) return true;

            // 获取剩余商品中体积最小的商品体积
            Goods minGoods = GetMinGoodsVolumeByGoodsList(goodsList);
            if (minGoods == null || minGoods.Volume <= 0) return true;

            // 获取剩余空间中最大的容器体积
            float dMaxRemainPanelVolume = GetMaxRemainPanelByPanelList(packPanel.RemainPanelList);

            // 验证容器的剩余空间是否可以存放最小体积的商品，如果放不下，则返回
            if (dMaxRemainPanelVolume < minGoods.Volume) return true;

            // 递归遍历剩余空间
            foreach (var remainPanel in packPanel.RemainPanelList)
            {
                PackGoods2Panel(ref goodsList, remainPanel, PutType);
            }

            return true;
        }

        /// <summary>
        /// 获取剩余空间中最大的容器体积
        /// </summary>
        /// <param name="remainPanelList"></param>
        /// <returns></returns>
        private static float GetMaxRemainPanelByPanelList(List<ChildPackPanel> remainPanelList)
        {
            if (remainPanelList == null || remainPanelList.Count <= 0) return -1f;

            float dMax = -1f;

            //foreach (var item in remainPanelList)
            //{
            //    if (item.Volume > dMax)
            //    {
            //        dMax = item.Volume;
            //    }
            //}

            dMax = remainPanelList.Max(x => x.Volume);

            return dMax;
        }

        /// <summary>
        /// 计算该容器的剩余空间, 每装入一次，则会产生三个子容器，有四种方案
        /// </summary>
        /// <param name="packPanel"></param>
        /// <returns></returns>
        private static List<ChildPackPanel> CalculateRemainPanelByPanel(PackPanel packPanel, PutType putType)
        {
            if (packPanel == null || packPanel.Volume <= 0) return null;

            if (packPanel.GoodsList == null || packPanel.GoodsList.Count <= 0) return null;

            List<ChildPackPanel> childPackPanelList = null;

            // 计算一种没有超出范围的摆放方式
            int errCount = 0;

            // 尝试第一种摆放方案
            childPackPanelList = CalculateNormal(packPanel);
            errCount = childPackPanelList.Where(x => x.Length < 0 || x.Width < 0 || x.High < 0).ToList().Count;

            if (errCount == 0)
            {
                packPanel.GoodPutType = PutType.Normal;
                return childPackPanelList;
            }

            // 尝试用第二种摆放方案
            childPackPanelList = CalculateNormalRightRotate(packPanel);
            errCount = childPackPanelList.Where(x => x.Length < 0 || x.Width < 0 || x.High < 0).ToList().Count;
            if (errCount == 0)
            {
                packPanel.GoodPutType = PutType.Normal_Right_Rotate_90;
                return childPackPanelList;
            }

            // 尝试第三种摆放方案
            childPackPanelList = CalculateNormalLeftRotate(packPanel);
            errCount = childPackPanelList.Where(x => x.Length < 0 || x.Width < 0 || x.High < 0).ToList().Count;
            if (errCount == 0)
            {
                packPanel.GoodPutType = PutType.Normal_Left_Rotate_90;
                return childPackPanelList;
            }

            // 尝试第四种摆放方案
            childPackPanelList = CalculateNormalLeftRotateRight(packPanel);
            errCount = childPackPanelList.Where(x => x.Length < 0 || x.Width < 0 || x.High < 0).ToList().Count;
            if (errCount == 0)
            {
                packPanel.GoodPutType = PutType.Normal_Left_Rotate_Right_90;
                return childPackPanelList;
            }

            //switch (putType)
            //{
            //    case PutType.Normal:
            //        childPackPanelList = CalculateNormal(packPanel);
            //        break;
            //    case PutType.Normal_Right_Rotate_90:
            //        childPackPanelList = CalculateNormalRightRotate(packPanel);
            //        break;
            //    case PutType.Normal_Left_Rotate_90:
            //        childPackPanelList = CalculateNormalLeftRotate(packPanel);
            //        break;
            //    case PutType.Normal_Left_Rotate_Right_90:

            //        break;
            //    default:
            //        break;
            //}

            return childPackPanelList;
        }

        /// <summary>
        /// 商品不介意放倒（平躺）的话，在商品处于第三种方案摆放姿态的情况下，向右旋转90度,剩余空间
        /// </summary>
        /// <param name="packPanel"></param>
        /// <returns></returns>
        private static List<ChildPackPanel> CalculateNormalLeftRotateRight(PackPanel packPanel)
        {
            List<ChildPackPanel> remainPackPanelList = new List<ChildPackPanel>();
            Goods good = packPanel.GoodsList[0];

            ChildPackPanel panel1 = new ChildPackPanel();
            panel1.Length = good.Width;
            panel1.Width = good.Hight;
            panel1.High = packPanel.High - good.Length;
            panel1.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X,
                Y = packPanel.PanelZeroPosition.Y + good.Length,
                Z = packPanel.PanelZeroPosition.Z
            };

            ChildPackPanel panel2 = new ChildPackPanel();
            panel2.Length = good.Width;
            panel2.Width = packPanel.Width - good.Hight;
            panel2.High = packPanel.High;
            panel2.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X,
                Y = packPanel.PanelZeroPosition.Y,
                Z = packPanel.PanelZeroPosition.Z + good.Hight
            };

            ChildPackPanel panel3 = new ChildPackPanel();
            panel3.Length = packPanel.Length - good.Width;
            panel3.Width = packPanel.Width;
            panel3.High = packPanel.High;
            panel3.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X + good.Width,
                Y = packPanel.PanelZeroPosition.Y,
                Z = packPanel.PanelZeroPosition.Z
            };

            remainPackPanelList.Add(panel1);
            remainPackPanelList.Add(panel2);
            remainPackPanelList.Add(panel3);

            //return remainPackPanelList.Where(x => x.Volume > 0).ToList();
            return remainPackPanelList;
        }

        /// <summary>
        /// 商品不介意放倒（平躺）的话，在商品正常摆放（正面朝上）情况下，向左倾斜90度,剩余空间
        /// </summary>
        /// <param name="packPanel"></param>
        /// <returns></returns>
        private static List<ChildPackPanel> CalculateNormalLeftRotate(PackPanel packPanel)
        {
            List<ChildPackPanel> remainPackPanelList = new List<ChildPackPanel>();
            Goods good = packPanel.GoodsList[0];

            ChildPackPanel panel1 = new ChildPackPanel();
            panel1.Length = good.Length;
            panel1.Width = good.Width;
            panel1.High = packPanel.High - good.Hight;
            panel1.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X,
                Y = packPanel.PanelZeroPosition.Y + good.Length,
                Z = packPanel.PanelZeroPosition.Z
            };

            ChildPackPanel panel2 = new ChildPackPanel();
            panel2.Length = good.Length;
            panel2.Width = packPanel.Width - good.Width;
            panel2.High = packPanel.High;
            panel2.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X,
                Y = packPanel.PanelZeroPosition.Y,
                Z = packPanel.PanelZeroPosition.Z + good.Width
            };

            ChildPackPanel panel3 = new ChildPackPanel();
            panel3.Length = packPanel.Length - good.Length;
            panel3.Width = packPanel.Width;
            panel3.High = packPanel.High; panel2.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X + good.Hight,
                Y = packPanel.PanelZeroPosition.Y,
                Z = packPanel.PanelZeroPosition.Z
            };

            remainPackPanelList.Add(panel1);
            remainPackPanelList.Add(panel2);
            remainPackPanelList.Add(panel3);

            return remainPackPanelList;
        }

        /// <summary>
        /// 商品正常摆放（正面朝上）情况下，横向向右旋转90度得到的剩余空间
        /// </summary>
        /// <param name="packPanel"></param>
        /// <returns></returns>
        private static List<ChildPackPanel> CalculateNormalRightRotate(PackPanel packPanel)
        {
            List<ChildPackPanel> remainPackPanelList = new List<ChildPackPanel>();
            Goods good = packPanel.GoodsList[0];

            ChildPackPanel panel1 = new ChildPackPanel();
            panel1.Length = good.Width;
            panel1.Width = good.Length;
            panel1.High = packPanel.High - good.Hight;
            panel1.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X,
                Y = packPanel.PanelZeroPosition.Y + good.Hight,
                Z = packPanel.PanelZeroPosition.Z
            };

            ChildPackPanel panel2 = new ChildPackPanel();
            panel2.Length = good.Width;
            panel2.Width = packPanel.Width - good.Length;
            panel2.High = packPanel.Width - good.Length;
            panel2.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X,
                Y = packPanel.PanelZeroPosition.Y,
                Z = packPanel.PanelZeroPosition.Z + good.Length
            };

            ChildPackPanel panel3 = new ChildPackPanel();
            panel3.Length = packPanel.Length - good.Width;
            panel3.Width = packPanel.Width;
            panel3.High = packPanel.High;
            panel3.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X + good.Width,
                Y = packPanel.PanelZeroPosition.Y,
                Z = packPanel.PanelZeroPosition.Z
            };

            remainPackPanelList.Add(panel1);
            remainPackPanelList.Add(panel2);
            remainPackPanelList.Add(panel3);

            return remainPackPanelList;
        }

        /// <summary>
        /// 计算正常摆放的剩余空间
        /// </summary>
        /// <param name="packPanel"></param>
        /// <returns></returns>
        private static List<ChildPackPanel> CalculateNormal(PackPanel packPanel)
        {
            List<ChildPackPanel> remainPackPanelList = new List<ChildPackPanel>();
            Goods good = packPanel.GoodsList[0];

            ChildPackPanel panel1 = new ChildPackPanel();
            panel1.Length = good.Length;
            panel1.Width = good.Width;
            panel1.High = packPanel.High - good.Hight;
            panel1.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X,
                Y = packPanel.PanelZeroPosition.Y + good.Hight,
                Z = packPanel.PanelZeroPosition.Z
            };

            ChildPackPanel panel2 = new ChildPackPanel();
            panel2.Length = good.Length;
            panel2.Width = packPanel.Width - good.Width;
            panel2.High = packPanel.High;
            panel2.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X,
                Y = packPanel.PanelZeroPosition.Y,
                Z = packPanel.PanelZeroPosition.Z + good.Width
            };

            ChildPackPanel panel3 = new ChildPackPanel();
            panel3.Length = packPanel.Length - good.Length;
            panel3.Width = packPanel.Width;
            panel3.High = packPanel.High;
            panel3.PanelZeroPosition = new ZeroPosition()
            {
                X = packPanel.PanelZeroPosition.X + good.Length,
                Y = packPanel.PanelZeroPosition.Y,
                Z = packPanel.PanelZeroPosition.Z
            };

            remainPackPanelList.Add(panel1);
            remainPackPanelList.Add(panel2);
            remainPackPanelList.Add(panel3);

            return remainPackPanelList;
        }

        /// <summary>
        /// 获取剩余商品中体积最小的商品体积
        /// </summary>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        private static Goods GetMinGoodsVolumeByGoodsList(List<Goods> goodsList)
        {
            if (goodsList == null || goodsList.Count <= 0) return null;
            Goods good = null;

            float dMinVolumn = goodsList.Min(x => x.Volume);
            good = goodsList.Where(x => x.Volume == dMinVolumn).FirstOrDefault();

            //float dMinVolumn = float.MaxValue;
            //foreach (var item in goodsList)
            //{
            //    if (item.Volume < dMinVolumn)
            //    {
            //        dMinVolumn = item.Volume;
            //        good = item;
            //    }
            //}

            return good;
        }

        /// <summary>
        ///  创建箱子并初始化箱子
        /// </summary>
        /// <returns></returns>
        public static Box CreateBox(float dLength, float dWidth, float dHeight)
        {
            Box box = new Box();
            box.Length = dLength;
            box.High = dHeight;
            box.Width = dWidth;

            PackPanel packPanel = new PackPanel();
            packPanel.Length = box.Length;
            packPanel.High = box.High;
            packPanel.Width = box.Width;
            box.BoxPackPanel = packPanel;

            packPanel.RemainPanelList = new List<ChildPackPanel>()
            {
              new ChildPackPanel ()
              {
                  Length = box.Length,
                  Width = box.Width,
                  High = box.High
              }
            };

            packPanel.PanelZeroPosition = new ZeroPosition()
            {
                X = 0,
                Y = 0,
                Z = 0
            };

            return box;
        }

        /// <summary>
        /// 根据容器去拿取商品，匹配和容器容积大小最接近的商品
        /// </summary>
        /// <param name="packPanel"></param>
        /// <param name="goodsList"></param>
        /// <returns></returns>
        private static Goods GetMostSuitGoodsByPanel(ref PackPanel packPanel, List<Goods> goodsList)
        {
            if (packPanel == null || packPanel.Volume <= 0 || goodsList == null || goodsList.Count <= 0) return null;

            // 计算各个商品与容器的提交差
            float dDiffVolume = 0;
            float dMinDiffVolume = float.MaxValue;
            Goods suitGoods = null;

            foreach (var item in goodsList)
            {
                if (item.isPacked) continue;
                // 验证该商品是否能放的下容器,即能否匹配该容器
                if (!CheckGoodsPackIn(item, packPanel)) continue;

                dDiffVolume = packPanel.Volume - item.Volume;
                if (dDiffVolume < 0) continue; // 商品体积大于待存放容器体积不取

                if (dDiffVolume < dMinDiffVolume)
                {
                    dMinDiffVolume = dDiffVolume;
                    suitGoods = item;
                }
            }

            return suitGoods;
        }

        /// <summary>
        /// 验证该商品是否能放的下容器
        /// </summary>
        /// <param name="item"></param>
        /// <param name="packPanel"></param>
        /// <returns></returns>
        private static bool CheckGoodsPackIn(Goods item, PackPanel packPanel)
        {
            if (item == null || packPanel == null) return false;

            List<float> goodWHL = new List<float>() { item.Width, item.Hight, item.Length };
            List<float> panelWHL = new List<float>() { packPanel.Width, packPanel.High, packPanel.Length };

            // 分别排序
            Sort(goodWHL);
            Sort(panelWHL);

            for (int i = 0; i < goodWHL.Count; i++)
            {
                if (panelWHL[i] < goodWHL[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="list"></param>
        public static void Sort(List<float> list)
        {
            if (list == null || list.Count <= 0) return;

            float temp;
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[i] > list[j])
                    {
                        temp = list[i];
                        list[i] = list[j];
                        list[j] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// 获取商品集合
        /// </summary>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static List<Goods> GetGoodsList(int Count = 1)
        {
            List<Goods> goodsList = new List<Goods>();
            Random rand = new Random();

            for (int i = 0; i < Count; i++)
            {
                Goods goods = new Goods();
                goods.HawbCode = Guid.NewGuid().ToString().Substring(0, 16).Replace("-", string.Empty).ToUpper();
                goods.ID = i + 1;

                //var temp1 = rand.Next(1, 3);
                //var temp2 = rand.Next(1, 5);
                //var temp3 = rand.Next(1, 6);
                var temp1 = 1;
                var temp2 = 1;
                var temp3 = 1;

                List<float> valueList = new List<float>() { temp1, temp2, temp3 };

                // 从大到小排序
                Sort(valueList);

                // 定义： 最大的是长，第二是宽 ，最短的是高
                goods.Length = valueList[0];
                goods.Width = valueList[1];
                goods.Hight = valueList[2];

                goodsList.Add(goods);
            }

            return goodsList;
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="goodsList"></param>
        public static void SortVolume(ref List<Goods> goodsList)
        {
            if (goodsList == null || goodsList.Count <= 0) return;

            // 先按长度降序排序，再按体积降序排序
            goodsList = goodsList.OrderByDescending(x => x.Length).ThenByDescending(x => x.Volume).ToList();
        }

    }
}

