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

    public sealed class SetOrderNumberEntities : CodeActivity
    {
        [RequiredArgument]
        [Input("Record URL")]
        public InArgument<string> RecordURL { get; set; }

        [RequiredArgument]
        [Input("Parent Record URL")]
        public InArgument<string> ParentRecordURL { get; set; }

        [RequiredArgument]
        [Input("Parent Field Name")]
        public InArgument<string> ParentFieldName { get; set; }

        [RequiredArgument]
        [Input("Row Number Field Name")]
        public InArgument<string> RowNumberFieldName { get; set; }


        protected override void Execute(CodeActivityContext executionContext)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(executionContext);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            #endregion
            #region "Read Parameters"
            String recordURL = this.RecordURL.Get(executionContext);
            String parentRecordURL = this.ParentRecordURL.Get(executionContext);
            String parentFieldName = this.ParentFieldName.Get(executionContext);
            String rowNumberFieldName = this.RowNumberFieldName.Get(executionContext);
            if (parentRecordURL == null || parentRecordURL == "")
            {
                return;
            }
            if (recordURL == null || recordURL == "")
            {
                return;
            }

            string[] urlParts = recordURL.Split("?".ToArray());
            string[] urlParams = urlParts[1].Split("&".ToCharArray());
            string objectTypeCode = urlParams[0].Replace("etc=", "");
            string entityName = objCommon.sGetEntityNameFromCode(objectTypeCode, objCommon.orgService);
            string objectId = urlParams[1].Replace("id=", "");

            string[] parenturlParts = parentRecordURL.Split("?".ToArray());
            string[] parenturlParams = parenturlParts[1].Split("&".ToCharArray());
            string parebtobjectTypeCode = parenturlParams[0].Replace("etc=", "");
            string parententityName = objCommon.sGetEntityNameFromCode(parebtobjectTypeCode, objCommon.orgService);
            string parentobjectId = parenturlParams[1].Replace("id=", "");
            #endregion
            InternalExcuter(objCommon.orgService, entityName, parentobjectId, parentFieldName, rowNumberFieldName);


        }

        private void InternalExcuter(IOrganizationService orgService, string entityName, string parentobjectId, string parentFieldName, string rowNumberFieldName)
        {
            //throw new NotImplementedException();
            QueryExpression qry = new QueryExpression(entityName);
            //ColumnSet col1 = new ColumnSet("new_product", "new_rowno");
            qry.Criteria.AddCondition(parentFieldName, ConditionOperator.Equal, parentobjectId);
            qry.Orders.Add(new OrderExpression("createdon", OrderType.Ascending));
            EntityCollection entities = orgService.RetrieveMultiple(qry);
            if (entities != null && entities.Entities != null)
            {
                int Num = 1;
                foreach (var item in entities.Entities)
                {
                    Entity updatedEntity = new Entity(item.LogicalName);
                    updatedEntity.Id = item.Id;
                    updatedEntity[rowNumberFieldName] = Num;
                    Num++;
                    orgService.Update(updatedEntity);

                }
            }


        }
    }
}
