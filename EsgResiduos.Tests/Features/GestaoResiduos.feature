# language: pt
Funcionalidade: Gestão sustentável de resíduos pela API
  Como operador ambiental
  Quero validar os serviços de coleta e destinação
  Para assegurar rastreabilidade, conformidade e alertas de capacidade

  Cenário: Cadastro e autenticação de usuário com contrato válido
    Quando cadastro um novo usuário
    Então o cadastro deve retornar 201 e um token conforme o contrato
    Quando autentico o usuário cadastrado
    Então o login deve retornar 200 e um token conforme o contrato

  Cenário: Acesso anônimo às APIs protegidas é recusado
    Quando consulto todas as APIs protegidas sem token
    Então todas as consultas devem retornar 401

  Cenário: Ciclo completo de coleta e destinação gera rastreabilidade ESG
    Dado que sou um usuário autenticado
    Quando executo o ciclo completo de tipo de resíduo, ponto, coleta e destinação
    Então todas as APIs devem responder com status e contratos JSON válidos
    E deve existir um alerta automático de limite atingido
    E deve existir uma notificação de destinação correta
    E os recursos devem ser excluídos na ordem segura

  Esquema do Cenário: Paginação inválida é rejeitada nas listagens
    Dado que sou um usuário autenticado
    Quando consulto "<recurso>" com página zero
    Então a resposta deve ter status 400

    Exemplos:
      | recurso          |
      | wastetypes       |
      | collectionpoints |
      | collections      |
      | destinations     |
      | collectionalerts |

  Cenário: Credenciais inválidas são rejeitadas
    Quando tento autenticar com credenciais inexistentes
    Então a resposta deve ter status 401 e contrato de erro válido
