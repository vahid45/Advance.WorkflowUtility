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

    public sealed class QuotRecalculation : CodeActivity
    {
        [RequiredArgument]
        [Input("Quote")]
        [ReferenceTarget("quote")]
        public InArgument<EntityReference> Quote { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory organizationServiceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService orgService = organizationServiceFactory.CreateOrganizationService(workflowContext.UserId);
            EntityReference quoteRef = context.GetValue(this.Quote);
            CalculatePriceRequest req = new CalculatePriceRequest();
            req.Target = quoteRef;
            orgService.Execute(req);
        }
    }
}
