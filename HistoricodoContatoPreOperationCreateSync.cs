using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace plugin_treino
{
    public class HistoricodoContatoPreOperationCreateSync : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var crmService = serviceFactory.CreateOrganizationService(context.UserId);
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("Início do Plugin");

            if (context.MessageName.ToLower() == "create"
                && context.Mode == Convert.ToInt32(MeuEnum.Mode.Synchronous)
                && context.Stage == Convert.ToInt32(MeuEnum.Stage.PreOperation))
            {
                Entity entidadeContexto = null;

                if (context.InputParameters.Contains("Target"))
                    entidadeContexto = (Entity)context.InputParameters["Target"];

                if (entidadeContexto != null)
                {
                    trace.Trace("Entidade de contexto identificada.");

                    if (!entidadeContexto.Attributes.Contains("slo_contact") || !entidadeContexto.Attributes.Contains("slo_turma"))
                    {
                        throw new InvalidPluginExecutionException("Os campos Contato e Turma são obrigatórios.");
                    }

                    var contatoId = ((EntityReference)entidadeContexto.Attributes["slo_contact"]).Id;
                    var turmaId = ((EntityReference)entidadeContexto.Attributes["slo_turma"]).Id;

                    trace.Trace($"Contato ID: {contatoId}, Turma ID: {turmaId}");

                    
                    QueryExpression queryAssociacao = new QueryExpression("slo_turma_contact")
                    {
                        ColumnSet = new ColumnSet("contactid", "slo_turmaid")
                    };
                    queryAssociacao.Criteria.AddCondition("contactid", ConditionOperator.Equal, contatoId);
                    queryAssociacao.Criteria.AddCondition("slo_turmaid", ConditionOperator.Equal, turmaId);

                    trace.Trace("Executando consulta na tabela de relacionamento slo_turma_contact.");
                    var associacoes = crmService.RetrieveMultiple(queryAssociacao);

                    if (associacoes.Entities.Count == 0)
                    {
                        throw new InvalidPluginExecutionException("O contato não está associado à turma indicada.");
                    }

                    
                    QueryExpression queryHistorico = new QueryExpression("slo_historicodocontato")
                    {
                        ColumnSet = new ColumnSet("slo_contact", "slo_turma")
                    };
                    queryHistorico.Criteria.AddCondition("slo_contact", ConditionOperator.Equal, contatoId);
                    queryHistorico.Criteria.AddCondition("slo_turma", ConditionOperator.Equal, turmaId);

                    trace.Trace("Executando consulta na tabela slo_historicodocontato.");
                    var historicoExistente = crmService.RetrieveMultiple(queryHistorico);

                    if (historicoExistente.Entities.Count > 0)
                    {
                        throw new InvalidPluginExecutionException("O contato já possui um histórico associado a esta turma.");
                    }

                    
                    Entity contato = crmService.Retrieve("contact", contatoId, new ColumnSet("cr5f8_einstrutor"));
                    if (contato != null && contato.Contains("cr5f8_einstrutor"))
                    {
                        var ehInstrutor = contato.GetAttributeValue<bool>("cr5f8_einstrutor");
                        trace.Trace($"Contato é instrutor: {ehInstrutor}");

                        if (ehInstrutor)
                        {
                            throw new InvalidPluginExecutionException("Não é permitido criar um histórico de contato para um instrutor.");
                        }
                    }

                    trace.Trace("Validações concluídas. Criação permitida.");
                }
            }
            trace.Trace("Fim do Plugin.");
        }
    }
}
