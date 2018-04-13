using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AlgorithmDemo.PackHelper;

namespace AlgorithmDemo.Entity
{
    public class PackPanel
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int ID;

        /// <summary>
        /// 长
        /// </summary>
        public float Length;

        /// <summary>
        /// 宽
        /// </summary>
        public float Width;

        /// <summary>
        /// 高
        /// </summary>
        public float High;

        /// <summary>
        /// 体积
        /// </summary>
        public float Volume { get { return Length * High * Width; } }

        /// <summary>
        /// 剩余空间
        /// </summary>
        public float RemainVolumn
        {
            get
            {
                if (GoodsList == null || GoodsList.Count <= 0) return Volume;
                var remain = Volume - GoodsList.Where(x => x != null).Sum(x => x.Volume);
                return remain < 0 ? 0 : remain;
            }
        }

        /// <summary>
        ///  1 装满，0 未装满
        /// </summary>
        public int Status = 0;

        /// <summary>
        /// 容器所装的商品集合
        /// </summary>
        public List<Goods> GoodsList = new List<Goods>();

        /// <summary>
        /// 子容器集合
        /// </summary>
        public List<ChildPackPanel> RemainPanelList;

        /// <summary>
        /// 商品摆放类型
        /// </summary>
        public PutType GoodPutType = PutType.Normal;

        /// <summary>
        /// 容器的零点坐标（通过赋值设置）
        /// </summary>
        public ZeroPosition PanelZeroPosition;

        /// <summary>
        /// 商品的摆放位置的零点坐标（根据摆放类型确定）
        /// </summary>
        public GoodsZeroPosition GoodsZeroPosition
        {
            get
            {
                if (this.Volume <= 0) return null;
                if (this.GoodsList == null || this.GoodsList.Count <= 0) return null;
                if (PanelZeroPosition == null) return null;

                GoodsZeroPosition zeroPosition = null;

                switch (GoodPutType)
                {
                    case PutType.Normal:
                        zeroPosition = GetNormalZeroPosition();
                        break;
                    case PutType.Normal_Right_Rotate_90:
                        zeroPosition = GetNormalRightRotate90ZeroPosition();
                        break;
                    case PutType.Normal_Left_Rotate_90:
                        zeroPosition = GetNormalLeftRotate90ZeroPosition();
                        break;
                    case PutType.Normal_Left_Rotate_Right_90:
                        zeroPosition = GetNormalLeftRotateRight90ZeroPosition();
                        break;
                    default:
                        break;
                }

                return zeroPosition;
            }
        }

        /// <summary>
        /// 商品不介意放倒（平躺）的话，在商品处于第三种方案摆放姿态的情况下，向右旋转90度, 获取商品零点坐标
        /// </summary>
        /// <returns></returns>
        private GoodsZeroPosition GetNormalLeftRotateRight90ZeroPosition()
        {
            GoodsZeroPosition zeroPosition = new GoodsZeroPosition();

            zeroPosition.X = PanelZeroPosition.X;
            zeroPosition.Y = PanelZeroPosition.Y;
            zeroPosition.Z = PanelZeroPosition.Z;

            zeroPosition.Length = GoodsList[0].Width;
            zeroPosition.Width = GoodsList[0].Hight;
            zeroPosition.Hight = GoodsList[0].Length;

            return zeroPosition;
        }

        /// <summary>
        /// 商品不介意放倒（平躺）的话，在商品正常摆放（正面朝上）情况下，向左倾斜90度, 获取商品的零点坐标
        /// </summary>
        /// <returns></returns>
        private GoodsZeroPosition GetNormalLeftRotate90ZeroPosition()
        {
            GoodsZeroPosition zeroPosition = new GoodsZeroPosition();

            zeroPosition.X = PanelZeroPosition.X;
            zeroPosition.Y = PanelZeroPosition.Y;
            zeroPosition.Z = PanelZeroPosition.Z;

            zeroPosition.Length = GoodsList[0].Hight;
            zeroPosition.Width = GoodsList[0].Width;
            zeroPosition.Hight = GoodsList[0].Length;

            return zeroPosition;
        }

        /// <summary>
        /// 商品正常摆放（正面朝上）情况下，横向向右旋转90度得到,获取商品零点坐标
        /// </summary>
        /// <returns></returns>
        private GoodsZeroPosition GetNormalRightRotate90ZeroPosition()
        {
            GoodsZeroPosition zeroPosition = new GoodsZeroPosition();

            zeroPosition.X = PanelZeroPosition.X;
            zeroPosition.Y = PanelZeroPosition.Y;
            zeroPosition.Z = PanelZeroPosition.Z;

            zeroPosition.Length = GoodsList[0].Width;
            zeroPosition.Width = GoodsList[0].Length;
            zeroPosition.Hight = GoodsList[0].Hight;

            return zeroPosition;
        }

        /// <summary>
        /// 获取Normal类型的商品摆放的零点坐标
        /// </summary>
        /// <returns></returns>
        private GoodsZeroPosition GetNormalZeroPosition()
        {
            GoodsZeroPosition zeroPosition = new GoodsZeroPosition();

            zeroPosition.X = PanelZeroPosition.X;
            zeroPosition.Y = PanelZeroPosition.Y;
            zeroPosition.Z = PanelZeroPosition.Z;

            zeroPosition.Length = GoodsList[0].Length;
            zeroPosition.Width = GoodsList[0].Width;
            zeroPosition.Hight = GoodsList[0].Hight;

            return zeroPosition;
        }
    }
}
