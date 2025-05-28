using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advance.WorkflowUtility
{
    public sealed class WFNoteDeleteAttachment: CodeActivity
    {
        [RequiredArgument]
        [Input("Note With Attachments To Remove")]
        [ReferenceTarget("annotation")]
        public InArgument<EntityReference> NoteWithAttachment { get; set; }

        [Input("Delete >= Than X Bytes (Empty = 2,147,483,647)")]
        public InArgument<int> DeleteSizeMax { get; set; }

        [Input("Delete <= Than X Bytes (Empty = 0)")]
        public InArgument<int> DeleteSizeMin { get; set; }

        [Input("Limit To Extensions (Comma Delimited, Empty = Ignore)")]
        public InArgument<string> Extensions { get; set; }

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
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            try
            {
                EntityReference noteWithAttachment = NoteWithAttachment.Get(context);
                int deleteSizeMax = DeleteSizeMax.Get(context);
                int deleteSizeMin = DeleteSizeMin.Get(context);
                string extensions = Extensions.Get(context);
                bool appendNotice = AppendNotice.Get(context);

                if (deleteSizeMax == 0) deleteSizeMax = int.MaxValue;
                if (deleteSizeMin > deleteSizeMax)
                {
                    objCommon.tracingService.Trace("Exception: {0}", "Min:" + deleteSizeMin + " Max:" + deleteSizeMax);
                    throw new InvalidPluginExecutionException("Minimum Size Cannot Be Greater Than Maximum Size");
                }

                Entity note = GetNote(orgService, noteWithAttachment.Id);
                if (!CheckForAttachment(note))
                    return;

                string[] filetypes = new string[0];
                if (!string.IsNullOrEmpty(extensions))
                    filetypes = extensions.Replace(".", string.Empty).Split(',');

                StringBuilder notice = new StringBuilder();
                int numberOfAttachmentsDeleted = 0;

                bool delete = false;

                if (note.GetAttributeValue<int>("filesize") >= deleteSizeMax)
                    delete = true;

                if (note.GetAttributeValue<int>("filesize") <= deleteSizeMin)
                    delete = true;

                if (filetypes.Length > 0 && delete)
                    delete = ExtensionMatch(filetypes, note.GetAttributeValue<string>("filename"));

                if (delete)
                {
                    numberOfAttachmentsDeleted++;

                    if (appendNotice)
                        notice.AppendLine("Deleted Attachment: " + note.GetAttributeValue<string>("filename") + " " +
                                          DateTime.Now.ToShortDateString());

                    UpdateNote(orgService, note, notice.ToString());
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
            return service.Retrieve("annotation", noteId, new ColumnSet("filename", "filesize", "isdocument", "notetext"));
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

        private static bool ExtensionMatch(IEnumerable<string> extenstons, string filename)
        {
            foreach (string ex in extenstons)
            {
                if (filename.EndsWith("." + ex))
                    return true;
            }
            return false;
        }
    }
}
