using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServiceDemo.Data
{
    public static class DataDemo
    {
        /// <summary>
        /// 现有已经注册的客户集合
        /// </summary>
        public static List<string> CustomerCodeList = new List<string>() { "QHY", "EC" };

        /// <summary>
        /// 现有可用的license集合
        /// </summary>
        public static List<string> PowerfullLicences = new List<string>() { "A1", "A2", "B2", "A3" };

        /// <summary>
        /// 注册客户以及对应的license字典集合
        /// </summary>
        public static Dictionary<string, string> DicCustomerLicence = new Dictionary<string, string>()
        {
            { "QHY","A3"},
            { "EC","A1"}
        };
    }
}