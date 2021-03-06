﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.18444.
// 
#pragma warning disable 1591

namespace Metricon.WCF.MetriconRetailSystem.Services.DocuSignWebService {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="DocuSignWebServiceSoap", Namespace="http://tempuri.org/")]
    public partial class DocuSignWebService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback DocuSign_CreateAndSendEnvelopeOperationCompleted;
        
        private System.Threading.SendOrPostCallback DocuSign_SignInPersonOperationCompleted;
        
        private System.Threading.SendOrPostCallback DocuSign_GetEnvelopeStatusOperationCompleted;
        
        private System.Threading.SendOrPostCallback DocuSign_GetRecipientViewOperationCompleted;
        
        private System.Threading.SendOrPostCallback DocuSign_VoidEnvelopeOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public DocuSignWebService() {
            this.Url = global::Metricon.WCF.MetriconRetailSystem.Services.Properties.Settings.Default.Metricon_WCF_MetriconRetailSystem_Services_DocuSignWebService_DocuSignWebService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event DocuSign_CreateAndSendEnvelopeCompletedEventHandler DocuSign_CreateAndSendEnvelopeCompleted;
        
        /// <remarks/>
        public event DocuSign_SignInPersonCompletedEventHandler DocuSign_SignInPersonCompleted;
        
        /// <remarks/>
        public event DocuSign_GetEnvelopeStatusCompletedEventHandler DocuSign_GetEnvelopeStatusCompleted;
        
        /// <remarks/>
        public event DocuSign_GetRecipientViewCompletedEventHandler DocuSign_GetRecipientViewCompleted;
        
        /// <remarks/>
        public event DocuSign_VoidEnvelopeCompletedEventHandler DocuSign_VoidEnvelopeCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/DocuSign_CreateAndSendEnvelope", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public CreateEnvelopeResponse DocuSign_CreateAndSendEnvelope([System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] pdf, int pagenumber, string estimateID, string versiontype, string printtype, string versionnumber, string signeremail, string signername, string anchorXOffset, string anchorYOffset) {
            object[] results = this.Invoke("DocuSign_CreateAndSendEnvelope", new object[] {
                        pdf,
                        pagenumber,
                        estimateID,
                        versiontype,
                        printtype,
                        versionnumber,
                        signeremail,
                        signername,
                        anchorXOffset,
                        anchorYOffset});
            return ((CreateEnvelopeResponse)(results[0]));
        }
        
        /// <remarks/>
        public void DocuSign_CreateAndSendEnvelopeAsync(byte[] pdf, int pagenumber, string estimateID, string versiontype, string printtype, string versionnumber, string signeremail, string signername, string anchorXOffset, string anchorYOffset) {
            this.DocuSign_CreateAndSendEnvelopeAsync(pdf, pagenumber, estimateID, versiontype, printtype, versionnumber, signeremail, signername, anchorXOffset, anchorYOffset, null);
        }
        
        /// <remarks/>
        public void DocuSign_CreateAndSendEnvelopeAsync(byte[] pdf, int pagenumber, string estimateID, string versiontype, string printtype, string versionnumber, string signeremail, string signername, string anchorXOffset, string anchorYOffset, object userState) {
            if ((this.DocuSign_CreateAndSendEnvelopeOperationCompleted == null)) {
                this.DocuSign_CreateAndSendEnvelopeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDocuSign_CreateAndSendEnvelopeOperationCompleted);
            }
            this.InvokeAsync("DocuSign_CreateAndSendEnvelope", new object[] {
                        pdf,
                        pagenumber,
                        estimateID,
                        versiontype,
                        printtype,
                        versionnumber,
                        signeremail,
                        signername,
                        anchorXOffset,
                        anchorYOffset}, this.DocuSign_CreateAndSendEnvelopeOperationCompleted, userState);
        }
        
        private void OnDocuSign_CreateAndSendEnvelopeOperationCompleted(object arg) {
            if ((this.DocuSign_CreateAndSendEnvelopeCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DocuSign_CreateAndSendEnvelopeCompleted(this, new DocuSign_CreateAndSendEnvelopeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/DocuSign_SignInPerson", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public CreateEnvelopeResponse DocuSign_SignInPerson([System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] pdf, int pagenumber, string estimateID, string versiontype, string printtype, string versionnumber, string signeremail, string signername, string anchorXOffset, string anchorYOffset) {
            object[] results = this.Invoke("DocuSign_SignInPerson", new object[] {
                        pdf,
                        pagenumber,
                        estimateID,
                        versiontype,
                        printtype,
                        versionnumber,
                        signeremail,
                        signername,
                        anchorXOffset,
                        anchorYOffset});
            return ((CreateEnvelopeResponse)(results[0]));
        }
        
        /// <remarks/>
        public void DocuSign_SignInPersonAsync(byte[] pdf, int pagenumber, string estimateID, string versiontype, string printtype, string versionnumber, string signeremail, string signername, string anchorXOffset, string anchorYOffset) {
            this.DocuSign_SignInPersonAsync(pdf, pagenumber, estimateID, versiontype, printtype, versionnumber, signeremail, signername, anchorXOffset, anchorYOffset, null);
        }
        
        /// <remarks/>
        public void DocuSign_SignInPersonAsync(byte[] pdf, int pagenumber, string estimateID, string versiontype, string printtype, string versionnumber, string signeremail, string signername, string anchorXOffset, string anchorYOffset, object userState) {
            if ((this.DocuSign_SignInPersonOperationCompleted == null)) {
                this.DocuSign_SignInPersonOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDocuSign_SignInPersonOperationCompleted);
            }
            this.InvokeAsync("DocuSign_SignInPerson", new object[] {
                        pdf,
                        pagenumber,
                        estimateID,
                        versiontype,
                        printtype,
                        versionnumber,
                        signeremail,
                        signername,
                        anchorXOffset,
                        anchorYOffset}, this.DocuSign_SignInPersonOperationCompleted, userState);
        }
        
        private void OnDocuSign_SignInPersonOperationCompleted(object arg) {
            if ((this.DocuSign_SignInPersonCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DocuSign_SignInPersonCompleted(this, new DocuSign_SignInPersonCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/DocuSign_GetEnvelopeStatus", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public EnvelopeStatusInfo[] DocuSign_GetEnvelopeStatus(System.DateTime fromdate, string estimateid) {
            object[] results = this.Invoke("DocuSign_GetEnvelopeStatus", new object[] {
                        fromdate,
                        estimateid});
            return ((EnvelopeStatusInfo[])(results[0]));
        }
        
        /// <remarks/>
        public void DocuSign_GetEnvelopeStatusAsync(System.DateTime fromdate, string estimateid) {
            this.DocuSign_GetEnvelopeStatusAsync(fromdate, estimateid, null);
        }
        
        /// <remarks/>
        public void DocuSign_GetEnvelopeStatusAsync(System.DateTime fromdate, string estimateid, object userState) {
            if ((this.DocuSign_GetEnvelopeStatusOperationCompleted == null)) {
                this.DocuSign_GetEnvelopeStatusOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDocuSign_GetEnvelopeStatusOperationCompleted);
            }
            this.InvokeAsync("DocuSign_GetEnvelopeStatus", new object[] {
                        fromdate,
                        estimateid}, this.DocuSign_GetEnvelopeStatusOperationCompleted, userState);
        }
        
        private void OnDocuSign_GetEnvelopeStatusOperationCompleted(object arg) {
            if ((this.DocuSign_GetEnvelopeStatusCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DocuSign_GetEnvelopeStatusCompleted(this, new DocuSign_GetEnvelopeStatusCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/DocuSign_GetRecipientView", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public GetViewResponse DocuSign_GetRecipientView(string envelopeid, string username) {
            object[] results = this.Invoke("DocuSign_GetRecipientView", new object[] {
                        envelopeid,
                        username});
            return ((GetViewResponse)(results[0]));
        }
        
        /// <remarks/>
        public void DocuSign_GetRecipientViewAsync(string envelopeid, string username) {
            this.DocuSign_GetRecipientViewAsync(envelopeid, username, null);
        }
        
        /// <remarks/>
        public void DocuSign_GetRecipientViewAsync(string envelopeid, string username, object userState) {
            if ((this.DocuSign_GetRecipientViewOperationCompleted == null)) {
                this.DocuSign_GetRecipientViewOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDocuSign_GetRecipientViewOperationCompleted);
            }
            this.InvokeAsync("DocuSign_GetRecipientView", new object[] {
                        envelopeid,
                        username}, this.DocuSign_GetRecipientViewOperationCompleted, userState);
        }
        
        private void OnDocuSign_GetRecipientViewOperationCompleted(object arg) {
            if ((this.DocuSign_GetRecipientViewCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DocuSign_GetRecipientViewCompleted(this, new DocuSign_GetRecipientViewCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/DocuSign_VoidEnvelope", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public GetViewResponse DocuSign_VoidEnvelope(string envelopeid, string username) {
            object[] results = this.Invoke("DocuSign_VoidEnvelope", new object[] {
                        envelopeid,
                        username});
            return ((GetViewResponse)(results[0]));
        }
        
        /// <remarks/>
        public void DocuSign_VoidEnvelopeAsync(string envelopeid, string username) {
            this.DocuSign_VoidEnvelopeAsync(envelopeid, username, null);
        }
        
        /// <remarks/>
        public void DocuSign_VoidEnvelopeAsync(string envelopeid, string username, object userState) {
            if ((this.DocuSign_VoidEnvelopeOperationCompleted == null)) {
                this.DocuSign_VoidEnvelopeOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDocuSign_VoidEnvelopeOperationCompleted);
            }
            this.InvokeAsync("DocuSign_VoidEnvelope", new object[] {
                        envelopeid,
                        username}, this.DocuSign_VoidEnvelopeOperationCompleted, userState);
        }
        
        private void OnDocuSign_VoidEnvelopeOperationCompleted(object arg) {
            if ((this.DocuSign_VoidEnvelopeCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DocuSign_VoidEnvelopeCompleted(this, new DocuSign_VoidEnvelopeCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class CreateEnvelopeResponse {
        
        private string envelopeIdField;
        
        private string uriField;
        
        private string statusDateTimeField;
        
        private string statusField;
        
        /// <remarks/>
        public string envelopeId {
            get {
                return this.envelopeIdField;
            }
            set {
                this.envelopeIdField = value;
            }
        }
        
        /// <remarks/>
        public string uri {
            get {
                return this.uriField;
            }
            set {
                this.uriField = value;
            }
        }
        
        /// <remarks/>
        public string statusDateTime {
            get {
                return this.statusDateTimeField;
            }
            set {
                this.statusDateTimeField = value;
            }
        }
        
        /// <remarks/>
        public string status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class GetViewResponse {
        
        private string errorCodeField;
        
        private string messageField;
        
        /// <remarks/>
        public string ErrorCode {
            get {
                return this.errorCodeField;
            }
            set {
                this.errorCodeField = value;
            }
        }
        
        /// <remarks/>
        public string Message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.34234")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class EnvelopeStatusInfo {
        
        private string statusField;
        
        private string recipientsField;
        
        private string envelopeIdField;
        
        private string estimateidField;
        
        private string documenttypeField;
        
        private string printtypeField;
        
        private string revisionnumberField;
        
        private string statusChangedDateTimeField;
        
        private string deletedField;
        
        /// <remarks/>
        public string status {
            get {
                return this.statusField;
            }
            set {
                this.statusField = value;
            }
        }
        
        /// <remarks/>
        public string recipients {
            get {
                return this.recipientsField;
            }
            set {
                this.recipientsField = value;
            }
        }
        
        /// <remarks/>
        public string envelopeId {
            get {
                return this.envelopeIdField;
            }
            set {
                this.envelopeIdField = value;
            }
        }
        
        /// <remarks/>
        public string estimateid {
            get {
                return this.estimateidField;
            }
            set {
                this.estimateidField = value;
            }
        }
        
        /// <remarks/>
        public string documenttype {
            get {
                return this.documenttypeField;
            }
            set {
                this.documenttypeField = value;
            }
        }
        
        /// <remarks/>
        public string printtype {
            get {
                return this.printtypeField;
            }
            set {
                this.printtypeField = value;
            }
        }
        
        /// <remarks/>
        public string revisionnumber {
            get {
                return this.revisionnumberField;
            }
            set {
                this.revisionnumberField = value;
            }
        }
        
        /// <remarks/>
        public string statusChangedDateTime {
            get {
                return this.statusChangedDateTimeField;
            }
            set {
                this.statusChangedDateTimeField = value;
            }
        }
        
        /// <remarks/>
        public string deleted {
            get {
                return this.deletedField;
            }
            set {
                this.deletedField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void DocuSign_CreateAndSendEnvelopeCompletedEventHandler(object sender, DocuSign_CreateAndSendEnvelopeCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DocuSign_CreateAndSendEnvelopeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DocuSign_CreateAndSendEnvelopeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public CreateEnvelopeResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((CreateEnvelopeResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void DocuSign_SignInPersonCompletedEventHandler(object sender, DocuSign_SignInPersonCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DocuSign_SignInPersonCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DocuSign_SignInPersonCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public CreateEnvelopeResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((CreateEnvelopeResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void DocuSign_GetEnvelopeStatusCompletedEventHandler(object sender, DocuSign_GetEnvelopeStatusCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DocuSign_GetEnvelopeStatusCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DocuSign_GetEnvelopeStatusCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public EnvelopeStatusInfo[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((EnvelopeStatusInfo[])(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void DocuSign_GetRecipientViewCompletedEventHandler(object sender, DocuSign_GetRecipientViewCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DocuSign_GetRecipientViewCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DocuSign_GetRecipientViewCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public GetViewResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((GetViewResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    public delegate void DocuSign_VoidEnvelopeCompletedEventHandler(object sender, DocuSign_VoidEnvelopeCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.18408")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DocuSign_VoidEnvelopeCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DocuSign_VoidEnvelopeCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public GetViewResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((GetViewResponse)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591