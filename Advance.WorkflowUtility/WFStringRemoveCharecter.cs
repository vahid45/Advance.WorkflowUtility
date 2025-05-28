using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advance.WorkflowUtility
{
    public sealed class WFStringRemoveCharecter : CodeActivity
    {
        [RequiredArgument]
        [Input("String Value")]
        public InArgument<string> Text { get; set; }

        [RequiredArgument]
        [Input("Charecter to Replace")]
        public InArgument<string> Charecter { get; set; }

        [Input("Replace Charecter")]
        public InArgument<string> ReplaceCharecter { get; set; }

        [Output("Output String Value")]
        public OutArgument<string> OutputStringValue { get; set; }
        protected override void Execute(CodeActivityContext context)
        {

            #region "Load CRM Service from context"

            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");

            #endregion
            string editedString = "";
            #region "Read Parameters"
            string text = context.GetValue(this.Text);
            string charecter = context.GetValue(this.Charecter);
            string replaceCharecter = context.GetValue(this.ReplaceCharecter);
            #endregion
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(charecter))
            {
                if (!string.IsNullOrEmpty(replaceCharecter))
                {
                    editedString = text.Replace(charecter, replaceCharecter);

                }
                else
                {
                    editedString = text.Replace(charecter, "");
                }

            }
            context.SetValue(this.OutputStringValue, editedString);
        }
    }
}
