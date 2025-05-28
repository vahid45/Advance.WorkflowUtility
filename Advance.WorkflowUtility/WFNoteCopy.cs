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

    public sealed class WFNoteCopy : CodeActivity
    {
        [RequiredArgument]
        [Input("Note To Copy")]
        [ReferenceTarget("annotation")]
        public InArgument<EntityReference> NoteToCopy { get; set; }

        [Input("Record Dynamic Url")]
        [RequiredArgument]
        public InArgument<string> RecordUrl { get; set; }

        [RequiredArgument]
        [Default("True")]
        [Input("Copy Attachment?")]
        public InArgument<bool> CopyAttachment { get; set; }

        [Output("Was Note Copied")]
        public OutArgument<bool> WasNoteCopied { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            try
            {
                EntityReference noteToCopy = NoteToCopy.Get(context);
                string recordUrl = RecordUrl.Get<string>(context);
                bool copyAttachment = CopyAttachment.Get(context);

                var dup = new DynamicUrlParser(recordUrl);

                string newEntityLogical = dup.GetEntityLogicalName(orgService);

                Entity note = GetNote(orgService, noteToCopy.Id);
                if (note.GetAttributeValue<EntityReference>("objectid").Id == dup.Id && note.GetAttributeValue<EntityReference>("objectid").LogicalName == newEntityLogical)
                {
                    WasNoteCopied.Set(context, false);
                    return;
                }

                Entity newNote = new Entity("annotation");
                newNote["objectid"] = new EntityReference(newEntityLogical, dup.Id);
                newNote["notetext"] = note.GetAttributeValue<string>("notetext");
                newNote["subject"] = note.GetAttributeValue<string>("subject");
                if (copyAttachment)
                {
                    newNote["isdocument"] = note.GetAttributeValue<bool>("isdocument");
                    newNote["filename"] = note.GetAttributeValue<string>("filename");
                    newNote["filesize"] = note.GetAttributeValue<int>("filesize");
                    newNote["documentbody"] = note.GetAttributeValue<string>("documentbody");
                }
                else
                    newNote["isdocument"] = false;

                orgService.Create(newNote);

                WasNoteCopied.Set(context, true);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        private Entity GetNote(IOrganizationService service, Guid noteId)
        {
            return service.Retrieve("annotation", noteId, new ColumnSet("objectid", "documentbody", "filename", "filesize", "isdocument", "notetext", "subject"));
        }
    }
}
