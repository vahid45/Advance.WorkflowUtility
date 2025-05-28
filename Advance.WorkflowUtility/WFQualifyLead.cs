using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Advance.WorkflowUtility
{

    public sealed class WFQualifyLead : CodeActivity
    {
        #region "Parameter Definition"
        [RequiredArgument]
        [Input("Lead")]
        [ReferenceTarget("lead")]
        public InArgument<EntityReference> Lead { get; set; }

        [RequiredArgument]
        [Input("Create Account")]
        public InArgument<bool> CreateAccount { get; set; }

        [RequiredArgument]
        [Input("Create Contact")]
        public InArgument<bool> CreateContact { get; set; }

        [RequiredArgument]
        [Input("Create Opportunity")]
        public InArgument<bool> CreateOpportunity { get; set; }

        [Input("Existing Account")]
        [ReferenceTarget("account")]
        public InArgument<EntityReference> ExistingAccount { get; set; }

        [RequiredArgument]
        [Input("LeadStatus")]

        public InArgument<int> LeadStatus { get; set; }



        [Output("Created Account")]
        [ReferenceTarget("account")]
        public OutArgument<EntityReference> CreatedAccount { get; set; }

        [Output("Created Contact")]
        [ReferenceTarget("contact")]
        public OutArgument<EntityReference> CreatedContact { get; set; }

        [Output("Created Opportunity")]
        [ReferenceTarget("opportunity")]
        public OutArgument<EntityReference> CreatedOpportunity { get; set; }
        #endregion
        protected override void Execute(CodeActivityContext executionContext)
        {


            #region "Load CRM Service from context"

            Common objCommon = new Common(executionContext);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            #endregion

            #region "Read Parameters"
            EntityReference lead = this.Lead.Get(executionContext);
            if (lead == null)
            {
                return;
            }

            bool createAccount = this.CreateAccount.Get(executionContext);
            bool createContact = this.CreateContact.Get(executionContext);
            bool createOpportunity = this.CreateOpportunity.Get(executionContext);
            EntityReference existingAccount = this.ExistingAccount.Get(executionContext);
            int leadStatus = this.LeadStatus.Get(executionContext);

            objCommon.tracingService.Trace("LeadID=" + lead.Id);
            #endregion


            #region "QualifyLead Execution"
            var query = new QueryExpression("organization");
            query.ColumnSet = new ColumnSet("basecurrencyid");
            var result = objCommon.orgService.RetrieveMultiple(query);
            var currencyId = (EntityReference)result.Entities[0]["basecurrencyid"];


            var qualifyIntoOpportunityReq = new QualifyLeadRequest();

            qualifyIntoOpportunityReq.CreateOpportunity = createOpportunity;
            qualifyIntoOpportunityReq.CreateAccount = createAccount;
            qualifyIntoOpportunityReq.CreateContact = createContact;
            qualifyIntoOpportunityReq.OpportunityCurrencyId = currencyId;
            if (existingAccount != null)
            {
                qualifyIntoOpportunityReq.OpportunityCustomerId = new EntityReference(
                        "account", existingAccount.Id);
            }
            qualifyIntoOpportunityReq.Status = new OptionSetValue(leadStatus);
            qualifyIntoOpportunityReq.LeadId = new EntityReference("lead", lead.Id);


            var qualifyIntoOpportunityRes =
                (QualifyLeadResponse)objCommon.orgService.Execute(qualifyIntoOpportunityReq);
            Console.WriteLine("  Executed OK.");
            this.CreatedAccount.Set(executionContext,null);
            this.CreatedContact.Set(executionContext, null);
            this.CreatedOpportunity.Set(executionContext, null);
            if (qualifyIntoOpportunityRes != null && qualifyIntoOpportunityRes.CreatedEntities != null)
            {
                foreach (var item in qualifyIntoOpportunityRes.CreatedEntities)
                {
                    if (item != null && item.LogicalName == "account" )
                    {
                        this.CreatedAccount.Set(executionContext, item);

                    }
                    else if (item != null && item.LogicalName == "contact")
                    {
                        this.CreatedContact.Set(executionContext, item);

                    }
                    else if (item != null && item.LogicalName == "opportunity")
                    {
                        this.CreatedOpportunity.Set(executionContext, item);

                    }
                }
            }
            #endregion

        }
    }
}
