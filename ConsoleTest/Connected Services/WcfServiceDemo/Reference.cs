﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConsoleTest.WcfServiceDemo {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="WcfServiceDemo.IDemoInterface")]
    public interface IDemoInterface {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDemoInterface/CheckCustomerLicence", ReplyAction="http://tempuri.org/IDemoInterface/CheckCustomerLicenceResponse")]
        ConsoleTest.WcfServiceDemo.CheckCustomerLicenceResponse CheckCustomerLicence(ConsoleTest.WcfServiceDemo.CheckCustomerLicenceRequest request);
        
        // CODEGEN: 正在生成消息协定，应为该操作具有多个返回值。
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDemoInterface/CheckCustomerLicence", ReplyAction="http://tempuri.org/IDemoInterface/CheckCustomerLicenceResponse")]
        System.Threading.Tasks.Task<ConsoleTest.WcfServiceDemo.CheckCustomerLicenceResponse> CheckCustomerLicenceAsync(ConsoleTest.WcfServiceDemo.CheckCustomerLicenceRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="CheckCustomerLicence", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class CheckCustomerLicenceRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public string sCustomerCode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public string sLicenceCode;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=2)]
        public string sMsg;
        
        public CheckCustomerLicenceRequest() {
        }
        
        public CheckCustomerLicenceRequest(string sCustomerCode, string sLicenceCode, string sMsg) {
            this.sCustomerCode = sCustomerCode;
            this.sLicenceCode = sLicenceCode;
            this.sMsg = sMsg;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="CheckCustomerLicenceResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class CheckCustomerLicenceResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public bool CheckCustomerLicenceResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public string sMsg;
        
        public CheckCustomerLicenceResponse() {
        }
        
        public CheckCustomerLicenceResponse(bool CheckCustomerLicenceResult, string sMsg) {
            this.CheckCustomerLicenceResult = CheckCustomerLicenceResult;
            this.sMsg = sMsg;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDemoInterfaceChannel : ConsoleTest.WcfServiceDemo.IDemoInterface, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DemoInterfaceClient : System.ServiceModel.ClientBase<ConsoleTest.WcfServiceDemo.IDemoInterface>, ConsoleTest.WcfServiceDemo.IDemoInterface {
        
        public DemoInterfaceClient() {
        }
        
        public DemoInterfaceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DemoInterfaceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DemoInterfaceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DemoInterfaceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        ConsoleTest.WcfServiceDemo.CheckCustomerLicenceResponse ConsoleTest.WcfServiceDemo.IDemoInterface.CheckCustomerLicence(ConsoleTest.WcfServiceDemo.CheckCustomerLicenceRequest request) {
            return base.Channel.CheckCustomerLicence(request);
        }
        
        public bool CheckCustomerLicence(string sCustomerCode, string sLicenceCode, ref string sMsg) {
            ConsoleTest.WcfServiceDemo.CheckCustomerLicenceRequest inValue = new ConsoleTest.WcfServiceDemo.CheckCustomerLicenceRequest();
            inValue.sCustomerCode = sCustomerCode;
            inValue.sLicenceCode = sLicenceCode;
            inValue.sMsg = sMsg;
            ConsoleTest.WcfServiceDemo.CheckCustomerLicenceResponse retVal = ((ConsoleTest.WcfServiceDemo.IDemoInterface)(this)).CheckCustomerLicence(inValue);
            sMsg = retVal.sMsg;
            return retVal.CheckCustomerLicenceResult;
        }
        
        public System.Threading.Tasks.Task<ConsoleTest.WcfServiceDemo.CheckCustomerLicenceResponse> CheckCustomerLicenceAsync(ConsoleTest.WcfServiceDemo.CheckCustomerLicenceRequest request) {
            return base.Channel.CheckCustomerLicenceAsync(request);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="WcfServiceDemo.IDemoService")]
    public interface IDemoService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDemoService/DoWork", ReplyAction="http://tempuri.org/IDemoService/DoWorkResponse")]
        void DoWork();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDemoService/DoWork", ReplyAction="http://tempuri.org/IDemoService/DoWorkResponse")]
        System.Threading.Tasks.Task DoWorkAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDemoServiceChannel : ConsoleTest.WcfServiceDemo.IDemoService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DemoServiceClient : System.ServiceModel.ClientBase<ConsoleTest.WcfServiceDemo.IDemoService>, ConsoleTest.WcfServiceDemo.IDemoService {
        
        public DemoServiceClient() {
        }
        
        public DemoServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DemoServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DemoServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DemoServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void DoWork() {
            base.Channel.DoWork();
        }
        
        public System.Threading.Tasks.Task DoWorkAsync() {
            return base.Channel.DoWorkAsync();
        }
    }
}
