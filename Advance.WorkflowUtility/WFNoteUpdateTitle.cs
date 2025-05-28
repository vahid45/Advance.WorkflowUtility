using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFNoteUpdateTitle : CodeActivity
    {
        [RequiredArgument]
        [Input("Note To Update")]
        [ReferenceTarget("annotation")]
        public InArgument<EntityReference> NoteToUpdate { get; set; }

        [RequiredArgument]
        [Input("New Title")]
        public InArgument<string> NewTitle { get; set; }

        [Output("Updated Title")]
        public OutArgument<string> UpdatedTitle { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(executionContext);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            try
            {
                EntityReference noteToUpdate = NoteToUpdate.Get(executionContext);
                string newTitle = NewTitle.Get(executionContext);

                Entity note = new Entity("annotation");
                note.Id = noteToUpdate.Id;
                note["subject"] = newTitle;
                orgService.Update(note);

                UpdatedTitle.Set(executionContext, newTitle);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
