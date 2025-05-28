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

    public sealed class WFNoteMove : CodeActivity
    {
        [RequiredArgument]
        [Input("Note To Move")]
        [ReferenceTarget("annotation")]
        public InArgument<EntityReference> NoteToMove { get; set; }

        [Input("Record Dynamic Url")]
        [RequiredArgument]
        public InArgument<string> RecordUrl { get; set; }

        [Output("Was Note Moved")]
        public OutArgument<bool> WasNoteMoved { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            try
            {
                EntityReference noteToMove = NoteToMove.Get(context);
                string recordUrl = RecordUrl.Get<string>(context);

                var dup = new DynamicUrlParser(recordUrl);

                string newEntityLogical = dup.GetEntityLogicalName(orgService);

                Entity note = GetNote(orgService, noteToMove.Id);
                if (note.GetAttributeValue<EntityReference>("objectid").Id == dup.Id && note.GetAttributeValue<EntityReference>("objectid").LogicalName == newEntityLogical)
                {
                    WasNoteMoved.Set(context, false);
                    return;
                }

                Entity updateNote = new Entity("annotation");
                updateNote.Id = noteToMove.Id;
                updateNote["objectid"] = new EntityReference(newEntityLogical, dup.Id);

                orgService.Update(updateNote);

                WasNoteMoved.Set(context, true);
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        private Entity GetNote(IOrganizationService service, Guid noteId)
        {
            return service.Retrieve("annotation", noteId, new ColumnSet("objectid", "objecttypecode"));
        }
    }
}
