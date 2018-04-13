using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WcfServiceDemo.WcfInterface
{
    [ServiceContract]
    interface IDemoInterface
    {
        [OperationContract]
        bool CheckCustomerLicence(string sCustomerCode, string sLicenceCode, ref string sMsg);
    }
}
