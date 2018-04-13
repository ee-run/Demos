using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDemo.Entity
{
    public class Goods
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int ID;

        /// <summary>
        /// 商品单号
        /// </summary>
        public string HawbCode;

        /// <summary>
        /// 长度
        /// </summary>
        public float Length;

        /// <summary>
        /// 宽度
        /// </summary>
        public float Width;

        /// <summary>
        /// 高
        /// </summary>
        public float Hight;

        /// <summary>
        /// 体积
        /// </summary>
        public float Volume { get { return Length * Hight * Width; } }

        /// <summary>
        /// 是否已经装进箱子
        /// </summary>
        public bool isPacked = false;

        /// <summary>
        /// 能否装进容器
        /// </summary>
        public bool isCanPack = true;

        /// <summary>
        /// 零点位置
        /// </summary>
        public GoodsZeroPosition GoodZeroPosition;
    }
}
