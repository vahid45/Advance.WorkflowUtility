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

    public sealed class WFConvertOrderToInvoice : CodeActivity
    {

        [RequiredArgument]
        [Input("Order")]
        [ReferenceTarget("salesorder")]
        public InArgument<EntityReference> Order { get; set; }


        [Output("Invoice")]
        [ReferenceTarget("invoice")]
        public OutArgument<EntityReference> Invoice { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            EntityReference order = context.GetValue(this.Order);
            if (order != null)
            {
                this.Invoice.Set(context, InternalExecuter(orgService, order));



            }

        }

        public EntityReference InternalExecuter(IOrganizationService orgService, EntityReference order)
        {
            ColumnSet invoiceColumns = new ColumnSet("salesorderid", "totalamount");
            ConvertSalesOrderToInvoiceRequest convertOrderToInvoiceRequest = new ConvertSalesOrderToInvoiceRequest()
            {
                SalesOrderId = order.Id,
                ColumnSet = invoiceColumns
            };
            try
            {
                ConvertSalesOrderToInvoiceResponse convertOrderResponse = (ConvertSalesOrderToInvoiceResponse)orgService.Execute(convertOrderToInvoiceRequest);
                Entity invoice = (Entity)convertOrderResponse.Entity;
                return invoice.ToEntityReference();
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }

}
