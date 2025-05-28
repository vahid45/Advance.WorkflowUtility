using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk.Messages;

namespace Advance.WorkflowUtility
{

    public sealed class WFStartWorkflowFromWorkflow : CodeActivity
    {
        [Input("Workflow")]
        [ReferenceTarget("workflow")]
        public InArgument<EntityReference> Workflow { get; set; }

        [RequiredArgument]
        [Input("Record URL")]
        [ReferenceTarget("")]
        public InArgument<String> RecordURL { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            if (this.Workflow != null && this.Workflow.Get(context) != null)
            {
                #region "Read Parameters"
                String _ParentRecordURL = this.RecordURL.Get(context);
                if (_ParentRecordURL == null || _ParentRecordURL == "")
                {
                    return;
                }
                string[] urlParts = _ParentRecordURL.Split("?".ToArray());
                string[] urlParams = urlParts[1].Split("&".ToCharArray());
                string RecordObjectTypeCode = urlParams[0].Replace("etc=", "");
                string RecordId = urlParams[1].Replace("id=", "");
                #endregion
                if (!string.IsNullOrEmpty(RecordId))
                {
                    InternalExecute(orgService, new Guid(RecordId), this.Workflow.Get(context));
                }
            }
        }
        public void InternalExecute(IOrganizationService orgService, Guid recordId, EntityReference workflow)
        {
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest()
            {
                WorkflowId = workflow.Id,
                EntityId = recordId
            };
           
            orgService.Execute(request);
        }
    }
}
