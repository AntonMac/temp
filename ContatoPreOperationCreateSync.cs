using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;


namespace plugin_treino
{
    public class ContatoPreOperationCreateSync : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var crmService = serviceFactory.CreateOrganizationService(context.UserId);
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("Início Plugin");

            if (context.MessageName.ToLower() == "create"
                && context.Mode == Convert.ToInt32(MeuEnum.Mode.Synchronous)
                && context.Stage == Convert.ToInt32(MeuEnum.Stage.PreOperation))

            {
                Entity entidadeContexto = null;

                if (context.InputParameters.Contains("Target"))
                    entidadeContexto = (Entity)context.InputParameters["Target"];

                if (entidadeContexto != null)
                {

                    trace.Trace("Contexto diferente de nulo. ");

                    if (entidadeContexto.Attributes.ContainsKey("slo_cpf"))
                    {

                        var cpfContexto = entidadeContexto.Attributes["slo_cpf"].ToString();
                        trace.Trace($"CPF do contexto: {cpfContexto}");

                        QueryExpression queryExpression = new QueryExpression("contact");

                        queryExpression.TopCount = 1;

                        queryExpression.ColumnSet.AddColumns("slo_cpf", "lastname", "firstname");

                        queryExpression.Criteria.AddCondition("slo_cpf", ConditionOperator.Equal, cpfContexto);

                        var colecaoEntidades = crmService.RetrieveMultiple(queryExpression);

                        if (colecaoEntidades.Entities.Count > 0)
                            throw new InvalidPluginExecutionException("CPF já cadastrado!");


                    }
                }
            }
            trace.Trace("Fim Plugin");
        }

    }
}
