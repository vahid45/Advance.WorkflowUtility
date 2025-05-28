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

    public sealed class WFNoteDeleteAttachmentByName : CodeActivity
    {
        [RequiredArgument]
        [Input("Note With Attachment To Remove")]
        [ReferenceTarget("annotation")]
        public InArgument<EntityReference> NoteWithAttachment { get; set; }

        [RequiredArgument]
        [Input("File Name With Extension")]
        public InArgument<string> FileName { get; set; }

        [RequiredArgument]
        [Input("Add Delete Notice As Text?")]
        [Default("false")]
        public InArgument<bool> AppendNotice { get; set; }

        [Output("Number Of Attachments Deleted")]
        public OutArgument<int> NumberOfAttachmentsDeleted { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService service = objCommon.orgService;
            #endregion

            try
            {
                EntityReference noteWithAttachment = NoteWithAttachment.Get(context);
                string fileName = FileName.Get(context);
                bool appendNotice = AppendNotice.Get(context);

                Entity note = GetNote(service, noteWithAttachment.Id);
                if (!CheckForAttachment(note))
                    return;

                StringBuilder notice = new StringBuilder();
                int numberOfAttachmentsDeleted = 0;

                if (String.Equals(note.GetAttributeValue<string>("filename"), fileName, StringComparison.CurrentCultureIgnoreCase))
                {
                    numberOfAttachmentsDeleted++;

                    if (appendNotice)
                        notice.AppendLine("Deleted Attachment: " + note.GetAttributeValue<string>("filename") + " " +
                                          DateTime.Now.ToShortDateString());

                    UpdateNote(service, note, notice.ToString());
                }

                NumberOfAttachmentsDeleted.Set(context, numberOfAttachmentsDeleted);
            }
            catch (Exception ex)
            {
                objCommon.tracingService.Trace("Exception: {0}", ex.ToString());
            }
        }

        private static bool CheckForAttachment(Entity note)
        {
            object oIsAttachment;
            bool hasValue = note.Attributes.TryGetValue("isdocument", out oIsAttachment);
            if (!hasValue)
                return false;

            return (bool)oIsAttachment;
        }

        private static Entity GetNote(IOrganizationService service, Guid noteId)
        {
            return service.Retrieve("annotation", noteId, new ColumnSet("filename", "isdocument", "notetext"));
        }

        private void UpdateNote(IOrganizationService service, Entity note, string notice)
        {
            Entity updateNote = new Entity("annotation");
            updateNote.Id = note.Id;
            if (!string.IsNullOrEmpty(notice))
            {
                string newText = note.GetAttributeValue<string>("notetext");
                if (!string.IsNullOrEmpty(newText))
                    newText += "\r\n";

                updateNote["notetext"] = newText + notice;
            }
            updateNote["isdocument"] = false;
            updateNote["filename"] = null;
            updateNote["documentbody"] = null;
            updateNote["filesize"] = null;

            service.Update(updateNote);
        }
    }
}
