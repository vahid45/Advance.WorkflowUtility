using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFConvertMiladiToShamsi : CodeActivity
    {
        [RequiredArgument]
        [Input("Mildai Date")]
        public InArgument<DateTime> MiladiDate { get; set; }

        [RequiredArgument]
        [Input("Foramt")]
        [Default("yyyy/MM/dd")]
        public InArgument<string> Foramt { get; set; }

        [Output("Shamsi Date")]
        public OutArgument<string> ShamsiDate { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            DateTime miladiDate = context.GetValue(this.MiladiDate);
            string format = context.GetValue(this.Foramt);
            if (miladiDate != null)
            {
                string shamsiDate = miladiDate.ToPeString(format);
                context.SetValue(this.ShamsiDate, shamsiDate);
            }
        }
    }
}
