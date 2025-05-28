using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFNumberToWords : CodeActivity
    {
        [Input("Decimal Number")]
        public InArgument<decimal> DecimalNumber { get; set; }

        [Input("Int Number")]
        public InArgument<int> IntNumber { get; set; }

        [RequiredArgument]
        [Input("Lang:1-Persian 2-English")]
        public InArgument<int> Lang { get; set; }

        [Output("Number To Word")]
        public OutArgument<string> NumberToWord { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            decimal? decimalNumber = context.GetValue(this.DecimalNumber);
            int? intNumber = context.GetValue(this.IntNumber);
            int lang = context.GetValue(this.Lang);
            string word = "";
            if (lang == 2)
            {
                if (decimalNumber.HasValue)
                {
                    word = Classes.PersianTools.HumanReadableInteger.NumberToText(decimalNumber.Value, Classes.PersianTools.Language.English);

                }
                if (intNumber.HasValue)
                {
                    word = Classes.PersianTools.HumanReadableInteger.NumberToText(intNumber.Value, Classes.PersianTools.Language.English);

                }
            }
            else
            {
                if (decimalNumber.HasValue)
                {
                    word = Classes.PersianTools.HumanReadableInteger.NumberToText(decimalNumber.Value, Classes.PersianTools.Language.Persian);

                }
                if (intNumber.HasValue)
                {
                    word = Classes.PersianTools.HumanReadableInteger.NumberToText(intNumber.Value, Classes.PersianTools.Language.Persian);

                }
            }
            context.SetValue(this.NumberToWord, word);
        }
    }
}
