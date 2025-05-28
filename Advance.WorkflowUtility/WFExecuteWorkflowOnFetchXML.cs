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

    public sealed class WFExecuteWorkflowOnFetchXML : CodeActivity
    {
        [RequiredArgument]
        [Input("FetchXml")]
        public InArgument<string> FetchXml { get; set; }

        [RequiredArgument]
        [Input("Workflow")]
        [ReferenceTarget("workflow")]
        public InArgument<EntityReference> Workflow { get; set; }

        [RequiredArgument]
        [Input("Record URL(Dynamic)")]
        public InArgument<string> RecordURL { get; set; }


        [Output("Have Error")]
        public OutArgument<bool> IsError { get; set; }
        [Output("Message")]
        public OutArgument<string> Message { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string fetchXml = context.GetValue(this.FetchXml);
            string recordURL = context.GetValue(this.RecordURL);
            EntityReference workflow = context.GetValue(this.Workflow);
            Helper.CRMUtilityHelper utHelper = new Helper.CRMUtilityHelper(orgService);
            string objectCode = "";
            string entityName = "";
            Guid id = utHelper.GetInfoFromRecordURL(recordURL, out objectCode, out entityName);
            fetchXml = fetchXml.Replace("#id", id.ToString());
            FetchExpression fetch = new FetchExpression(fetchXml);
            string msg = "";


            EntityCollection entities = orgService.RetrieveMultiple(fetch);
            if (entities != null && entities.Entities != null && entities.Entities.Count > 0)
            {
                foreach (var item in entities.Entities)
                {
                    OrganizationRequest request = new OrganizationRequest("ExecuteWorkflow");
                    request["EntityId"] = item.Id;
                    request["WorkflowId"] = workflow.Id;
                    try
                    {
                        orgService.Execute(request);
                    }
                    catch (Exception ex)
                    {
                        msg = msg + Environment.NewLine;
                    }
                }
            }

        }
    }
}
