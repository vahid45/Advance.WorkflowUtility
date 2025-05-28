using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{
    public class WFExecuteSQLJob : CodeActivity
    {

        [RequiredArgument]
        [Input("SSIS SQL Connection String")]
        [Default("Data Source=.;Initial Catalog=DBName;User ID=User;password=pass")]
        public InArgument<string> SQLConnectionString { get; set; }

        [RequiredArgument]
        [Input("SQL Job Name")]
        public InArgument<string> JobName { get; set; }

        [Output("Message")]
        public OutArgument<string> Message { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            #region "Load CRM Service from context"
            Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string jobName = context.GetValue(this.JobName);
            string connectionString = context.GetValue(this.SQLConnectionString);
            if (!string.IsNullOrEmpty(jobName) && !string.IsNullOrEmpty(connectionString))
            {
                string message = InternalExecuter(jobName, connectionString);
                context.SetValue(this.Message, message);
            }
        }

        public string InternalExecuter(string jobName, string connectionString)
        {
            string msg = "";

            using (var conn = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand("msdb.dbo.sp_start_job", conn))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@job_name", jobName);
                    try
                    {
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        msg = e.Message;
                    }




                }
            }

            return msg;
        }
    }
}
