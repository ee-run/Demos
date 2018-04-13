using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace WebServiceDemo
{
    /// <summary>
    /// CheckLicense 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class CheckLicense : System.Web.Services.WebService
    {
        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        /// <summary>
        /// 验证客户的注册码
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public bool CheckCustomerLicence(string sCustomerCode, string sLicenceCode, ref string sMsg)
        {
            List<string> lstCustomerCode = Data.DataDemo.CustomerCodeList;
            if (lstCustomerCode == null || lstCustomerCode.Count <= 0)
            {
                sMsg = string.Format("获取客户数据失败！");
                return false;
            }

            if (!lstCustomerCode.Contains(sCustomerCode))
            {
                sMsg = string.Format("客户不存在！");
                return false;
            }

            // 没有验证码，直接返回false
            if (!Data.DataDemo.DicCustomerLicence.Keys.Contains(sCustomerCode))
            {
                sMsg = string.Format("客户未注册！");
                return false;
            }

            if (string.IsNullOrWhiteSpace(sLicenceCode))
            {
                // 验证是否有license
                string sCustomerLicense = Data.DataDemo.DicCustomerLicence[sCustomerCode];

                if (!Data.DataDemo.PowerfullLicences.Contains(sCustomerLicense))
                {
                    sMsg = string.Format("注册码已经失效！");
                    return false;
                }

                sMsg = string.Format("验证成功11！");
                return true;
            }

            // 有验证码，则验证licence是否有效
            if (!Data.DataDemo.PowerfullLicences.Contains(sLicenceCode))
            {
                sMsg = string.Format("注册码无效！");
                return false;
            }

            sMsg = string.Format("验证成功22！");
            return true;
        }
    }
}
