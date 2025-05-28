using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;

namespace Advance.WorkflowUtility
{

    public sealed class WFCancelRunningWorkflow : CodeActivity
    {
        [RequiredArgument]
        [Input("Current WorkflowName")]
        public InArgument<string> WorkflowName { get; set; }

        [RequiredArgument]
        [Input("Parent Record URL")]
        [ReferenceTarget("")]
        public InArgument<String> ParentRecordURL { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string workflowName = context.GetValue(this.WorkflowName);
     
            #region "Read Parameters"
            String _ParentRecordURL = this.ParentRecordURL.Get(context);
            if (_ParentRecordURL == null || _ParentRecordURL == "")
            {
                return;
            }
            string[] urlParts = _ParentRecordURL.Split("?".ToArray());
            string[] urlParams = urlParts[1].Split("&".ToCharArray());
            string ParentObjectTypeCode = urlParams[0].Replace("etc=", "");
            string ParentId = urlParams[1].Replace("id=", "");
            objCommon.tracingService.Trace("ParentObjectTypeCode=" + ParentObjectTypeCode + "--ParentId=" + ParentId);
            #endregion

            CancelRunningWorkflow(orgService, new Guid(ParentId), workflowName);

        }

        private static void CancelRunningWorkflow(IOrganizationService orgService, Guid regardingObjectId, string processName)
        {

            QueryExpression qry = new QueryExpression("asyncoperation");
            qry.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, regardingObjectId);
            qry.Criteria.AddCondition("name", ConditionOperator.NotEqual, processName);
            FilterExpression filter1 = new FilterExpression(LogicalOperator.Or);
            filter1.AddCondition("statuscode", ConditionOperator.Equal, 10);
            filter1.AddCondition("statuscode", ConditionOperator.Equal, 20);
            filter1.AddCondition("statuscode", ConditionOperator.Equal, 0);
            qry.Criteria.AddFilter(filter1);
            qry.ColumnSet = new ColumnSet(true);
            EntityCollection process = orgService.RetrieveMultiple(qry);

            foreach (Entity item in process.Entities)
            {
                // 0-init, 1-active, 2-waiting, 3-paused, 4-completed, 5-aborted, 6-failed.
                item["statecode"] = new OptionSetValue(3);
                item["statuscode"] = new OptionSetValue(32);
                orgService.Update(item);
            }

        }
    }
}
