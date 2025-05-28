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

    public sealed class WFCopyPriceList : CodeActivity
    {
        [Input("Current Price List")]
        [RequiredArgument]
        [ReferenceTarget("pricelevel")]
        public InArgument<EntityReference> CurrentPriceList { get; set; }

        [RequiredArgument]
        [Input("New Price List : Name")]
        public InArgument<string> NewPriceListName { get; set; }

        [RequiredArgument]
        [Input("New Price List : Start Date")]
        public InArgument<DateTime> NewPriceListStartDate { get; set; }

        [Input("New Price List : End Date")]
        public InArgument<DateTime> NewPriceListEndDate { get; set; }

        [Output("Created PriceList")]
        [ReferenceTarget("pricelevel")]
        public OutArgument<EntityReference> CreatedPriceList { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            // Create a column set to define which attributes should be retrieved.
            ColumnSet attributes = new ColumnSet(true);

            Entity currentPriceLevelEntity = new Entity("pricelevel");
            currentPriceLevelEntity = orgService.Retrieve("pricelevel", CurrentPriceList.Get<EntityReference>(context).Id, attributes);

            // Create New Price List
            Entity newPriceLevel = new Entity("pricelevel");
            newPriceLevel["name"] = NewPriceListName.Get<string>(context);
            newPriceLevel["begindate"] = NewPriceListStartDate.Get<DateTime>(context);
            newPriceLevel["enddate"] = NewPriceListEndDate.Get<DateTime>(context);
            newPriceLevel["transactioncurrencyid"] = currentPriceLevelEntity["transactioncurrencyid"];
            Guid newPriceLevelId = orgService.Create(newPriceLevel);
            CreatedPriceList.Set(context, new EntityReference("pricelevel", newPriceLevelId));

            // Get current product price level
            QueryExpression productPriceLevelExpression = new QueryExpression("productpricelevel");

            FilterExpression productPriceLevelFilterExpression = new FilterExpression();
            productPriceLevelFilterExpression.Conditions.Add(new ConditionExpression("pricelevelid", ConditionOperator.Equal, currentPriceLevelEntity["pricelevelid"]));

            productPriceLevelExpression.ColumnSet = new ColumnSet(true);
            productPriceLevelExpression.Criteria = productPriceLevelFilterExpression;

            EntityCollection productPriceLevelList = orgService.RetrieveMultiple(productPriceLevelExpression);

            // Create new product price level records
            for (int index = 0; productPriceLevelList.Entities != null && index < productPriceLevelList.Entities.Count; index++)
            {
                Entity newProductPriceLevelEntity = new Entity("productpricelevel");
                newProductPriceLevelEntity["pricelevelid"] = new EntityReference("pricelevel", newPriceLevelId);
                newProductPriceLevelEntity["productid"] = productPriceLevelList.Entities[index]["productid"];
                newProductPriceLevelEntity["uomid"] = productPriceLevelList.Entities[index]["uomid"];
                newProductPriceLevelEntity["amount"] = productPriceLevelList.Entities[index]["amount"];
                orgService.Create(newProductPriceLevelEntity);
            }
        }
    }
}
