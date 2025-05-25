# API de Gerenciamento de Pedidos

Uma API RESTful robusta para gerenciamento de pedidos de produtos, construída com .NET 8 e seguindo princípios de arquitetura limpa. Esta API fornece uma solução completa para criar e gerenciar pedidos e produtos em um contexto de e-commerce ou varejo.

## 📋 Sumário

- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Funcionalidades](#funcionalidades)
- [Documentação da API](#documentação-da-api)
- [Endpoints da API](#endpoints-da-api)
- [Regras de Negócio](#regras-de-negócio)
- [Como Começar](#como-começar)
- [Banco de Dados](#banco-de-dados)
- [Testes](#testes)

## 🏗️ Arquitetura

O projeto segue os princípios de Arquitetura Limpa (Clean Architecture), separado em quatro projetos principais:

- **OrderManagement.Domain**: Contém entidades de negócio, enumerações e exceções de domínio
- **OrderManagement.Application**: Contém lógica de negócio, DTOs, interfaces e serviços
- **OrderManagement.Infrastructure**: Contém contexto de banco de dados, implementação de repositórios e serviços externos
- **OrderManagementApi**: Contém controladores da API, modelos de requisição, validação e middleware

Esta arquitetura proporciona:
- Clara separação de responsabilidades
- Inversão de dependência (módulos de alto nível não dependem de módulos de baixo nível)
- Componentes testáveis
- Código base manutenível e escalável

## 🚀 Tecnologias

- **.NET 8**: Plataforma .NET mais recente para construção de aplicações de alto desempenho e multiplataforma
- **ASP.NET Core**: Framework web para construção de APIs RESTful
- **Entity Framework Core**: ORM para operações de banco de dados
- **AutoMapper**: Biblioteca para mapeamento objeto-a-objeto
- **FluentValidation**: Biblioteca para construção de regras de validação
- **Swagger/OpenAPI**: Documentação e teste de API
- **SQL Server**: Banco de dados relacional
- **xUnit**: Framework de teste para testes unitários
- **Moq**: Framework de mock para testes unitários

## ✨ Funcionalidades

- Criar, recuperar, atualizar e excluir produtos
- Iniciar novos pedidos
- Adicionar produtos aos pedidos
- Remover produtos dos pedidos
- Fechar pedidos
- Consultar pedidos com capacidades de filtragem
- Validação de dados usando FluentValidation
- Padrão de exclusão lógica para integridade de dados
- Tratamento global de exceções
- Suporte à paginação para endpoints de coleção

## 📚 Documentação da API

O projeto inclui uma documentação completa da API no formato OpenAPI (Swagger). Você pode visualizar e testar a API de duas maneiras:

1. **Swagger UI embarcado**: Quando a aplicação está em execução, acesse `/swagger` para visualizar a documentação interativa.

2. **Arquivo OpenAPI**: O arquivo `swagger.yaml` na raiz do projeto contém a especificação completa da API. Você pode visualizá-lo em:
   - [Swagger Editor Online](https://editor.swagger.io/) - Cole o conteúdo do arquivo
   - Qualquer ferramenta compatível com OpenAPI 3.0

O arquivo Swagger fornece documentação detalhada de:
- Todos os endpoints disponíveis
- Parâmetros de requisição
- Formatos de resposta
- Códigos de status
- Modelos de dados
- Exemplos de uso


## 🔌 Endpoints da API

### Produtos

| Método | Endpoint | Descrição |
|--------|----------|-------------|
| POST | `/api/Product` | Criar um novo produto |
| GET | `/api/Product` | Obter todos os produtos com paginação |
| GET | `/api/Product/{productId}` | Obter produto por ID |
| PUT | `/api/Product/{productId}` | Atualizar um produto |
| DELETE | `/api/Product/{productId}` | Excluir um produto |

### Pedidos

| Método | Endpoint | Descrição |
|--------|----------|-------------|
| POST | `/api/Order/start` | Iniciar um novo pedido |
| POST | `/api/Order/{orderId}/products` | Adicionar um produto a um pedido |
| DELETE | `/api/Order/{orderId}/products/{productId}` | Remover um produto de um pedido |
| PATCH | `/api/Order/{orderId}/close` | Fechar um pedido |
| GET | `/api/Order` | Obter todos os pedidos com paginação e filtragem opcional por status |
| GET | `/api/Order/{orderId}` | Obter pedido por ID |

## 📏 Regras de Negócio

- Produtos devem ter um nome e preço válidos
- Pedidos têm dois status: Aberto e Fechado
- Produtos só podem ser adicionados ou removidos de pedidos Abertos
- Um pedido não pode ser fechado se não tiver produtos
- Ao adicionar um produto que já existe no pedido, a quantidade é aumentada
- Ao remover um produto de um pedido, ele é excluído logicamente (marcado com timestamp DeletionAt)
- Pedidos mantêm um histórico de todas as operações
- O valor total é calculado com base na soma de (preço do produto × quantidade) para todos os itens não excluídos

## 🏁 Como Começar

### Pré-requisitos

- SDK .NET 8
- SQL Server

### Instalação

1. Clone o repositóriogit clone https://github.com/seuusuario/OrderManagementApi.git
2. Navegue até o diretório do projetocd OrderManagementApi
3. Restaure as dependênciasdotnet restore
4. Atualize a string de conexão em `appsettings.json` se necessário

5. Aplique as migrações do banco de dadosdotnet ef database update
6. Execute a aplicaçãodotnet run --project OrderManagementApi
7. Acesse a UI do Swagger em `https://localhost:5001/swagger` (a porta pode variar)

## 💾 Banco de Dados

A aplicação usa SQL Server com Entity Framework Core para persistência de dados. O banco de dados inclui as seguintes tabelas principais:

- **Products**: Armazena informações do produto
- **Orders**: Armazena informações do pedido com status
- **OrderItems**: Armazena os produtos incluídos em cada pedido

A conexão com o banco de dados é configurada em `appsettings.json`:
"ConnectionStrings": {
  "database": "workstation id=OrderManagementApi-Sandbox.mssql.somee.com;packet size=4096;user id=xxxxx;pwd=xxxxx;data source=OrderManagementApi-Sandbox.mssql.somee.com;persist security info=False;initial catalog=OrderManagementApi-Sandbox;TrustServerCertificate=True"
}
## 🧪 Testes

O projeto inclui testes unitários para validação da lógica de negócios. Os testes são escritos usando xUnit e Moq para mock de dependências.

Para executar os testes:
dotnet test
Cenários de teste principais incluem:
- Criação de um pedido (deve estar com status Aberto)
- Adição de produtos aos pedidos
- Remoção de produtos dos pedidos
- Tentativa de fechar um pedido sem produtos (deve lançar exceção)
- Tentativa de modificar um pedido fechado (deve lançar exceção)

## 🔒 Segurança

A API está preparada para autenticação e autorização, com o middleware necessário configurado no pipeline. O código comentado em `Program.cs` pode ser descomentado ao implementar uma solução completa de autenticação.

## 📝 Conclusão

Esta API de Gerenciamento de Pedidos demonstra a implementação de arquitetura limpa, design orientado por domínio e práticas modernas de desenvolvimento .NET. Ela fornece uma base sólida para construção de sistemas de e-commerce e gerenciamento de pedidos com foco em manutenibilidade, testabilidade e escalabilidade.

Sinta-se à vontade para contribuir ou abrir issues se encontrar bugs ou tiver sugestões para melhorias!
