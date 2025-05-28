using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Globalization;

namespace Advance.WorkflowUtility
{

    public sealed class WFUpdateDisableEntity : CodeActivity
    {
        [Input("Current Record URL")]
        public InArgument<string> URL { get; set; }
        [Input("FieldName")]
        public InArgument<string> FieldName { get; set; }
        [Input("Value")]
        public InArgument<string> Value { get; set; }

        [Input("Int Decimal Value")]
        public InArgument<decimal> IntValue { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string url = context.GetValue(this.URL);
            string fieldName = context.GetValue(this.FieldName);
            string value = context.GetValue(this.Value);
            decimal intValue = context.GetValue(this.IntValue);
            Helper.CRMUtilityHelper utHelper = new Helper.CRMUtilityHelper(orgService);

            string entityName = "";
            string etcCode = "";
            Guid id = utHelper.GetInfoFromRecordURL(url, out etcCode, out entityName);
            //Entity disabledEntity = orgService.Retrieve(entityName, id, new Microsoft.Xrm.Sdk.Query.ColumnSet(fieldName));
            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
            {
                EntityLogicalName = entityName,
                LogicalName = fieldName,
                RetrieveAsIfPublished = true
            };
            RetrieveAttributeResponse attributeResponse = (RetrieveAttributeResponse)orgService.Execute(attributeRequest);
            AttributeTypeCode? fieldObject = attributeResponse.AttributeMetadata.AttributeType;
            if (fieldObject.HasValue)
            {
                Entity disabledEntity = new Entity()
                {
                    LogicalName = entityName,
                    Id = id
                };
                string type = fieldObject.Value.ToString();
                if (type == "Decimal")
                {

                    disabledEntity[fieldName] = intValue;


                }
                else if (type == "Money")
                {
                    //value = value.Replace(".", "").Replace("/", "");


                    disabledEntity[fieldName] = new Money(intValue);


                }
                else if (type == "Int32")
                {
                    //value = value.Replace(".", "").Replace("/", "");


                    disabledEntity[fieldName] = Convert.ToInt32(intValue);

                }
                else if (type == "String")
                {
                    disabledEntity[fieldName] = value;
                }
                else if (type == "OptionSetValue")
                {

                    disabledEntity[fieldName] = new OptionSetValue(Convert.ToInt32(intValue));

                }
                else if (type == "Integer")
                {

                    disabledEntity[fieldName] = Convert.ToInt32(intValue);


                }
                else if (type == "EntityReference")
                {
                    string targetEtcCode = "";
                    string targetEntityName = "";
                    Guid targetId = utHelper.GetInfoFromRecordURL(value, out targetEtcCode, out targetEntityName);
                    disabledEntity[fieldName] = new EntityReference(targetEntityName, targetId);
                }
                else if (fieldObject.Value == AttributeTypeCode.Boolean)
                {
                    bool boolValue;
                    if (bool.TryParse(value, out boolValue))
                    {
                        disabledEntity[fieldName] = boolValue;
                    }

                }

                orgService.Update(disabledEntity);
            }

        }
    }
}
