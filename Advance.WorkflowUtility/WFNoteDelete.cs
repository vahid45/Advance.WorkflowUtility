using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFNoteDelete : CodeActivity
    {
        [RequiredArgument]
        [Input("Note To Delete")]
        [ReferenceTarget("annotation")]
        public InArgument<EntityReference> NoteToDelete { get; set; }

        [Output("Was Note Deleted")]
        public OutArgument<bool> WasNoteDeleted { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(executionContext);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            try
            {
                EntityReference noteToDelete = NoteToDelete.Get(executionContext);

                orgService.Delete("annotation", noteToDelete.Id);

                WasNoteDeleted.Set(executionContext, true);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}
