using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;


namespace Advance.WorkflowUtility
{


    public class WFCalculateRollupField : CodeActivity
    {
        #region "Parameter Definition"
        [RequiredArgument]
        [Input("FieldName")]
        [Default("")]
        public InArgument<String> FieldName { get; set; }

        [RequiredArgument]
        [Input("Parent Record URL")]
        [ReferenceTarget("")]
        public InArgument<String> ParentRecordURL { get; set; }

        [Output("Result")]
        public OutArgument<decimal> Result{ get; set; }


        #endregion

        protected override void Execute(CodeActivityContext executionContext)
        {

            #region "Load CRM Service from context"

            Common objCommon = new Common(executionContext);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            #endregion

            #region "Read Parameters"
            String _FieldName = this.FieldName.Get(executionContext);
            String _ParentRecordURL = this.ParentRecordURL.Get(executionContext);
            if (_ParentRecordURL == null || _ParentRecordURL == "")
            {
                return;
            }
            string[] urlParts = _ParentRecordURL.Split("?".ToArray());
            string[] urlParams = urlParts[1].Split("&".ToCharArray());
            string ParentObjectTypeCode = urlParams[0].Replace("etc=", "");
            string ParentId = urlParams[1].Replace("id=", "");
            objCommon.tracingService.Trace("ParentObjectTypeCode=" + ParentObjectTypeCode + "--ParentId=" + ParentId);
            #endregion


            #region "CalculateRollupField Execution"
            string ParentEntityName = objCommon.sGetEntityNameFromCode(ParentObjectTypeCode, objCommon.orgService);
            CalculateRollupFieldRequest calculateRollup = new CalculateRollupFieldRequest();
            calculateRollup.FieldName = _FieldName;
            calculateRollup.Target = new EntityReference(ParentEntityName, new Guid(ParentId));
            CalculateRollupFieldResponse resp = (CalculateRollupFieldResponse)objCommon.orgService.Execute(calculateRollup);
            if (resp != null && resp.Entity != null)
            {
                Entity requestedEntity = resp.Entity;
                if (requestedEntity.Contains(_FieldName) && requestedEntity[_FieldName] != null)
                {
                    var sumColumn = requestedEntity[_FieldName];
                    System.Type type = sumColumn.GetType();
                    if (type == typeof(decimal))
                    {
                        decimal? result = Convert.ToDecimal(requestedEntity[_FieldName]);
                        if (result.HasValue)
                        {
                            this.Result.Set(executionContext, result.Value);
                        }
                    }
                    else if (type == typeof(Money))
                    {
                        decimal? result = Convert.ToDecimal(((Money)requestedEntity[_FieldName]).Value);
                        if (result.HasValue)
                        {
                            this.Result.Set(executionContext, result.Value);
                        }
                    }

                }
            }
            #endregion

        }


    }
}
