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

    public sealed class WFCalculateBusinessDate : CodeActivity
    {
        [RequiredArgument]
        [Input("Date Input")]
        public InArgument<DateTime> InputDate { get; set; }


        [Input("Count Of Business Hour(Negative Is Approve)")]
        public InArgument<decimal> Hours { get; set; }


        [Input("End Date Input")]
        public InArgument<DateTime> EndDate { get; set; }


        [Output("Real End Date")]
        public OutArgument<DateTime> RealEndDate { get; set; }

        [Output("Total Real Hours")]
        public OutArgument<decimal> TotalHours { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion

            DateTime inputDate = context.GetValue(this.InputDate);
            DateTime? endDate = context.GetValue(this.EndDate);
            decimal hours = context.GetValue(this.Hours);
            decimal totalHolidaysHours = 0;
            DateTime realEndDate = calcBD(orgService, inputDate, hours, out totalHolidaysHours);


            context.SetValue(this.RealEndDate, realEndDate);
            context.SetValue(this.TotalHours, totalHolidaysHours);


        }



        private DateTime calcBD(IOrganizationService orgService, DateTime inputDate, decimal hours, out decimal totalHolidaysHours)
        {
            DateTime? endDate = null;
            DateTime? toDate = null;
            if (endDate == null)
            {
                toDate = inputDate.AddHours(Convert.ToDouble(hours));
            }
            else
            {
                toDate = endDate;
            }
            decimal totalHours = 0;
            decimal oldHoursOfHolidays = 0;
            DateTime toDateTmp = toDate.Value;
            while (true)
            {
                totalHours = GetHoursOfHolidays(orgService, inputDate, toDateTmp);
                if (totalHours == oldHoursOfHolidays)
                {
                    break;
                }
                oldHoursOfHolidays = totalHours;
                toDateTmp = toDateTmp.AddHours(Convert.ToDouble(totalHours));
            }

            totalHolidaysHours = totalHours;
            return toDate.Value.AddHours(Convert.ToDouble(totalHours));



        }

        private int GetDiff(DateTime inputDate, DateTime? endDate, decimal totalHours)
        {
            try
            {
                DateTime startingDate = inputDate;
                DateTime endingDate = endDate.Value;
                startingDate = new DateTime(startingDate.Year, startingDate.Month, startingDate.Day, startingDate.Hour,
                        startingDate.Minute, 0, startingDate.Kind);

                endingDate = new DateTime(endingDate.Year, endingDate.Month, endingDate.Day, endingDate.Hour,
                    endingDate.Minute, 0, endingDate.Kind);

                TimeSpan difference = startingDate - endingDate;

                int minutesDifference = Math.Abs(Convert.ToInt32(difference.TotalMinutes));
                return minutesDifference;
            }
            catch (Exception)
            {

                return 0;
            }


        }
        private decimal GetHoursOfHolidays(IOrganizationService orgService, DateTime inputDate, DateTime toDate)
        {
            decimal hours = 0;
            EntityCollection holidays = GetAllHoliday(orgService);
            List<Entity> holiDays = holidays.Entities.Where(a => (DateTime)a["starttime"] >= inputDate && (DateTime)a["starttime"] <= toDate).ToList();
            if (holiDays != null && holiDays.Count > 0)
            {
                foreach (Entity item in holiDays)
                {
                    if (item.Attributes.Contains("duration"))
                    {
                        int minute = (int)item["duration"];
                        hours = hours + (minute / 60);
                    }
                }
                return hours;
            }
            return 0;
        }

        private EntityCollection GetAllHoliday(IOrganizationService orgService)
        {
            QueryExpression query = new QueryExpression("calendar");
            query.ColumnSet = new ColumnSet(true);
            ConditionExpression condition = new ConditionExpression();
            condition.AttributeName = "name";
            condition.Operator = ConditionOperator.Equal;
            condition.Values.Add("Business Closure Calendar");
            query.Criteria.Conditions.Add(condition);
            EntityCollection calendars = orgService.RetrieveMultiple(query);
            EntityCollection calendarrule = calendars[0].GetAttributeValue<EntityCollection>("calendarrules");
            return calendarrule;
        }

        private decimal GetHoursOfDefaultHolidays(IOrganizationService orgService, string configEntity, string keyFieldName, string key, string valueFieldName, DateTime inputDate, DateTime toDate, out List<DayOfWeek> weekends)
        {
            decimal defaultHolidaysMustBeAdd = 0;
            if (configEntity != null && keyFieldName != null && key != null && valueFieldName != null && configEntity != "" && keyFieldName != "" && key != "" && valueFieldName != "")
            {
                List<DayOfWeek> DefaultHolidays = GetDefaultHolidays(orgService, configEntity, keyFieldName, key, valueFieldName);
                if (DefaultHolidays != null && DefaultHolidays.Count > 0)
                {


                    foreach (DayOfWeek item in DefaultHolidays)
                    {
                        defaultHolidaysMustBeAdd = defaultHolidaysMustBeAdd + CountDays(item, inputDate, toDate) * 24;
                        toDate = toDate.AddHours(Convert.ToDouble(defaultHolidaysMustBeAdd));

                    }
                    weekends = DefaultHolidays;
                    return defaultHolidaysMustBeAdd;
                }


            }
            else
            {
                defaultHolidaysMustBeAdd = CountDays(DayOfWeek.Friday, inputDate, toDate) * 24;
                List<DayOfWeek> weekendslist = new List<DayOfWeek> { DayOfWeek.Friday };
                weekends = weekendslist;
            }
            weekends = null;
            return defaultHolidaysMustBeAdd;
        }

        private List<DayOfWeek> GetDefaultHolidays(IOrganizationService orgService, string configEntity, string keyFieldName, string key, string valueFieldName)
        {
            QueryExpression qry = new QueryExpression(configEntity);
            qry.ColumnSet = new ColumnSet(true);
            qry.Criteria.AddCondition(keyFieldName, ConditionOperator.Equal, key);
            try
            {
                EntityCollection configs = orgService.RetrieveMultiple(qry);
                if (configs != null && configs.Entities != null && configs.Entities.Count > 0)
                {
                    try
                    {
                        string value =
                        (string)configs.Entities.First()[valueFieldName];
                        string[] spilitedValue = value.Split(',');
                        List<DayOfWeek> listOfDays = new List<DayOfWeek>();
                        foreach (string item in spilitedValue)
                        {

                            DayOfWeek? day = GetDayByNumber(item);
                            if (day != null)
                            {
                                listOfDays.Add(day.Value);

                            }


                        }
                        return listOfDays;
                    }
                    catch (Exception ex)
                    {

                        throw new InvalidPluginExecutionException(ex.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }


            return null;
        }

        private DayOfWeek? GetDayByNumber(string dayNumber)
        {
            DayOfWeek? day = null;
            switch (dayNumber)
            {
                case "1":
                    day = DayOfWeek.Saturday;
                    break;
                case "2":
                    day = DayOfWeek.Sunday;
                    break;
                case "3":
                    day = DayOfWeek.Monday;
                    break;
                case "4":
                    day = DayOfWeek.Tuesday;
                    break;
                case "5":
                    day = DayOfWeek.Wednesday;
                    break;
                case "6":
                    day = DayOfWeek.Thursday;
                    break;
                case "7":
                    day = DayOfWeek.Friday;
                    break;

            }
            return day;
        }

        static int CountDays(DayOfWeek day, DateTime start, DateTime end)
        {
            TimeSpan ts = end - start;                       // Total duration
            int count = (int)Math.Floor(ts.TotalDays / 7);   // Number of whole weeks
            int remainder = (int)(ts.TotalDays % 7);         // Number of remaining days
            int sinceLastDay = (int)(end.DayOfWeek - day);   // Number of days since last [day]
            if (sinceLastDay < 0) sinceLastDay += 7;         // Adjust for negative days since last [day]

            // If the days in excess of an even week are greater than or equal to the number days since the last [day], then count this one, too.
            if (remainder >= sinceLastDay) count++;
            if (count < 0)
            {
                count = count * -1;
            }
            return count;
        }
    }
}
