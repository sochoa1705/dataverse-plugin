using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIT.Autonumeracion
{
    public class PLAutonumeracion : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            IOrganizationService iServices = ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(new Guid?(context.UserId));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("Ingresa");
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {

                    tracingService.Trace("Ingresa");
                    /////Programar la logica
                    ///
                    Entity solicitud = (Entity)context.InputParameters["Target"];

                    EntityReference tiposolicitud = solicitud.GetAttributeValue<EntityReference>("bit_tiposolicitud");
                    Entity itemtiposolicitud = service.Retrieve("bit_tiposolicitud", tiposolicitud.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bit_codigo", "bit_ultimosecuencial") );
                    
                    string subtiposolicitud = solicitud.GetAttributeValue<string>("bit_subtiposolicitud");

                    QueryExpression query = new QueryExpression()
                    {
                        Distinct = false,
                        EntityName = "bit_subtipos",
                        ColumnSet = new ColumnSet(new string[] { "bit_abreviatura", "bit_ultimosecuencial" }),
                        Criteria =
                        {
                            Filters =
                            {
                                new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("bit_name", ConditionOperator.Equal, subtiposolicitud)
                                    },
                                }
                            }
                        }
                    };
                    EntityCollection subtiposencontrados = service.RetrieveMultiple(query);
                    var sub = subtiposencontrados.Entities[0];



                    //Entity xxx = new Entity("bit_tiposolicitud");
                    //xxx["bit_ultimo_secuencial"] = 21222;
                    //xxx.Id = itemtiposolicitud.Id;
                    //service.Update(xxx);


                    tracingService.Trace("Termina");


                }
                catch (Exception ex)
                {

                    throw new InvalidPluginExecutionException("Error plugin. " + ex.Message);
                }
            }

        }
    }
}
