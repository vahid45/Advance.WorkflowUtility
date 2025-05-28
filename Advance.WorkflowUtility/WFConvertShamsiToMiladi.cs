using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Globalization;

namespace Advance.WorkflowUtility
{

    public sealed class WFConvertShamsiToMiladi : CodeActivity
    {
        [RequiredArgument]
        [Input("Shamsi Date")]
        public InArgument<string> ShamsiDate { get; set; }

        [RequiredArgument]
        [Input("Send Error")]
        public InArgument<bool> SendError { get; set; }

        [Output("MiladiDate")]
        public OutArgument<DateTime> MiladiDate { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string shamsiDateStr = context.GetValue(this.ShamsiDate);
            bool sendError = context.GetValue(this.SendError);
            DateTime? miladiDate = ConvertShamsiToMiladi(shamsiDateStr, sendError);
            if (miladiDate.HasValue)
            {
                this.MiladiDate.Set(context, miladiDate.Value);
            }
            else
            {
                if (sendError)
                {
                    throw new InvalidPluginExecutionException("فرمت تاریخ ورودی اشتباه است");
                }
            }
        }

        private DateTime? ConvertShamsiToMiladi(string shamsiDateStr, bool sendError)
        {
            DateTime? miladiDate = null;
            if (!string.IsNullOrEmpty(shamsiDateStr) && shamsiDateStr.Length >= 6 && shamsiDateStr.Length <= 10)
            {
                string year = "";
                string month = "";
                string day = "";
                if (shamsiDateStr.Contains("/"))
                {
                    string[] spiltedDate = shamsiDateStr.Split('/');
                    if (spiltedDate.Length == 3)
                    {
                        for (int i = 0; i < spiltedDate.Length; i++)
                        {
                            if (i == 0)
                            {
                                year = spiltedDate[i];
                                if (year.Length == 2)
                                {
                                    year = "13" + year;
                                }
                            }
                            else if (i == 1)
                            {
                                month = spiltedDate[i];
                                if (month.Length == 1)
                                {
                                    month = "0" + month;
                                }
                            }
                            else if (i == 2)
                            {
                                day = spiltedDate[i];
                                if (day.Length == 1)
                                {
                                    day = "0" + day;
                                }
                            }
                        }

                        PersianCalendar calendar = new PersianCalendar();
                        miladiDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), calendar);


                    }
                    else
                    {
                        if (sendError)
                        {
                            throw new InvalidPluginExecutionException("فرمت تاریخ ورودی اشتباه است");
                        }


                    }
                }
            }
            else
            {
                if (sendError)
                {
                    throw new InvalidPluginExecutionException("فرمت تاریخ ورودی اشتباه است");
                }
            }
            return miladiDate;
        }
    }
}

