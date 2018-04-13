using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDemo.Entity
{
    /// <summary>
    /// 商品位置零点坐标对象
    /// </summary>
    public class GoodsZeroPosition : ZeroPosition
    {
        /// <summary>
        /// 正对面对应的长（非排序后）
        /// </summary>
        public float Length;

        /// <summary>
        /// 正对面对应的宽（非排序后）
        /// </summary>
        public float Width;

        /// <summary>
        /// 正对面对应的高（非排序后）
        /// </summary>
        public float Hight;
    }
}
