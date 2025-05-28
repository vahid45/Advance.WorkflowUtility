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

    public sealed class AddProductToPriceList : CodeActivity
    {
        [RequiredArgument]
        [Input("Product")]
        [ReferenceTarget("product")]
        public InArgument<EntityReference> Product { get; set; }

        [RequiredArgument]
        [Input("Price List")]
        [ReferenceTarget("pricelevel")]
        public InArgument<EntityReference> PriceList { get; set; }


        [RequiredArgument]
        [Input("Amount")]
        public InArgument<decimal> Amount { get; set; }

        [RequiredArgument]
        [Input("Pricing Type")]
        [AttributeTarget("productpricelevel", "pricingmethodcode")]
        public InArgument<OptionSetValue> PricingType { get; set; }



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
            EntityReference product = context.GetValue(this.Product);
            EntityReference pricelist = context.GetValue(this.PriceList);
            OptionSetValue priceType = context.GetValue(this.PricingType);
            decimal amount = context.GetValue(this.Amount);
            if (product != null && pricelist != null)
            {
                string message = CreatePriceListItem(product, pricelist, orgService, priceType, amount);
                if (!string.IsNullOrEmpty(message))
                {
                    context.SetValue(this.Message, message);
                    context.SetValue(this.IsError, true);
                }
            }

        }
        private string CreatePriceListItem(EntityReference productRef, EntityReference priceListRef, IOrganizationService orgService, OptionSetValue pricingType, decimal amount)
        {
            try
            {

                Entity product = orgService.Retrieve(productRef.LogicalName, productRef.Id, new ColumnSet("price", "defaultuomid"));
                Entity priceList = orgService.Retrieve(priceListRef.LogicalName, priceListRef.Id, new ColumnSet("transactioncurrencyid"));
                EntityReference defaultUnit = (EntityReference)product.Attributes["defaultuomid"];
                EntityReference currency = (EntityReference)priceList["transactioncurrencyid"];
                Entity priceListItem = new Entity("productpricelevel");
                if (pricingType.Value == 1)
                {
                    priceListItem["amount"] = new Money(amount);

                }
                else
                {
                    priceListItem["percentage"] = amount;
                }
                priceListItem["transactioncurrencyid"] = currency;
                priceListItem["pricingmethodcode"] = pricingType;
                priceListItem["uomid"] = defaultUnit;
                priceListItem["productid"] = new EntityReference("product", product.Id);
                priceListItem["pricelevelid"] = priceListRef;
                orgService.Create(priceListItem); //create the price list item

            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            return "";

        }
    }
}
