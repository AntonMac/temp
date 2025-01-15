using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace plugin_treino
{
    public class TurmaPreOperationUpdateSync : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var crmService = serviceFactory.CreateOrganizationService(context.UserId);
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("Início do Plugin");

            if (context.MessageName.ToLower() == "update"
                && context.Mode == Convert.ToInt32(MeuEnum.Mode.Synchronous)
                && context.Stage == Convert.ToInt32(MeuEnum.Stage.PreOperation))
            {
                Entity entidadeContexto = null;

                if (context.InputParameters.Contains("Target"))
                    entidadeContexto = (Entity)context.InputParameters["Target"];

                if (entidadeContexto != null && entidadeContexto.Attributes.Contains("slo_datadeconclusao"))
                {
                    trace.Trace("slo_datadeconclusao detectado na atualização.");

                    
                    var turmaId = entidadeContexto.Id;
                    trace.Trace($"ID da Turma no contexto: {turmaId}");

                    
                    QueryExpression queryExpression = new QueryExpression("slo_historicodocontato");
                    queryExpression.ColumnSet.AddColumns("slo_nota", "slo_turma");
                    queryExpression.Criteria.AddCondition("slo_turma", ConditionOperator.Equal, turmaId);
                    queryExpression.Criteria.AddCondition("slo_nota", ConditionOperator.Null); 

                    trace.Trace("Executando consulta na tabela slo_historicodocontato.");
                    var resultados = crmService.RetrieveMultiple(queryExpression);

                    if (resultados.Entities.Count > 0)
                    {
                        trace.Trace("Foram encontrados registros sem Nota.");
                        throw new InvalidPluginExecutionException("Não é possível Preencher a data de conclusão da Turma enquanto houver Contatos sem nota.");
                    }
                    
                }
            }
            trace.Trace("Fim do Plugin.");
        }
    }
}
