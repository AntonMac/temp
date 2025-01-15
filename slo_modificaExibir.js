function personalizarCaixasSelecao(executionContext) {
    // ObtÃ©m o contexto do formulÃ¡rio
    var formContext = executionContext.getFormContext();

    // Aguarda o carregamento completo da pÃ¡gina para garantir que todos os elementos DOM estejam disponÃ­veis
    document.addEventListener("DOMContentLoaded", function() {
        // Seleciona todas as caixas de seleÃ§Ã£o na exibiÃ§Ã£o
        var checkboxes = document.querySelectorAll("input[type='checkbox']");

        // Itera sobre todas as caixas de seleÃ§Ã£o
        checkboxes.forEach(function(checkbox) {
            // Adiciona um evento de clique para personalizar a aparÃªncia
            checkbox.addEventListener("change", function() {
                // Verifica se a caixa de seleÃ§Ã£o estÃ¡ marcada
                if (checkbox.checked) {
                    // Altera o estilo da caixa de seleÃ§Ã£o para mostrar um tique verde
                    checkbox.style.backgroundColor = "green";
                    checkbox.style.color = "white";
                    checkbox.style.border = "none";
                } else {
                    // Redefine o estilo da caixa de seleÃ§Ã£o para o padrÃ£o
                    checkbox.style.backgroundColor = "";
                    checkbox.style.color = "";
                    checkbox.style.border = "";
                }
            });
        });
    });
}
