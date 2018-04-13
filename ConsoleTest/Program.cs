using ConsoleTest.CheckLicenseService;
using ConsoleTest.WcfServiceDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string sCustomerCode = "QHY";
            string sMsg = string.Empty;
            string sLicenseCode = string.Empty; //"A3";

            //CheckLicenseSoapClient client = new CheckLicenseSoapClient();
            //bool isCheckPass = client.CheckCustomerLicence(sCustomerCode, sLicenseCode, ref sMsg);

            DemoInterfaceClient client = new DemoInterfaceClient();
            bool isCheckPass = client.CheckCustomerLicence(sCustomerCode, sLicenseCode, ref sMsg);

            Console.WriteLine(sMsg);
            Console.ReadLine();
        }
    }
}
