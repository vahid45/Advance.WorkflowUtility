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

    public sealed class WFNoteCheckAttachment : CodeActivity
    {
        [RequiredArgument]
        [Input("Note To Check")]
        [ReferenceTarget("annotation")]
        public InArgument<EntityReference> NoteToCheck { get; set; }

        [Output("Has Attachment")]
        public OutArgument<bool> HasAttachment { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            try
            {
                EntityReference noteToCheck = NoteToCheck.Get(context);

                HasAttachment.Set(context, CheckForAttachment(orgService, noteToCheck.Id));
            }
            catch (Exception ex)
            {
                objCommon.tracingService.Trace("Exception: {0}", ex.ToString());
            }
        }

        private static bool CheckForAttachment(IOrganizationService service, Guid noteId)
        {
            Entity note = service.Retrieve("annotation", noteId, new ColumnSet("isdocument"));

            object oIsDocument;
            bool hasValue = note.Attributes.TryGetValue("isdocument", out oIsDocument);
            if (!hasValue)
                return false;

            return (bool)oIsDocument;
        }
    }
}
