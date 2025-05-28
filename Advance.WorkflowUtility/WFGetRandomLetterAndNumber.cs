using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFGetRandomLetterAndNumber : CodeActivity
    {
        [RequiredArgument]
        [Input("Size")]
        [Default("10")]
        public InArgument<int> Size { get; set; }

        [Input("Type:1-Onlyletter 2-OnlyNo 3-letterNo")]
        
        public InArgument<int> Type { get; set; }


        [Output("Random Letter")]
        public OutArgument<string> RandomLetter { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            int? size = context.GetValue(this.Size);
            int? type = context.GetValue(this.Type);
            if (type.HasValue)
            {
                type = 3;
            }
            this.RandomLetter.Set(context, RandomString(size.Value, type.Value));

        }
        private static Random random = new Random();
        private string RandomString(int Size, int type)
        {
            string input = "";
            if (type == 1)
            {
                input = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            }
            else if (type == 2)
            {
                input = "0123456789";
            }
            else
            {
                input = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            }
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < Size; i++)
            {
                ch = input[random.Next(0, input.Length)];
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}
