using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmDemo.Entity
{
    /// <summary>
    /// 用于传输网络命令的实体
    /// </summary>
    [Serializable]
    [DataContract]
    public class NetWorkCommandEntity
    {
        /// <summary>
        /// 命令名称（指请求的API）
        /// </summary>
        [DataMember]
        public string CommandName { get; set; }

        /// <summary>
        /// 指请求的内容，需要转换为其他实体
        /// </summary>
        [DataMember]
        public string Context { get; set; }
    }
}
