using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFGetCurrentUser : CodeActivity
    {
        [Output("Current User")]
        [ReferenceTarget("systemuser")]
        public OutArgument<EntityReference> User { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            context.SetValue(this.User, new EntityReference("systemyser", objCommon.workflowContext.UserId));
        }
    }
}
