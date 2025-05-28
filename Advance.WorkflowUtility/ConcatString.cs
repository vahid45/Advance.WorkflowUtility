using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class ConcatString : CodeActivity
    {
        [Input("String1")]
        public InArgument<string> String1 { get; set; }

        [Input("String2")]
        public InArgument<string> String2 { get; set; }


        [Output("ConcatedString")]
        public OutArgument<string> ConcatedString { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string string1 = context.GetValue(this.String1);
            string string2 = context.GetValue(this.String2);
            if (string.IsNullOrEmpty(string1))
            {
                string1 = "";
            }
            if (string.IsNullOrEmpty(string2))
            {
                string2 = "";
            }
            context.SetValue(this.ConcatedString, string1 + string2);
        }
    }
}
