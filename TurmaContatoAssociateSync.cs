using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace plugin_treino
{
    public class TurmaContatoAssociateSync : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var crmService = serviceFactory.CreateOrganizationService(context.UserId);
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("Início do Plugin");

            if (context.MessageName.ToLower() == "associate"
                && context.Mode == Convert.ToInt32(MeuEnum.Mode.Synchronous)
                && context.Stage == Convert.ToInt32(MeuEnum.Stage.PostOperation))
            {
                trace.Trace("Verificando contexto de entrada");

               
                if (context.InputParameters.Contains("Relationship") && context.InputParameters.Contains("RelatedEntities"))
                {
                    var relationship = (Relationship)context.InputParameters["Relationship"];
                    var relatedEntities = (EntityReferenceCollection)context.InputParameters["RelatedEntities"];
                    var targetEntity = (EntityReference)context.InputParameters["Target"];

                    if (relationship.SchemaName == "slo_turma_contact")
                    {
                        trace.Trace("Relacionamento 'slo_turma_contact' identificado.");

                        var turmaId = targetEntity.Id;

                        
                        foreach (var entityReference in relatedEntities)
                        {
                            var contatoId = entityReference.Id;

                            
                            QueryExpression queryHistorico = new QueryExpression("slo_historicodocontato")
                            {
                                ColumnSet = new ColumnSet("slo_contact", "slo_turma")
                            };
                            queryHistorico.Criteria.AddCondition("slo_contact", ConditionOperator.Equal, contatoId);
                            queryHistorico.Criteria.AddCondition("slo_turma", ConditionOperator.Equal, turmaId);

                            var historicoExistente = crmService.RetrieveMultiple(queryHistorico);

                            if (historicoExistente.Entities.Count > 0)
                            {
                                trace.Trace("Já existe um histórico para este contato e esta turma. Nenhuma ação necessária.");
                                continue;
                            }

                            
                            Entity novoHistorico = new Entity("slo_historicodocontato");
                            novoHistorico["slo_contact"] = new EntityReference("contact", contatoId);
                            novoHistorico["slo_turma"] = new EntityReference("slo_turma", turmaId);
                            crmService.Create(novoHistorico);
                            trace.Trace("Histórico de contato criado com sucesso.");
                        }
                    }
                    else
                    {
                        trace.Trace("Relacionamento não reconhecido: " + relationship.SchemaName);
                    }
                }
                else
                {
                    trace.Trace("Parâmetros de entrada não contêm 'Relationship' ou 'RelatedEntities'.");
                }
            }
            trace.Trace("Fim do Plugin.");
        }
    }
}
