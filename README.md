# API de Gerenciamento de Pedidos

Uma API RESTful robusta para gerenciamento de pedidos de produtos, constru�da com .NET 8 e seguindo princ�pios de arquitetura limpa. Esta API fornece uma solu��o completa para criar e gerenciar pedidos e produtos em um contexto de e-commerce ou varejo.

## ?? Sum�rio

- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Funcionalidades](#funcionalidades)
- [Documenta��o da API](#documenta��o-da-api)
- [Endpoints da API](#endpoints-da-api)
- [Regras de Neg�cio](#regras-de-neg�cio)
- [Como Come�ar](#como-come�ar)
- [Banco de Dados](#banco-de-dados)
- [Testes](#testes)

## ??? Arquitetura

O projeto segue os princ�pios de Arquitetura Limpa (Clean Architecture), separado em quatro projetos principais:

- **OrderManagement.Domain**: Cont�m entidades de neg�cio, enumera��es e exce��es de dom�nio
- **OrderManagement.Application**: Cont�m l�gica de neg�cio, DTOs, interfaces e servi�os
- **OrderManagement.Infrastructure**: Cont�m contexto de banco de dados, implementa��o de reposit�rios e servi�os externos
- **OrderManagementApi**: Cont�m controladores da API, modelos de requisi��o, valida��o e middleware

Esta arquitetura proporciona:
- Clara separa��o de responsabilidades
- Invers�o de depend�ncia (m�dulos de alto n�vel n�o dependem de m�dulos de baixo n�vel)
- Componentes test�veis
- C�digo base manuten�vel e escal�vel

## ?? Tecnologias

- **.NET 8**: Plataforma .NET mais recente para constru��o de aplica��es de alto desempenho e multiplataforma
- **ASP.NET Core**: Framework web para constru��o de APIs RESTful
- **Entity Framework Core**: ORM para opera��es de banco de dados
- **AutoMapper**: Biblioteca para mapeamento objeto-a-objeto
- **FluentValidation**: Biblioteca para constru��o de regras de valida��o
- **Swagger/OpenAPI**: Documenta��o e teste de API
- **SQL Server**: Banco de dados relacional
- **xUnit**: Framework de teste para testes unit�rios
- **Moq**: Framework de mock para testes unit�rios

## ? Funcionalidades

- Criar, recuperar, atualizar e excluir produtos
- Iniciar novos pedidos
- Adicionar produtos aos pedidos
- Remover produtos dos pedidos
- Fechar pedidos
- Consultar pedidos com capacidades de filtragem
- Valida��o de dados usando FluentValidation
- Padr�o de exclus�o l�gica para integridade de dados
- Tratamento global de exce��es
- Suporte � pagina��o para endpoints de cole��o

## ?? Documenta��o da API

O projeto inclui uma documenta��o completa da API no formato OpenAPI (Swagger). Voc� pode visualizar e testar a API de duas maneiras:

1. **Swagger UI embarcado**: Quando a aplica��o est� em execu��o, acesse `/swagger` para visualizar a documenta��o interativa.

2. **Arquivo OpenAPI**: O arquivo `swagger.yaml` na raiz do projeto cont�m a especifica��o completa da API. Voc� pode visualiz�-lo em:
   - [Swagger Editor Online](https://editor.swagger.io/) - Cole o conte�do do arquivo
   - Qualquer ferramenta compat�vel com OpenAPI 3.0

O arquivo Swagger fornece documenta��o detalhada de:
- Todos os endpoints dispon�veis
- Par�metros de requisi��o
- Formatos de resposta
- C�digos de status
- Modelos de dados
- Exemplos de uso

![Exemplo da documenta��o Swagger](https://raw.githubusercontent.com/swagger-api/swagger-ui/master/docs/v4.3.0/assets/swagger-ui.png)

## ?? Endpoints da API

### Produtos

| M�todo | Endpoint | Descri��o |
|--------|----------|-------------|
| POST | `/api/Product` | Criar um novo produto |
| GET | `/api/Product` | Obter todos os produtos com pagina��o |
| GET | `/api/Product/{productId}` | Obter produto por ID |
| PUT | `/api/Product/{productId}` | Atualizar um produto |
| DELETE | `/api/Product/{productId}` | Excluir um produto |

### Pedidos

| M�todo | Endpoint | Descri��o |
|--------|----------|-------------|
| POST | `/api/Order/start` | Iniciar um novo pedido |
| POST | `/api/Order/{orderId}/products` | Adicionar um produto a um pedido |
| DELETE | `/api/Order/{orderId}/products/{productId}` | Remover um produto de um pedido |
| PATCH | `/api/Order/{orderId}/close` | Fechar um pedido |
| GET | `/api/Order` | Obter todos os pedidos com pagina��o e filtragem opcional por status |
| GET | `/api/Order/{orderId}` | Obter pedido por ID |

## ?? Regras de Neg�cio

- Produtos devem ter um nome e pre�o v�lidos
- Pedidos t�m dois status: Aberto e Fechado
- Produtos s� podem ser adicionados ou removidos de pedidos Abertos
- Um pedido n�o pode ser fechado se n�o tiver produtos
- Ao adicionar um produto que j� existe no pedido, a quantidade � aumentada
- Ao remover um produto de um pedido, ele � exclu�do logicamente (marcado com timestamp DeletionAt)
- Pedidos mant�m um hist�rico de todas as opera��es
- O valor total � calculado com base na soma de (pre�o do produto � quantidade) para todos os itens n�o exclu�dos

## ?? Como Come�ar

### Pr�-requisitos

- SDK .NET 8
- SQL Server

### Instala��o

1. Clone o reposit�riogit clone https://github.com/seuusuario/OrderManagementApi.git
2. Navegue at� o diret�rio do projetocd OrderManagementApi
3. Restaure as depend�nciasdotnet restore
4. Atualize a string de conex�o em `appsettings.json` se necess�rio

5. Aplique as migra��es do banco de dadosdotnet ef database update
6. Execute a aplica��odotnet run --project OrderManagementApi
7. Acesse a UI do Swagger em `https://localhost:5001/swagger` (a porta pode variar)

## ?? Banco de Dados

A aplica��o usa SQL Server com Entity Framework Core para persist�ncia de dados. O banco de dados inclui as seguintes tabelas principais:

- **Products**: Armazena informa��es do produto
- **Orders**: Armazena informa��es do pedido com status
- **OrderItems**: Armazena os produtos inclu�dos em cada pedido

A conex�o com o banco de dados � configurada em `appsettings.json`:
"ConnectionStrings": {
  "database": "workstation id=OrderManagementApi-Sandbox.mssql.somee.com;packet size=4096;user id=Amandaolv_SQLLogin_1;pwd=k7ycwp3wtn;data source=OrderManagementApi-Sandbox.mssql.somee.com;persist security info=False;initial catalog=OrderManagementApi-Sandbox;TrustServerCertificate=True"
}
## ?? Testes

O projeto inclui testes unit�rios para valida��o da l�gica de neg�cios. Os testes s�o escritos usando xUnit e Moq para mock de depend�ncias.

Para executar os testes:
dotnet test
Cen�rios de teste principais incluem:
- Cria��o de um pedido (deve estar com status Aberto)
- Adi��o de produtos aos pedidos
- Remo��o de produtos dos pedidos
- Tentativa de fechar um pedido sem produtos (deve lan�ar exce��o)
- Tentativa de modificar um pedido fechado (deve lan�ar exce��o)

## ?? Seguran�a

A API est� preparada para autentica��o e autoriza��o, com o middleware necess�rio configurado no pipeline. O c�digo comentado em `Program.cs` pode ser descomentado ao implementar uma solu��o completa de autentica��o.

## ?? Conclus�o

Esta API de Gerenciamento de Pedidos demonstra a implementa��o de arquitetura limpa, design orientado por dom�nio e pr�ticas modernas de desenvolvimento .NET. Ela fornece uma base s�lida para constru��o de sistemas de e-commerce e gerenciamento de pedidos com foco em manutenibilidade, testabilidade e escalabilidade.

Sinta-se � vontade para contribuir ou abrir issues se encontrar bugs ou tiver sugest�es para melhorias!