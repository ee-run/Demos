using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDemo.Entity
{
    /// <summary>
    /// 创建箱子的命令
    /// </summary>
    [Serializable]
    [DataContract]
    public class CreateBoxEntity
    {
        /// <summary>
        /// 箱子的GUID
        /// </summary>
        [DataMember]
        public string guid { get; set; }

        /// <summary>
        /// 箱子的颜色
        /// </summary>
        [DataMember]
        public Color color { get; set; }

        /// <summary>
        /// 长
        /// </summary>
        [DataMember]
        public float lenght { get; set; }

        /// <summary>
        /// 宽
        /// </summary>
        [DataMember]
        public float width { get; set; }

        /// <summary>
        /// 高
        /// </summary>
        [DataMember]
        public float height { get; set; }

        /// <summary>
        /// 目标点的x坐标
        /// </summary>
        [DataMember]
        public float x { get; set; }

        /// <summary>
        /// 目标点的y坐标
        /// </summary>
        [DataMember]
        public float y { get; set; }

        /// <summary>
        /// 目标点的z坐标
        /// </summary>
        [DataMember]
        public float z { get; set; }

        /// <summary>
        /// 是否成功摆放
        /// </summary>
        [DataMember]
        public bool IsPlaced { get; set; }
    }
}
