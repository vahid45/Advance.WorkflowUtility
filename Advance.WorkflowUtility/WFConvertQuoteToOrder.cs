using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace Advance.WorkflowUtility
{

    public sealed class WFConvertQuoteToOrder : CodeActivity
    {
        [RequiredArgument]
        [Input("Quote")]
        [ReferenceTarget("quote")]
        public InArgument<EntityReference> Quote { get; set; }


        [Output("Sale Order")]
        [ReferenceTarget("salesorder")]
        public OutArgument<EntityReference> Order { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            EntityReference quote = context.GetValue(this.Quote);
            if (quote != null)
            {
                ColumnSet salesOrderColumns = new ColumnSet("salesorderid", "totalamount");
                ConvertQuoteToSalesOrderRequest convertQuoteRequest = new ConvertQuoteToSalesOrderRequest()
                {
                    QuoteId = quote.Id,
                    ColumnSet = salesOrderColumns
                };
                try
                {
                    ConvertQuoteToSalesOrderResponse convertQuoteResponse = (ConvertQuoteToSalesOrderResponse)orgService.Execute(convertQuoteRequest);
                    Entity salesOrder = (Entity)convertQuoteResponse.Entity;
                    this.Order.Set(context, salesOrder.ToEntityReference());
                }
                catch (Exception ex)
                {

                    throw new InvalidPluginExecutionException(ex.Message);
                }

            }

        }
    }
}
