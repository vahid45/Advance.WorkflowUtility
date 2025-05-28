using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Helper
{
    public class CRMUtilityHelper
    {
        private IOrganizationService orgService { get; set; }
        public CRMUtilityHelper(IOrganizationService CrmOrgService)
        {
            this.orgService = CrmOrgService;
        }
        public void DeactiveRecord(Guid recordId, string entityName, int stateCode, int statusCode)
        {
            var cols = new ColumnSet(new[] { "statecode", "statuscode" });

            //Check if it is Active or not
            var entity = orgService.Retrieve(entityName, recordId, cols);

            if (entity != null && entity.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
            {
                //StateCode = 1 and StatusCode = 2 for deactivating Account or Contact
                SetStateRequest setStateRequest = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = recordId,
                        LogicalName = entityName,
                    },
                    State = new OptionSetValue(stateCode),
                    Status = new OptionSetValue(statusCode)
                };
                orgService.Execute(setStateRequest);
            }
        }

        /// <summary>
        /// This Method Return bool That Say This User in this roles Or Not
        /// </summary>
        /// <param name="roleNames">Splite By ","</param>
        /// <param name="userId">"User Id That You Want Check It"</param>
        /// <returns></returns>
        public bool UserInRoles(string roleNames, Guid userId)
        {
            EntityCollection filterRoles = GetFilterRoleByeName(orgService, roleNames.Split(','));
            if (filterRoles != null)
            {
                Guid[] filterRoleIds = new Guid[filterRoles.Entities.Count];
                int i = 0;
                foreach (Entity role in filterRoles.Entities)
                {
                    filterRoleIds[i] = role.Id;
                    i += 1;
                }


                EntityCollection roles = GetUserRoles(orgService, userId);
                if (roles != null && roles.Entities != null)
                {
                    foreach (Entity role in roles.Entities)
                    {
                        Guid roleId = (Guid)role["roleid"];
                        if (FindInRoleFilter(filterRoleIds, roleId))
                        {
                            return true;
                        }
                    }
                }

            }

            return false;

        }
        public string GetEntityNameFromCode(string ObjectTypeCode, IOrganizationService service)
        {
            MetadataFilterExpression entityFilter = new MetadataFilterExpression(LogicalOperator.And);
            entityFilter.Conditions.Add(new MetadataConditionExpression("ObjectTypeCode", MetadataConditionOperator.Equals, Convert.ToInt32(ObjectTypeCode)));
            EntityQueryExpression entityQueryExpression = new EntityQueryExpression()
            {
                Criteria = entityFilter
            };
            RetrieveMetadataChangesRequest retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest()
            {
                Query = entityQueryExpression,
                ClientVersionStamp = null
            };
            RetrieveMetadataChangesResponse response = (RetrieveMetadataChangesResponse)service.Execute(retrieveMetadataChangesRequest);

            EntityMetadata entityMetadata = (EntityMetadata)response.EntityMetadata[0];
            return entityMetadata.SchemaName.ToLower();
        }
        public QueryExpression ConvertFetchExpressionToQueryExpression(string fetchXml)
        {
            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = fetchXml
            };
            FetchXmlToQueryExpressionResponse response = (FetchXmlToQueryExpressionResponse)orgService.Execute(conversionRequest);
            return response.Query;
        }
        public Guid GetInfoFromRecordURL(string url, out string objectTypeCode, out string entityName)
        {
            CRMHelper helper = new CRMHelper(orgService, false);
            string[] urlParts = url.Split("?".ToArray());
            string[] urlParams = urlParts[1].Split("&".ToCharArray());
            objectTypeCode = urlParams[0].Replace("etc=", "");
            entityName = helper.sGetEntityNameFromCode(objectTypeCode);
            string id = urlParams[1].Replace("id=", "");
            return new Guid(id);
        }
        public string GetConfig(string key)
        {
            QueryExpression qry = new QueryExpression("new_config");
            qry.ColumnSet = new ColumnSet(true);
            qry.Criteria.AddCondition("new_name", ConditionOperator.Equal, key);
            try
            {
                EntityCollection configs = orgService.RetrieveMultiple(qry);
                if (configs != null && configs.Entities != null && configs.Entities.Count > 0)
                {
                    Entity config = configs.Entities.First();
                    if (config.Contains("new_value"))
                    {
                        return (string)config["new_value"];
                    }
                }
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Retrieve Entity With All Columns
        /// </summary>
        /// <param name="entityName">Logical Name Of Entity</param>
        /// <param name="MessageName">Like Create,Update,...</param>
        /// <param name="friendlyName">FriendlyName Of Plugin</param>
        /// <param name="stage">Stage:Prevalidation = 10 ,PreOpration = 20 , PostOpration = 40 </param>
        /// <param name="mode">Mode:Sync = 0 ,Async = 1 </param>
        /// <returns>Return Entity</returns>
        public Guid CreateSdkMessageStep(string entityName, string MessageName, string friendlyName, int stage, int mode)
        {
            //EntityReference sdkMessage = GetSDKMessage("RetrieveMultiple");
            EntityReference sdkMessage = GetSDKMessage(MessageName);
            if (sdkMessage != null)
            {
                //EntityReference pluginType = GetPluginType("  ");
                EntityReference pluginType = GetPluginType(friendlyName);
                if (pluginType != null)
                {
                    EntityReference filteredEntity = getFilteredEntity(entityName, sdkMessage.Id);
                    return CreateSdkMessageStep(sdkMessage, pluginType, filteredEntity, MessageName + " of " + entityName, stage, mode);
                }
            }
            return Guid.Empty;
        }
        public void DeleteSdkMessage(Guid sdkMessageId)
        {
            QueryExpression qry = new QueryExpression("sdkmessageprocessingstep");
            qry.Criteria.AddCondition("sdkmessageprocessingstepid", ConditionOperator.Equal, sdkMessageId);
            EntityCollection messages = orgService.RetrieveMultiple(qry);
            if (messages != null && messages.Entities != null && messages.Entities.Count > 0)
            {
                orgService.Delete(messages.Entities.First().LogicalName, messages.Entities.First().Id);
            }

        }
        private Guid CreateSdkMessageStep(EntityReference sdkMessage, EntityReference pluginType, EntityReference filteredEntity, string messageName, int stage, int mode)
        {
            Entity sdkMessageStep = new Entity("sdkmessageprocessingstep");
            sdkMessageStep["name"] = messageName;
            sdkMessageStep["mode"] = new OptionSetValue(0);//mode 0=sync,1 =asynv
            sdkMessageStep["rank"] = 1;
            sdkMessageStep["stage"] = new OptionSetValue(stage); //10 prevalidation , 20 pre opration , 40 postopration
            ; sdkMessageStep["supporteddeployment"] = new OptionSetValue(0);
            sdkMessageStep["invocationsource"] = new OptionSetValue(0);
            sdkMessageStep["plugintypeid"] = pluginType;
            sdkMessageStep["sdkmessageid"] = sdkMessage;
            if (filteredEntity != null)
            {
                sdkMessageStep["sdkmessagefilterid"] = filteredEntity;
            }
            try
            {
                return orgService.Create(sdkMessageStep);
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }

        }

        private EntityReference getFilteredEntity(string entityName, Guid messageId)
        {
            QueryExpression qryFilterdEntity = new QueryExpression("sdkmessagefilter");
            qryFilterdEntity.ColumnSet = new ColumnSet(true);
            qryFilterdEntity.Criteria.AddCondition("primaryobjecttypecode", ConditionOperator.Equal, entityName);
            qryFilterdEntity.Criteria.AddCondition("sdkmessageid", ConditionOperator.Equal, messageId);
            EntityCollection filterEntites = orgService.RetrieveMultiple(qryFilterdEntity);
            if (filterEntites != null && filterEntites.Entities != null && filterEntites.Entities.Count > 0)
            {
                return filterEntites.Entities.First().ToEntityReference();
            }
            return null;
        }

        private EntityReference GetPluginType(string friendlyname)
        {
            QueryExpression qryPluginType = new QueryExpression("plugintype");
            qryPluginType.ColumnSet = new ColumnSet(true);
            qryPluginType.Criteria.AddCondition("friendlyname", ConditionOperator.Equal, friendlyname);
            EntityCollection types = orgService.RetrieveMultiple(qryPluginType);
            if (types != null && types.Entities != null && types.Entities.Count > 0)
            {
                return types.Entities.First().ToEntityReference();
            }
            return null;
        }

        private EntityReference GetSDKMessage(string message)
        {
            QueryExpression qrySDKMessage = new QueryExpression("sdkmessage");
            qrySDKMessage.ColumnSet = new ColumnSet(true);
            qrySDKMessage.Criteria.AddCondition("name", ConditionOperator.Equal, message);
            EntityCollection messages = orgService.RetrieveMultiple(qrySDKMessage);
            if (messages != null && messages.Entities != null && messages.Entities.Count > 0)
            {
                return messages.Entities.First().ToEntityReference();
            }
            return null;
        }



        #region private method

        private EntityCollection GetFilterRoleByeName(IOrganizationService organizationService, string[] rolesName)
        {
            QueryExpression qry = new QueryExpression("role");
            qry.ColumnSet = new ColumnSet("roleid", "name");
            FilterExpression filter1 = new FilterExpression(LogicalOperator.Or);
            foreach (string roleName in rolesName)
            {
                filter1.AddCondition("name", ConditionOperator.Equal, roleName);
            }
            qry.Criteria = filter1;
            EntityCollection roles = organizationService.RetrieveMultiple(qry);
            if (roles != null && roles.Entities != null && roles.Entities.Count > 0)
            {
                return roles;
            }
            return null;
        }
        private EntityCollection GetUserRoles(IOrganizationService orgService, Guid userId)
        {
            QueryExpression qe = new QueryExpression();
            qe.EntityName = "role";
            qe.ColumnSet = new ColumnSet("roleid");

            // Set up the join between the "role" entity and the intersect table "systemuserroles".
            LinkEntity le = new LinkEntity();
            le.LinkFromEntityName = "role";
            le.LinkFromAttributeName = "roleid";
            le.LinkToEntityName = "systemuserroles";
            le.LinkToAttributeName = "roleid";

            // Set up the join between the intersect table "systemuserroles" and the "systemuser" entity.
            LinkEntity le2 = new LinkEntity();
            le2.LinkFromEntityName = "systemuserroles";
            le2.LinkFromAttributeName = "systemuserid";
            le2.LinkToEntityName = "systemuser";
            le2.LinkToAttributeName = "systemuserid";

            ConditionExpression ce = new ConditionExpression("systemuserid", ConditionOperator.Equal, userId);
            le2.LinkCriteria = new FilterExpression();
            le2.LinkCriteria.Conditions.Add(ce);

            le.LinkEntities.Add(le2);
            qe.LinkEntities.Add(le);

            return orgService.RetrieveMultiple(qe);
        }
        private bool FindInRoleFilter(Guid[] filterRoleIds, Guid roleId)
        {
            foreach (Guid filterRoleId in filterRoleIds)
            {
                if (filterRoleId == roleId)
                    return true;
            }
            return false;
        }
        #endregion

        #region enums
        #endregion
    }
}