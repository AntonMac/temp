async function chamarFluxoPowerAutomate(executionContext) {
    const formContext =
      typeof executionContext.getFormContext === "function"
        ? executionContext.getFormContext()
        : executionContext;
    let turma = formContext.getAttribute("slo_codigodaturma")?.getValue();
  
    const entityId = formContext.data.entity.getId();
    if (!entityId) {
      alert("Erro: Não foi possível obter o ID da entidade.");
      return;
    }
    Xrm.Utility.showProgressIndicator("Enviando emails...");
    const entityIdFormatado = entityId.replace(/[{}]/g, "");
  
    let contacts;
    try {
      const fetchXml = `
          <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">
              <entity name="contact">
                  <attribute name="contactid"/>
                  <attribute name="lastname"/>
                  <attribute name="firstname"/>
                  <attribute name="emailaddress1"/>
                  <order attribute="lastname" descending="false"/>
                  <link-entity name="slo_turma_contact" from="contactid" to="contactid" visible="false" intersect="true">
                    <link-entity name="slo_turma" from="slo_turmaid" to="slo_turmaid" alias="ad">
                        <filter type="and">
                        <condition attribute="slo_turmaid" operator="eq" uiname="5" uitype="slo_turma" value="${entityIdFormatado}"/>
                        </filter>
                      </link-entity>
                  </link-entity>
              </entity>
          </fetch>
      `;
  
      contacts = await Xrm.WebApi.online.retrieveMultipleRecords(
        "contact",
        `?fetchXml=${encodeURIComponent(fetchXml)}`
      );
    } catch (error) {
      alert(`Erro ao buscar contatos: ${error.message}`);
      return;
    }
  
    const data = {
      turma: turma,
      turmaId: entityIdFormatado,
      contatos: contacts.entities
        .filter(
          (contact) =>
            contact.firstname && contact.lastname && contact.emailaddress1
        ) 
        .map((contact) => ({
          nome: contact.firstname,
          sobrenome: contact.lastname,
          email: contact.emailaddress1,
        })),
    };
  
    if (data.length === 0) {
      alert("Nenhum contato relacionado encontrado.");
      return;
    }
  
    try {
      const response = await fetch(
        "https://prod-15.brazilsouth.logic.azure.com:443/workflows/95678a9b1dd140a68ed6699450cee513/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=V7wU2T9b5U-9dVt2f9LtH4PLqHFCF3SzdcK3d6Efl9g",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(data),
        }
      );
  
      if (!response.ok) {
        throw new Error(`Erro na solicitação: ${response.statusText}`);
      }
  
      const responseData = await response.json();
      if (responseData.success === true) {
        alert(responseData.message);
      } else {
        alert(`Erro no Power Automate: ${responseData.message}`);
      }
    } catch (error) {
      alert(`Erro ao processar a solicitação: ${error.message}`);
    } finally {
      Xrm.Utility.closeProgressIndicator();
    }
  }
  //------------------------------------------------------------
  async function chamarFluxoPowerAutomate2(executionContext) {
    const formContext =
      typeof executionContext.getFormContext === "function"
        ? executionContext.getFormContext()
        : executionContext;
    const turma = formContext.getAttribute("slo_codigodaturma")?.getValue();

    const entityId = formContext.data.entity.getId();
    if (!entityId) {
      alert("Erro: Não foi possível obter o ID da entidade.");
      return;
    }

    Xrm.Utility.showProgressIndicator("Enviando emails...");
    const entityIdFormatado = entityId.replace(/[{}]/g, "");

    let dataDeConclusao;
    try {
      const turmaRecord = await Xrm.WebApi.online.retrieveRecord("slo_turma", entityIdFormatado, "?$select=slo_datadeconclusao");
      dataDeConclusao = turmaRecord.slo_datadeconclusao;
    } catch (error) {
      alert(`Erro ao buscar a data de conclusão: ${error.message}`);
      Xrm.Utility.closeProgressIndicator();
      return;
    }

    if (!dataDeConclusao) {
      alert("Erro: A turma deve possuir uma data de conclusão salva na tabela para enviar os e-mails.");
      Xrm.Utility.closeProgressIndicator();
      return;
    }

    let contacts;
    try {
      const fetchXml = `
        <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">
          <entity name="contact">
            <attribute name="contactid"/>
            <attribute name="firstname"/>
            <attribute name="lastname"/>
            <attribute name="emailaddress1"/>
            <order attribute="lastname" descending="false"/>
            <link-entity name="slo_turma_contact" from="contactid" to="contactid" visible="false" intersect="true">
              <link-entity name="slo_turma" from="slo_turmaid" to="slo_turmaid" alias="turma">
                <filter type="and">
                  <condition attribute="slo_turmaid" operator="eq" value="${entityIdFormatado}"/>
                </filter>
              </link-entity>
              <link-entity name="slo_historicodocontato" from="slo_contact" to="contactid" alias="historico">
                <filter type="and">
                  <condition attribute="slo_situacao" operator="eq" value="921980000"/>
                </filter>
              </link-entity>
            </link-entity>
          </entity>
        </fetch>
      `;

      contacts = await Xrm.WebApi.online.retrieveMultipleRecords(
        "contact",
        `?fetchXml=${encodeURIComponent(fetchXml)}`
      );
    } catch (error) {
      alert(`Erro ao buscar contatos: ${error.message}`);
      Xrm.Utility.closeProgressIndicator();
      return;
    }

    const data = {
      turma: turma,
      turmaId: entityIdFormatado,
      contatos: contacts.entities
        .filter(
          (contact) =>
            contact.firstname && contact.lastname && contact.emailaddress1
        )
        .map((contact) => ({
          nome: contact.firstname,
          sobrenome: contact.lastname,
          email: contact.emailaddress1,
        })),
    };

    if (data.contatos.length === 0) {
      alert("Nenhum contato aprovado relacionado encontrado.");
      Xrm.Utility.closeProgressIndicator();
      return;
    }

    try {
      const response = await fetch(
        "https://prod-29.brazilsouth.logic.azure.com:443/workflows/18c716c3cca24fdd80c151c2b0f36d3c/triggers/manual/paths/invoke?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=6q0fx4w1IYFkggXaf9--CmFGbCnqRHOSI43OxJoCpn8",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(data),
        }
      );

      if (!response.ok) {
        throw new Error(`Erro na solicitação: ${response.statusText}`);
      }

      const responseData = await response.json();
      if (responseData.success === true) {
        alert(responseData.message);
      } else {
        alert(`Erro no Power Automate: ${responseData.message}`);
      }
    } catch (error) {
      alert(`Erro ao processar a solicitação: ${error.message}`);
    } finally {
      Xrm.Utility.closeProgressIndicator();
    }
  }

  
  //---------------------------------------------------
  