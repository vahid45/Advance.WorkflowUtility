using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;

namespace Helper
{
    public class CRMHelper
    {
        #region propertites
        public IOrganizationService orgService { get; set; }
        public string Message
        {
            get
            {
                return privateMessage;
            }
        }
        public bool WriteInEventViewr { get; set; }
        private string privateMessage { get; set; }

        #endregion

        #region public methodes
        public CRMHelper(IOrganizationService crmOrgService, bool writeInEventViewr)
        {
            this.orgService = crmOrgService;
            this.WriteInEventViewr = writeInEventViewr;
        }

        /// <summary>
        /// Retrieve Entity With All Columns
        /// </summary>
        /// <param name="entityName">Logical Name Of Entity</param>
        /// <param name="id">Record ID</param>
        /// <returns>Return Entity</returns>
        public Entity Select(string entityName, Guid id)
        {
            try
            {
                Entity entity = orgService.Retrieve(entityName, id, new ColumnSet(true));
                if (entity != null)
                {
                    this.privateMessage = "Record Successfuly Retrieve";
                    return entity;
                }

            }
            catch (Exception ex)
            {

                if (WriteInEventViewr)
                {
                    Loging.CreateLog(ex.Message, EventLogEntryType.Error, 100);

                }
                this.privateMessage = ex.Message;
            }
            this.privateMessage = "Dont Fine Any Record";

            return null;

        }

        /// <summary>
        /// Retrieve Entity With Specific Columns
        /// </summary>
        /// <param name="entityName">Logical Name Of Entity</param>
        /// <param name="id">Record ID</param>
        /// <param name="columnSet">Name Of Columns That delimiter By ','</param>
        /// <returns>Return Entity</returns>
        public Entity Select(string entityName, Guid id, string columnSet)
        {
            #region columns
            string[] columns = columnSet.Split(',');
            ColumnSet column = new ColumnSet();
            if (columnSet != null || columnSet == "")
            {
                column.AllColumns = true;
            }
            else
            {
                foreach (var item in columns)
                {
                    column.AddColumn(item);
                }
            }
            #endregion
            try
            {
                Entity entity = orgService.Retrieve(entityName, id, new ColumnSet(true));
                if (entity != null)
                {
                    this.privateMessage = "Record Successfuly Retrieve";
                    return entity;
                }

            }
            catch (Exception ex)
            {

                if (WriteInEventViewr)
                {
                    Loging.CreateLog(ex.Message, EventLogEntryType.Error, 100);

                }
                this.privateMessage = ex.Message;
            }
            this.privateMessage = "Dont Fine Any Record";

            return null;

        }

        /// <summary>
        /// RetrieveMultiple Entity  With All Columns
        /// </summary>
        /// <param name="filter">Query of your retrieve</param>
        /// <param name="entity">Logical Name Of Entity</param>
        /// <returns>Return EntityCollection</returns>
        public EntityCollection Select(string entityName, FilterExpression filter)
        {
            QueryExpression qry = new QueryExpression(entityName);
            qry.ColumnSet = new ColumnSet(true);
            qry.Criteria = filter;
            try
            {
                EntityCollection records = orgService.RetrieveMultiple(qry);
                if (records != null && records.Entities != null && records.Entities.Count > 0)
                {

                    this.privateMessage = "Record Successfuly Retrieve";
                    return records;
                }
            }
            catch (Exception ex)
            {
                if (WriteInEventViewr)
                {
                    Loging.CreateLog(ex.Message, EventLogEntryType.Error, 100);

                }
                this.privateMessage = ex.Message;

            }
            this.privateMessage = "Dont Fine Any Record";

            return null;
        }

        /// <summary>
        /// RetrieveMultiple Entity With Specific Columns
        /// </summary>
        /// <param name="filter">Query of your retrieve</param>
        /// <param name="entityName">Logical Name Of Entity</param>
        /// <param name="columnSet">Name Of Columns That delimiter By ','</param>
        /// <returns>Return EntityCollection</returns>
        public EntityCollection Select(string entityName, FilterExpression filter, string columnSet)
        {
            #region columns
            string[] columns = columnSet.Split(',');
            ColumnSet column = new ColumnSet();
            if (columnSet != null || columnSet == "")
            {
                column.AllColumns = true;
            }
            else
            {
                foreach (var item in columns)
                {
                    column.AddColumn(item);
                }
            }
            #endregion


            QueryExpression qry = new QueryExpression(entityName);
            qry.ColumnSet = column;
            qry.Criteria = filter;


            try
            {
                EntityCollection records = orgService.RetrieveMultiple(qry);

                if (records != null && records.Entities != null && records.Entities.Count > 0)
                {
                    return records;

                }
            }
            catch (Exception)
            {


            }
            return null;

        }
        public string AddManyToMany(Guid targetId, string targetLogicalName, Guid sourceId, string sourceLogicalName, string relationShipName)
        {
            try
            {

                AssociateRequest request = new AssociateRequest();

                EntityReference mon1 = new EntityReference(targetLogicalName, targetId);

                EntityReference mon2 = new EntityReference(sourceLogicalName, sourceId);

                request.Target = mon1;

                request.RelatedEntities = new EntityReferenceCollection { mon2 };

                request.Relationship = new Relationship(relationShipName);

                AssociateResponse response = (AssociateResponse)orgService.Execute(request);

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }


        }
        public void DeleteRecords(EntityCollection entities)
        {
            foreach (Entity item in entities.Entities)
            {
                this.orgService.Delete(item.LogicalName, item.Id);
            }
        }
        public string sGetEntityNameFromCode(string ObjectTypeCode)
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
            RetrieveMetadataChangesResponse response = (RetrieveMetadataChangesResponse)orgService.Execute(retrieveMetadataChangesRequest);

            EntityMetadata entityMetadata = (EntityMetadata)response.EntityMetadata[0];
            return entityMetadata.SchemaName.ToLower();
        }
        #endregion






        //public EntityCollection Select(FilterExpression filter, string entity)
        //{
        //    QueryExpression qry = new QueryExpression(entity);
        //    qry.ColumnSet = new ColumnSet(true);
        //    qry.Criteria = filter;
        //    try
        //    {
        //        EntityCollection records = orgService.RetrieveMultiple(qry);
        //        if (records != null && records.Entities != null && records.Entities.Count > 0)
        //        {
        //            return records;
        //        }
        //    }
        //    catch (Exception)
        //    {


        //    }
        //    return null;
        //}
    }
}