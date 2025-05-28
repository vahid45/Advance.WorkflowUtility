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

    public sealed class RemoveProductFromPriceList : CodeActivity
    {
        [RequiredArgument]
        [Input("Product")]
        [ReferenceTarget("product")]
        public InArgument<EntityReference> Product { get; set; }

        [RequiredArgument]
        [Input("Price List")]
        [ReferenceTarget("pricelevel")]
        public InArgument<EntityReference> PriceList { get; set; }


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
            if (product != null && pricelist != null)
            {
                string message = InternalExecuter(product, pricelist, orgService);
                if (!string.IsNullOrEmpty(message))
                {
                    context.SetValue(this.Message, message);
                    context.SetValue(this.IsError, true);
                }
            }

        }

        private string InternalExecuter(EntityReference product, EntityReference pricelist, IOrganizationService orgService)
        {
            try
            {
                QueryExpression qry = new QueryExpression("productpricelevel");
                qry.Criteria.AddCondition("productid", ConditionOperator.Equal, product.Id);
                qry.Criteria.AddCondition("pricelevelid", ConditionOperator.Equal, pricelist.Id);
                EntityCollection priclistItems = orgService.RetrieveMultiple(qry);
                if (priclistItems != null && priclistItems.Entities != null)
                {
                    foreach (var item in priclistItems.Entities)
                    {
                        orgService.Delete(item.LogicalName, item.Id);
                    }
                }
                

            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            return "";
        }
    }
}
