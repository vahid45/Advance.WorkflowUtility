using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFCreateOrderProduct : CodeActivity
    {
        [RequiredArgument]
        [Input("Order")]
        [ReferenceTarget("salesorder")]
        public InArgument<EntityReference> Order { get; set; }

        [RequiredArgument]
        [Input("Product")]
        [ReferenceTarget("product")]
        public InArgument<EntityReference> Product { get; set; }

        [RequiredArgument]
        [Input("Unit")]
        [ReferenceTarget("uom")]
        public InArgument<EntityReference> Unit { get; set; }

        [RequiredArgument]
        [Input("Quantity")]
        public InArgument<decimal> Quantity { get; set; }

        [Input("Manaual Discount")]
        public InArgument<decimal> Discount { get; set; }

        [Input("Tax")]
        public InArgument<decimal> Tax { get; set; }


        [Output("Sales Order Detail")]
        [ReferenceTarget("salesorderdetail")]
        public OutArgument<EntityReference> SalesOrderDetail { get; set; }

        [Output("Message")]
        public OutArgument<string> Message { get; set; }

        [Output("Is Error")]
        public OutArgument<bool> IsError { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            EntityReference order = context.GetValue(this.Order);
            EntityReference product = context.GetValue(this.Product);
            EntityReference unit = context.GetValue(this.Unit);
            decimal? quantity = context.GetValue(this.Quantity);
            decimal? discount = context.GetValue(this.Discount);
            decimal? tax = context.GetValue(this.Tax);
            context.SetValue(this.Message, "");
            context.SetValue(this.IsError, false);
            context.SetValue(this.SalesOrderDetail, null);

            if (order != null && product != null && unit != null && quantity.HasValue)
            {
                try
                {
                    Entity salesOrder = new Entity("salesorderdetail");
                    salesOrder["productid"] = product;
                    salesOrder["salesorderid"] = order;
                    salesOrder["uomid"] = unit;
                    salesOrder["quantity"] = quantity.Value;
                    if (discount.HasValue)
                    {
                        salesOrder["manualdiscountamount"] = discount.Value;
                    }
                    if (tax.HasValue)
                    {
                        salesOrder["tax"] = tax.Value;
                    }
                    Guid salesOrderDetailId = orgService.Create(salesOrder);
                    EntityReference salesOrderDetailRef = new EntityReference("salesorderdetail", salesOrderDetailId);
                    context.SetValue(this.SalesOrderDetail, salesOrderDetailRef);

                }
                catch (Exception ex)
                {

                    context.SetValue(this.Message, ex.Message);
                    context.SetValue(this.IsError, true);
                }
            }
            else
            {
                context.SetValue(this.Message, "Order,Unit,Product or Quantity is Empty");
                context.SetValue(this.IsError, true);
            }
        }
    }
}
