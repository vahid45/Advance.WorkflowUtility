using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace Advance.WorkflowUtility
{

    public sealed class WFAddManytoMany : CodeActivity
    {

        [RequiredArgument]
        [Input("Record Url 1")]
        public InArgument<string> Url1 { get; set; }


        [RequiredArgument]
        [Input("Record Url 2")]
        public InArgument<string> Url2 { get; set; }


        [RequiredArgument]
        [Input("Relationship Name")]
        public InArgument<string> Relation { get; set; }

        [Output("Message")]
        public OutArgument<string> Message { get; set; }

        [Output("Is Error")]
        public OutArgument<bool> IsError { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            string message = "";
            try
            {
                context.SetValue(this.IsError, false);

                #region "Load CRM Service from context"
                Common objCommon = new Common(context);
            objCommon.tracingService.Trace("Load CRM Service from context --- OK");
            IOrganizationService orgService = objCommon.orgService;
            #endregion
            string recordUrl1 = Url1.Get<string>(context);
            string recordUrl2 = Url2.Get<string>(context);
            string relationShip = Relation.Get<string>(context);

            var url1 = new DynamicUrlParser(recordUrl1);
            var url2 = new DynamicUrlParser(recordUrl2);
            var record1LogicalName = url1.GetEntityLogicalName(orgService);
            var record2LogicalName = url2.GetEntityLogicalName(orgService);



            Helper.CRMHelper helper = new Helper.CRMHelper(orgService, false);
             message = helper.AddManyToMany(url1.Id, record1LogicalName, url2.Id, record2LogicalName, relationShip);
            }
            catch (Exception ex)
            {

                message = ex.Message;
            }
            if (!string.IsNullOrEmpty(message))
            {
                context.SetValue(this.Message, message);
                context.SetValue(this.IsError, true);
            }
        }
    }
}
