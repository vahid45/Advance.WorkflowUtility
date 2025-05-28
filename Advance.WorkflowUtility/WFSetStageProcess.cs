﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;

namespace Advance.WorkflowUtility
{

    public sealed class WFSetStageProcess : CodeActivity
    {
        [RequiredArgument]
        [Input("Record URL")]
        [ReferenceTarget("")]
        public InArgument<String> ClonningRecordURL { get; set; }

        [Input("Process")]
        [ReferenceTarget("workflow")]
        public InArgument<EntityReference> Process { get; set; }

        [Input("Process Stage Name")]
        public InArgument<string> ProcessStage { get; set; }



        protected override void Execute(CodeActivityContext executionContext)
        {
            #region "Load CRM Service from context"

            Common objCommon = new Common(executionContext);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            #endregion

            #region "Read Parameters"
            String _ClonningRecordURL = this.ClonningRecordURL.Get(executionContext);
            if (_ClonningRecordURL == null || _ClonningRecordURL == "")
            {
                return;
            }
            string[] urlParts = _ClonningRecordURL.Split("?".ToArray());
            string[] urlParams = urlParts[1].Split("&".ToCharArray());
            string objectTypeCode = urlParams[0].Replace("etc=", "");
            string entityName = objCommon.sGetEntityNameFromCode(objectTypeCode, objCommon.orgService);
            string objectId = urlParams[1].Replace("id=", "");
            objCommon.tracingService.Trace("ObjectTypeCode=" + objectTypeCode + "--ParentId=" + objectId);

            EntityReference process = this.Process.Get(executionContext);
            string processStage = this.ProcessStage.Get(executionContext);

            #endregion

            #region "SetProcessStage Execution"

            string stageName = processStage;

            Guid? stageId = null;
            if (processStage != null)
            {
                objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] Process stage: " + stageName);
                Entity stageReference = new Entity("processstage");

                QueryExpression queryStage = new QueryExpression("processstage");
                queryStage.ColumnSet = new ColumnSet();
                queryStage.Criteria.AddCondition(new ConditionExpression(
                        "stagename",
                        ConditionOperator.Equal,
                        stageName));

                queryStage.Criteria.AddCondition(new ConditionExpression(
                        "processid",
                        ConditionOperator.Equal,
                        process.Id));

                objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] Fetching the requested Stage.");
                try
                {
                    stageReference = objCommon.orgService.RetrieveMultiple(queryStage).Entities.FirstOrDefault();
                    if (stageReference == null)
                    {
                        throw new InvalidPluginExecutionException(
                            "Process stage " + stageName + " not found");
                    }

                    stageId = stageReference.Id;
                }
                catch (Exception e)
                {
                    objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] Error trying to retrieve " +
                        "the requested stage. Exception: " + e.ToString());
                    throw new InvalidPluginExecutionException("An error occurred while trying to fetch process stage " +
                        stageName +
                    ". Exception message: " + e.Message + ". Inner Exception: " + e.ToString());
                }
            }
            // Set the active process and the phase if defined
            EntityReference objectReference = new EntityReference(entityName, new Guid(objectId));

            Entity entityToUpdate = new Entity();

            if (objectReference != null)
            {
                entityToUpdate.LogicalName = objectReference.LogicalName;
                entityToUpdate.Id = objectReference.Id;

                objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] Case tu update Id = " + objectReference.Id.ToString() + ", Name = " + objectReference.Name);
            }
            else
            {
                entityToUpdate.LogicalName = objCommon.workflowContext.PrimaryEntityName;
                entityToUpdate.Id = objCommon.workflowContext.PrimaryEntityId;

                objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] Case tu update Id = " + entityToUpdate.Id.ToString());
            }

            entityToUpdate["processid"] = process.Id;
            entityToUpdate["stageid"] = stageId.HasValue
                ? stageId.Value : default(Guid);

            try
            {
                objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] Updating " +
                    " Case to Update Id = " + entityToUpdate.Id +
                    " Process Id = " + process.Id + " | Process Name = " + process.Name +
                    " Stage Id = " + stageId.Value.ToString());

                objCommon.orgService.Update(entityToUpdate);

                objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] Case Id = " + entityToUpdate.Id.ToString() + " updated successfully.");
            }
            catch (Exception e)
            {
                objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] Error while setting " +
                    "the active BPF. Details: " + e.ToString());
                throw new InvalidPluginExecutionException("An error occurred while trying to update Business Process to Case Id = " + entityToUpdate.Id.ToString() +
                    "Exception message " + e.Message +
                    " Inner exception: " + e.ToString());
            }
            objCommon.tracingService.Trace("[Dynamics.ChangeBPFandPhase.Execute] End.");

            #endregion

        }
    }
}
