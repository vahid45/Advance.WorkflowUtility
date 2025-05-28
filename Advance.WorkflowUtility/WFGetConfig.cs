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

    public sealed class WFGetConfig : CodeActivity
    {
        [RequiredArgument]
        [Input("Entity Name")]
        [Default("new_config")]
        public InArgument<string> EntityName { get; set; }

        [RequiredArgument]
        [Input("Key Field name")]
        [Default("new_name")]
        public InArgument<string> KeyFieldName { get; set; }

        [RequiredArgument]
        [Input("Key")]
        public InArgument<string> Key { get; set; }

        [RequiredArgument]
        [Input("Value Field Name")]
        public InArgument<string> ValueFieldName { get; set; }


        [RequiredArgument]
        [Input("Value Field Type:1-String 2-User Or Team Or Queue LookUp")]
        public InArgument<int> ValueFieldType { get; set; }

        [Input("EntityRefrence With Primary Attribute?")]
        [Default("false")]
        public InArgument<bool> WithName { get; set; }

        [Input("If Yes Primary Attribute Name?")]
        public InArgument<string> PrimaryAttributeName { get; set; }

        [Output("String Value")]
        public OutArgument<string> Value { get; set; }
        [Output("Int Value")]
        public OutArgument<int> IntValue { get; set; }
        [Output("Decimal Value")]
        public OutArgument<decimal> DecimalValue { get; set; }

        [Output("LookUp User Value")]
        [ReferenceTarget("systemuser")]
        public OutArgument<EntityReference> UserValue { get; set; }

        [Output("LookUp Team Value")]
        [ReferenceTarget("team")]
        public OutArgument<EntityReference> TeamValue { get; set; }

        [Output("Queue")]
        [ReferenceTarget("queue")]
        public OutArgument<EntityReference> Queue { get; set; }

        [Output("PriceList")]
        [ReferenceTarget("pricelevel")]
        public OutArgument<EntityReference> PriceList { get; set; }

        [Output("Look Up Account")]
        [ReferenceTarget("account")]
        public OutArgument<EntityReference> Account { get; set; }

        [Output("Look Up Currency")]
        [ReferenceTarget("transactioncurrency")]
        public OutArgument<EntityReference> Currency { get; set; }
        [Output("Ref Name")]
        public OutArgument<string> RefName { get; set; }

        [Output("IsFind")]
        public OutArgument<bool> IsFinde { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            #region GetInput
            string entityName = context.GetValue(this.EntityName);
            string keyFieldName = context.GetValue(this.KeyFieldName);
            string key = context.GetValue(this.Key);
            string valueFieldName = context.GetValue(this.ValueFieldName);
            string primaryAtt = context.GetValue(this.PrimaryAttributeName);
            bool needPrimaryAtt = context.GetValue(this.WithName);
            int type = context.GetValue(this.ValueFieldType);
            #endregion
            bool isFind = false;
            EntityReference user;
            EntityReference team;
            EntityReference queue;
            EntityReference priceList;
            EntityReference account;
            EntityReference currency;
            string value;
            decimal? decimalValue;
            int? intValue;
            string refName;
            isFind = InternalExecuter(orgService, entityName, keyFieldName, key, valueFieldName, primaryAtt, needPrimaryAtt, type, out user, out team, out queue, out priceList, out account, out currency, out value, out intValue, out decimalValue, out refName);
            this.TeamValue.Set(context, team);
            this.UserValue.Set(context, user);
            this.Queue.Set(context, queue);
            this.PriceList.Set(context, priceList);
            this.Account.Set(context, account);
            this.Currency.Set(context, currency);
            this.Value.Set(context, value);
            this.RefName.Set(context, refName);
            if (intValue.HasValue)
            {
                this.IntValue.Set(context, intValue.Value);

            }
            if (decimalValue.HasValue)
            {
                this.DecimalValue.Set(context, decimalValue.Value);
            }
            context.SetValue(this.IsFinde, isFind);




        }
        public bool InternalExecuter(IOrganizationService orgService, string entityName, string keyFieldName, string key, string valueFieldName, string primaryAtt, bool needPrimary, int type, out EntityReference user, out EntityReference team, out EntityReference que, out EntityReference priceList, out EntityReference account, out EntityReference currency, out string value, out int? intValue, out decimal? decimalValue, out string refName)
        {
            user = null;
            team = null;
            que = null;
            priceList = null;
            account = null;
            currency = null;
            intValue = null;
            decimalValue = null;
            refName = "";
            value = "";

            bool isFind = false;

            EntityReference refValue = GetValue(orgService, key, valueFieldName, type, out value, entityName, keyFieldName, needPrimary, primaryAtt, out refName);
            if (type == 1 && value != null)
            {
                isFind = true;

                int intValuetmp;
                decimal decimalValuetmp;
                if (int.TryParse(value, out intValuetmp))
                {
                    intValue = intValuetmp;

                }
                if (decimal.TryParse(value, out decimalValuetmp))
                {
                    decimalValue = decimalValuetmp;
                }

            }
            else if (type == 2 && refValue != null)
            {
                if (refValue.LogicalName == "systemuser")
                {
                    EntityReference usersTeam = refValue;
                    EntityReference Queue = GetUsersQueue(orgService, refValue);
                    if (Queue != null)
                    {
                        que = Queue;
                        isFind = true;

                    }
                    user = refValue;

                }
                else if (refValue.LogicalName == "team")
                {
                    team = refValue;
                    isFind = true;
                }
                else if (refValue.LogicalName == "queue")
                {
                    que = refValue;
                    isFind = true;
                }
                else if (refValue.LogicalName == "transctioncurrency")
                {
                    currency = refValue;
                    isFind = true;

                }
                else if (refValue.LogicalName == "pricelevel")
                {
                    priceList = refValue;
                    isFind = true;
                }
                else if (refValue.LogicalName == "account")
                {
                    account = refValue;
                    isFind = true;

                }
            }
            return isFind;
        }
        private EntityReference GetUsersQueue(IOrganizationService orgService, EntityReference refValue)
        {
            Entity user = orgService.Retrieve(refValue.LogicalName, refValue.Id, new ColumnSet("queueid"));
            if (user.Attributes.Contains("queue"))
            {
                EntityReference queue = (EntityReference)user["queue"];
                return queue;
            }
            return null;
        }

        private EntityReference GetUsersTeam(IOrganizationService orgService, EntityReference refValue)
        {
            return null;
        }


        private EntityReference GetValue(IOrganizationService orgService, string key, string valueFiledName, int valueFieldtype, out string value, string entityName, string keyFieldName, bool withName, string primaryFieldName, out string refName)
        {
            QueryExpression configQry = new QueryExpression(entityName);
            configQry.ColumnSet = new ColumnSet(true);
            configQry.Criteria.AddCondition(keyFieldName, ConditionOperator.Equal, key);
            EntityCollection configs = orgService.RetrieveMultiple(configQry);
            refName = null;
            if (configs != null && configs.Entities != null && configs.Entities.Count > 0)
            {
                Entity config = configs.Entities.First();
                int valueType = valueFieldtype;
                if (valueType == 1)
                {
                    value = config[valueFiledName].ToString();

                    return null;
                }
                else if (valueType == 2)
                {
                    try
                    {

                        EntityReference valueRef = (EntityReference)config[valueFiledName];
                        if (withName && primaryFieldName != null && primaryFieldName != "")
                        {

                            Entity refRecord = orgService.Retrieve(valueRef.LogicalName, valueRef.Id, new ColumnSet(primaryFieldName));
                            string primaryName = (string)refRecord[primaryFieldName];
                            refName = primaryName;

                        }

                        value = null;
                        return valueRef;
                    }
                    catch (Exception)
                    {
                        value = null;
                        return null;
                    }

                }

            }
            value = null;
            return null;
        }
    }
}
