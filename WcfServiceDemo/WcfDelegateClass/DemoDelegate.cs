using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WcfServiceDemo.WcfCore;
using WcfServiceDemo.WcfInterface;

namespace WcfServiceDemo.WcfDelegateClass
{
    public class DemoDelegate : IDemoInterface
    {
        public bool CheckCustomerLicence(string sCustomerCode, string sLicenceCode, ref string sMsg)
        {
            DemoCore core = new DemoCore();
            return core.CheckCustomerLicence(sCustomerCode, sLicenceCode, ref sMsg);
        }
    }
}