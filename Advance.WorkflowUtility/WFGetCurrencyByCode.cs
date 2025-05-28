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

    public sealed class WFGetCurrencyByCode : CodeActivity
    {
        [RequiredArgument]
        [Input("Currency Code")]
        [Default("IRR")]
        public InArgument<string> CurrencyCode { get; set; }

        [Output("Currency")]
        [ReferenceTarget("transactioncurrency")]
        public OutArgument<EntityReference> Currency { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string currencyCode = context.GetValue(this.CurrencyCode);
            if (!string.IsNullOrEmpty(currencyCode))
            {
                context.SetValue(this.Currency, InternalExecuter(orgService, currencyCode));
            }
        }

        public EntityReference InternalExecuter(IOrganizationService orgService, string currencyCode)
        {
            QueryExpression qry = new QueryExpression("transactioncurrency");
            qry.Criteria.AddCondition("isocurrencycode", ConditionOperator.Equal, currencyCode);
            try
            {
                EntityCollection currencies = orgService.RetrieveMultiple(qry);
                if (currencies != null && currencies.Entities != null && currencies.Entities.Count > 0)
                {
                    return currencies.Entities.First().ToEntityReference();
                }
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }

            return null;
        }
    }
}
