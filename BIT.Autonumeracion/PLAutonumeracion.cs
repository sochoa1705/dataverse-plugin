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
                    Entity solicitud = (Entity)context.InputParameters["Target"];
                    tracingService.Trace("contexto obtenido");
                    EntityReference tipoSolicitud = solicitud.GetAttributeValue<EntityReference>("bit_tiposolicitud");
                    Entity itemTipoSolicitud = service.Retrieve("bit_tiposolicitud", tipoSolicitud.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("bit_codigo", "bit_ultimosecuencial") );
                    
                    string subTipoSolicitud = solicitud.Contains("bit_subtiposolicitud") ? solicitud.GetAttributeValue<string>("bit_subtiposolicitud") : string.Empty;
                    

                    if (!String.IsNullOrEmpty(subTipoSolicitud))
                    {
                       var queryOnSubTypeRequest = searchBySubTypeRequest(subTipoSolicitud);
                       EntityCollection subTypes = service.RetrieveMultiple(queryOnSubTypeRequest);
                          var subType = subTypes.Entities[0];
                          var newSequence = subType.GetAttributeValue<int>("bit_ultimosecuencial") + 1;
                          subType.Id = subTypes.Entities[0].Id;
                          subType["bit_ultimosecuencial"] = newSequence;
                          service.Update(subType);
                          Entity newRequest = new Entity("bit_solicitud");
                          newRequest["cr8a0_numerotramite"] = subType.GetAttributeValue<string>("bit_abreviatura") + newSequence;
                          newRequest.Id = solicitud.Id;
                          service.Update(newRequest);
                    }
                    else
                    {
                        Entity newRequest = new Entity("bit_tiposolicitud");
                        var newSequenceRequestType = itemTipoSolicitud.GetAttributeValue<int>("bit_ultimosecuencial") + 1;
                        newRequest["bit_ultimosecuencial"] = newSequenceRequestType;
                        newRequest.Id = itemTipoSolicitud.Id;
                        service.Update(newRequest);
                        Entity newRequestType = new Entity("bit_solicitud");
                        newRequestType["cr8a0_numerotramite"] = itemTipoSolicitud.GetAttributeValue<string>("bit_codigo") + newSequenceRequestType;
                        newRequestType.Id = solicitud.Id;
                        service.Update(newRequestType);
                    }
                    

                    tracingService.Trace("Termina");
                    
                }
                catch (Exception ex)
                {

                    throw new InvalidPluginExecutionException("Error plugin. " + ex.Message);
                }
            }

        }
        
        
        public QueryExpression searchBySubTypeRequest(string subTypeRequest)
        {
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
                                new ConditionExpression("bit_name", ConditionOperator.Equal, subTypeRequest)
                            },
                        }
                    }
                }
            };
            return query;
        }


    }
}
