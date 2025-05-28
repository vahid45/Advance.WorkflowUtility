using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFUserInRole : CodeActivity
    {

        [Input("User")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> User { get; set; }

        [Input("Is CurrentUser?")]
        public InArgument<bool> IsCurrentUser { get; set; }

        [RequiredArgument]
        [Input("Roles(Split By ,)")]
        public InArgument<string> Roles { get; set; }

        [Output("User In Role")]
        public OutArgument<bool> IsUserInRole { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            EntityReference user = context.GetValue(this.User);
            bool isCurrentUser = context.GetValue(this.IsCurrentUser);
            string roles = context.GetValue(this.Roles);

            if (isCurrentUser == true)
            {
                user = new EntityReference("systemuser", objCommon.workflowContext.InitiatingUserId);
            }
            if (roles != null && roles != "")
            {
                Helper.CRMUtilityHelper utlHelper = new Helper.CRMUtilityHelper(orgService);
                this.IsUserInRole.Set(context, utlHelper.UserInRoles(roles, user.Id));
            }

        }
    }
}
