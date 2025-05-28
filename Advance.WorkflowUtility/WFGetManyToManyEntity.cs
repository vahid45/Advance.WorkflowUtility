using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.ObjectModel;
using Microsoft.Crm.Sdk.Messages;

namespace Advance.WorkflowUtility
{

    public sealed class WFGetManyToManyEntity : CodeActivity
    {
        [RequiredArgument]
        [Input("Relation Name")]
        public InArgument<string> RelationName { get; set; }

        [RequiredArgument]
        [Input("To Entity Name")]
        public InArgument<string> ToEntity { get; set; }

        [RequiredArgument]
        [Input("Current Record URL")]
        public InArgument<string> CurrentRecordURL { get; set; }
        [RequiredArgument]
        [Input("ToEntity ETC")]
        public InArgument<string> ETCCode { get; set; }

        [RequiredArgument]
        [Input("ServerUrl")]
        [Default("http://localhost/Org/")]
        public InArgument<string> ServerURL { get; set; }


        [Output("Dynamic URL")]
        public OutArgument<string> DynamicURL { get; set; }
        [Output("ID")]
        public OutArgument<string> ID { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string toEntity = context.GetValue(this.ToEntity);
            string relationName = context.GetValue(this.RelationName);
            string serverUrl = context.GetValue(this.ServerURL);
            string currentRecordURL = context.GetValue(this.CurrentRecordURL);
            string etc = "";
            string toEtc = context.GetValue(this.ETCCode);

            Entity parent = internalExecuter(orgService, relationName, currentRecordURL);
            if (parent != null && parent.Contains("toEntity." + toEntity + "id"))
            {
                AliasedValue idAlias = (AliasedValue)parent["toEntity." + toEntity + "id"];
                string id = ((Guid)idAlias.Value).ToString();
                this.ID.Set(context, id);
                string url = serverUrl + "main.aspx?etc=" + toEtc + "&id=" + id;
                this.DynamicURL.Set(context, url);
            }


        }


        public Entity internalExecuter(IOrganizationService orgService, string relationName, string currentRecordURL)
        {
            string etc = "";
            Helper.CRMUtilityHelper utHelper = new Helper.CRMUtilityHelper(orgService);
            string fromEntity = "";
            Guid fromEntityId = utHelper.GetInfoFromRecordURL(currentRecordURL, out etc, out fromEntity);

            QueryExpression query = new QueryExpression(fromEntity);
            query.Criteria.AddCondition(fromEntity + "id", ConditionOperator.Equal, fromEntityId);
            LinkEntity link = new LinkEntity(fromEntity, relationName, fromEntity + "id", fromEntity + "id", JoinOperator.Inner);
            link.Columns = new ColumnSet(true);
            link.EntityAlias = "toEntity";
            query.LinkEntities.Add(link);
            EntityCollection manyToManies = orgService.RetrieveMultiple(query);
            if (manyToManies != null && manyToManies.Entities != null && manyToManies.Entities.Count > 0)
            {
                return manyToManies.Entities.First();
            }
            return null;
        }
    }
}

