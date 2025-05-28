using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace Advance.WorkflowUtility
{
    public class WFRollupQuery : CodeActivity
    {
        [RequiredArgument]
        [Input("FetchXml")]
        public InArgument<string> FetchXml { get; set; }

        [RequiredArgument]
        [Input("Record URL(Dynamic)")]
        public InArgument<string> RecordURL { get; set; }

        [Input("Sum Column")]
        public InArgument<string> SumColumn { get; set; }



        [Output("Count")]
        public OutArgument<int> Count { get; set; }
        [Output("Sum")]
        public OutArgument<decimal> Sum { get; set; }
        [Output("Avrage")]
        public OutArgument<decimal> Avrage { get; set; }
        [Output("Max")]
        public OutArgument<decimal> Max { get; set; }
        [Output("Min")]
        public OutArgument<decimal> Min { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string fetchXml = context.GetValue(this.FetchXml);
            string recordURL = context.GetValue(this.RecordURL);
            string sumColumn = context.GetValue(this.SumColumn);
            bool isDelete = false;
            if (objCommon.workflowContext.MessageName.Equals("delete", StringComparison.InvariantCultureIgnoreCase))
            {
                isDelete = true;
            }
            decimal sum = 0;
            decimal max = 0;
            decimal min = 0;
            int count = InternalExecuter(recordURL, orgService, fetchXml, sumColumn, out sum, out max, out min, isDelete, objCommon.workflowContext.PrimaryEntityId);
            if (count != null && count > 0)
            {
                decimal avrage = sum / Convert.ToDecimal(count);
                this.Avrage.Set(context, avrage);
                this.Count.Set(context, count);
            }
            this.Sum.Set(context, sum);
            this.Max.Set(context, max);
            this.Min.Set(context, min);

        }

        public int InternalExecuter(string primaryEntityURL, IOrganizationService orgService, string fetchXml, string columnName, out decimal sum, out decimal max, out decimal min, bool isDeleteMessage, Guid currentRecordId)
        {
            Helper.CRMUtilityHelper utHelper = new Helper.CRMUtilityHelper(orgService);
            string objectCode = "";
            string entityName = "";
            Guid id = utHelper.GetInfoFromRecordURL(primaryEntityURL, out objectCode, out entityName);
            fetchXml = fetchXml.Replace("#id", id.ToString());
            QueryExpression query = utHelper.ConvertFetchExpressionToQueryExpression(fetchXml);
            if (!query.ColumnSet.Columns.Contains(columnName))
            {
                query.ColumnSet.Columns.Add(columnName);
            }
            sum = 0;
            max = 0;
            min = 0;
            if (query != null)
            {
                EntityCollection entities = orgService.RetrieveMultiple(query);
                if (entities != null && entities.Entities != null && entities.Entities.Count > 0)
                {
                    var sumColumn = entities.Entities.First()[columnName];
                    System.Type type = sumColumn.GetType();
                    var unDeltedEntities = new List<Entity>();
                    if (isDeleteMessage)
                    {
                        unDeltedEntities = entities.Entities.Where(a => (Guid)a[query.EntityName + "id"] != currentRecordId).ToList();
                        if (unDeltedEntities == null || unDeltedEntities.Count() == 0)
                        {
                            return 0;
                        }
                    }


                    if (type == typeof(decimal))
                    {
                        if (isDeleteMessage)
                        {
                            sum = unDeltedEntities.Sum(a => (decimal)a[columnName]);
                            max = unDeltedEntities.Max(a => (decimal)a[columnName]);
                            min = unDeltedEntities.Min(a => (decimal)a[columnName]);
                        }
                        else
                        {
                            sum = entities.Entities.Sum(a => (decimal)a[columnName]);
                            max = entities.Entities.Max(a => (decimal)a[columnName]);
                            min = entities.Entities.Min(a => (decimal)a[columnName]);
                        }

                    }
                    if (type == typeof(double))
                    {
                        if (isDeleteMessage)
                        {
                            sum = unDeltedEntities.Sum(a => Convert.ToDecimal((double)a[columnName]));
                            max = unDeltedEntities.Max(a => Convert.ToDecimal((double)a[columnName]));
                            min = unDeltedEntities.Min(a => Convert.ToDecimal((double)a[columnName]));
                        }
                        else
                        {
                            sum = entities.Entities.Sum(a => Convert.ToDecimal((double)a[columnName]));
                            max = entities.Entities.Max(a => Convert.ToDecimal((double)a[columnName]));
                            min = entities.Entities.Min(a => Convert.ToDecimal((double)a[columnName]));
                        }

                    }
                    else if (type == typeof(Money))
                    {
                        if (isDeleteMessage)
                        {
                            sum = unDeltedEntities.Sum(a => ((Money)a[columnName]).Value);
                            max = unDeltedEntities.Max(a => ((Money)a[columnName]).Value);
                            min = unDeltedEntities.Min(a => ((Money)a[columnName]).Value);

                        }
                        else
                        {
                            //foreach (var item in entities.Entities)
                            //{
                            //    if (item.Contains(columnName) && item[columnName] != null)
                            //    {
                            //        sum += ((Money)item[columnName]).Value;
                            //    }
                            //}
                            sum = entities.Entities.Sum(a => ((Money)a[columnName]).Value);
                            min = entities.Entities.Min(a => ((Money)a[columnName]).Value);
                            max = entities.Entities.Max(a => ((Money)a[columnName]).Value);
                        }
                    }
                    if (isDeleteMessage)
                    {

                        return unDeltedEntities.Count();

                    }
                    return entities.Entities.Count;
                }
            }
            return 0;
        }
    }
}
