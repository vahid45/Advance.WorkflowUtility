using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFNoteUpdateText : CodeActivity
    {
        [RequiredArgument]
        [Input("Note To Update")]
        [ReferenceTarget("annotation")]
        public InArgument<EntityReference> NoteToUpdate { get; set; }

        [RequiredArgument]
        [Input("New Text")]
        public InArgument<string> NewText { get; set; }

        [Output("Updated Text")]
        public OutArgument<string> UpdatedText { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            try
            {
                EntityReference noteToUpdate = NoteToUpdate.Get(context);
                string newText = NewText.Get(context);

                Entity note = new Entity("annotation");
                note.Id = noteToUpdate.Id;
                note["notetext"] = newText;
                orgService.Update(note);

                UpdatedText.Set(context, newText);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
