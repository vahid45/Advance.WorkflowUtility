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

    public sealed class WFDuplicateDetection : CodeActivity
    {
        [Input("Entity")]
        public InArgument<string> Entity { get; set; }

        [Input("Fetch XML")]
        public InArgument<string> FetchXML { get; set; }

        [Input("Is Int")]
        public InArgument<bool> IsInt { get; set; }

        [Input("Value")]
        public InArgument<string> Value { get; set; }

        [Input("FieldName")]
        public InArgument<string> FieldName { get; set; }

        [Input("Operator(And,Or)")]
        public InArgument<string> Operator { get; set; }

        [Input(" Is Pre Operator")]
        public InArgument<bool> IsPreOperator { get; set; }

        [Input("Exact")]
        public InArgument<bool> Exact { get; set; }

        [Output("IsExist")]
        public OutArgument<bool> IsExist { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string entityName = context.GetValue(this.Entity);
            string fetchxml = context.GetValue(this.FetchXML);
            string fieldName = context.GetValue(this.FieldName);
            string value = context.GetValue(this.Value);
            bool isInt = context.GetValue(this.IsInt);
            bool exact = context.GetValue(this.Exact);
            bool _isExist = false;
            string operatorStr = context.GetValue(this.Operator);
            bool isPreOperator = context.GetValue(this.IsPreOperator);
            int intValue = 0;
            if (fieldName != null && value != null && value != "" && !isInt)
            {
                //_isExist = ExistField(entityName, fieldName, value, orgService, exact, operatorStr, isPreOperator);
                _isExist = ExistField(entityName,fetchxml, fieldName, value, orgService, exact, operatorStr, isPreOperator);

            }
            else if (fieldName != null && isInt && int.TryParse(value, out intValue))
            {
                //_isExist = ExistField(entityName, fieldName, intValue, orgService, exact, operatorStr, isPreOperator);
                _isExist = ExistField(entityName,fetchxml, fieldName, intValue, orgService, exact, operatorStr, isPreOperator);

            }

            context.SetValue(this.IsExist, _isExist);
        }
        private bool ExistField(string entity,string fetchXml, string FieldName, string stringValue, IOrganizationService orgService, bool Exact, string operatorStr, bool isPreoperator)
        {
            FilterExpression filter = new FilterExpression();
            if (operatorStr.ToLower() == "or")
            {
                filter.FilterOperator = LogicalOperator.Or;
            }
            else
            {
                filter.FilterOperator = LogicalOperator.And;
            }
            string[] fieldNames = FieldName.Split(',');
            if (Exact)
            {
                foreach (string item in fieldNames)
                {
                    filter.AddCondition(item, ConditionOperator.Equal, stringValue);
                }

            }
            else
            {
                foreach (string item in fieldNames)
                {
                    filter.AddCondition(item, ConditionOperator.Like, "%" + stringValue + "%");
                }
            }
            QueryExpression qry = new QueryExpression();
            if (!string.IsNullOrEmpty(fetchXml))
            {
                 qry = ConvertFetchToQueryExpression(fetchXml, orgService);

            }
            else
            {
                qry.EntityName = entity;
            }
            qry.Criteria.AddFilter(filter);
            EntityCollection enities = orgService.RetrieveMultiple(qry);
            if (enities != null && enities.Entities != null && isPreoperator && enities.Entities.Count > 0)
            {
                return true;
            }
            if (enities != null && enities.Entities != null && !isPreoperator && enities.Entities.Count > 1)
            {
                return true;
            }


            return false;
        }

        private QueryExpression ConvertFetchToQueryExpression(string fetchXml, IOrganizationService orgService)
        {
            Helper.CRMUtilityHelper util = new Helper.CRMUtilityHelper(orgService);
            return util.ConvertFetchExpressionToQueryExpression(fetchXml);
        }

        private bool ExistField(string entity,string fetchXml, string FieldName, int intvalue, IOrganizationService orgService, bool Exact, string operatorStr, bool isPreoperator)
        {
            #region int

            try
            {

                FilterExpression filter = new FilterExpression();
                if (operatorStr.ToLower() == "or")
                {
                    filter.FilterOperator = LogicalOperator.Or;
                }
                else
                {
                    filter.FilterOperator = LogicalOperator.And;
                }
                string[] fieldNames = FieldName.Split(',');
                if (Exact)
                {
                    foreach (string item in fieldNames)
                    {
                        filter.AddCondition(item, ConditionOperator.Equal, intvalue);
                    }

                }
                else
                {
                    foreach (string item in fieldNames)
                    {
                        filter.AddCondition(item, ConditionOperator.Like, "%" + intvalue + "%");
                    }
                }
                QueryExpression entityqry = new QueryExpression();
                if (!string.IsNullOrEmpty(fetchXml))
                {
                    entityqry = ConvertFetchToQueryExpression(fetchXml, orgService);

                }
                else
                {
                    entityqry.EntityName = entity;
                }
                entityqry.Criteria.AddFilter(filter);
                //todo Filter Kojast?
                EntityCollection enities = orgService.RetrieveMultiple(entityqry);
                if (enities != null && enities.Entities != null && isPreoperator && enities.Entities.Count > 0)
                {
                    return true;
                }
                if (enities != null && enities.Entities != null && !isPreoperator && enities.Entities.Count > 1)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }


            #endregion
            return false;
        }
    }
}
