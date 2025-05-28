using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFGetLenghtofString : CodeActivity
    {
        [RequiredArgument]
        [Input("Text")]

        public InArgument<string> Text { get; set; }

        [Output("Lenght")]
        public OutArgument<int> Lenght { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string text = context.GetValue(this.Text);
            int lenght = text.Length;
            this.Lenght.Set(context, lenght);
        }
    }
}
