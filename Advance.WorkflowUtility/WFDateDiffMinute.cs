﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFDateDiffMinute : CodeActivity
    {
        [RequiredArgument]
        [Input("Starting Date")]
        public InArgument<DateTime> StartingDate { get; set; }

        [RequiredArgument]
        [Input("Ending Date")]
        public InArgument<DateTime> EndingDate { get; set; }

        [OutputAttribute("Minutes Difference")]
        public OutArgument<int> MinutesDifference { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(executionContext);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            try
            {
                DateTime startingDate = StartingDate.Get(executionContext);
                DateTime endingDate = EndingDate.Get(executionContext);

                startingDate = new DateTime(startingDate.Year, startingDate.Month, startingDate.Day, startingDate.Hour,
                    startingDate.Minute, 0, startingDate.Kind);

                endingDate = new DateTime(endingDate.Year, endingDate.Month, endingDate.Day, endingDate.Hour,
                    endingDate.Minute, 0, endingDate.Kind);

                TimeSpan difference = startingDate - endingDate;

                int minutesDifference = Math.Abs(Convert.ToInt32(difference.TotalMinutes));

                MinutesDifference.Set(executionContext, minutesDifference);
            }
            catch (Exception ex)
            {
                objCommon.tracingService.Trace("Exception: {0}", ex.ToString());
            }
        }
    }
}
