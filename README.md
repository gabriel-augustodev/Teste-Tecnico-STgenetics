# Good Hamburger API

## Como executar

1. Clone o repositório
2. Abra no Visual Studio 2022
3. Aperte F5
4. Acesse `https://localhost:7001/swagger`

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core (InMemory)
- xUnit + Moq + FluentAssertions

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | /api/Cardapio | Lista produtos |
| GET | /api/Pedidos | Lista pedidos |
| GET | /api/Pedidos/{id} | Busca pedido |
| POST | /api/Pedidos | Cria pedido |
| PUT | /api/Pedidos/{id} | Atualiza pedido |
| DELETE | /api/Pedidos/{id} | Remove pedido |

## Regras de desconto

- Sanduíche + batata + refrigerante → 20%
- Sanduíche + refrigerante → 15%
- Sanduíche + batata → 10%

## Testes

18 testes automatizados com 100% de aprovação.

## Decisões técnicas

- Separação em Controllers, Services, Models
- Banco em memória para desenvolvimento
- Testes de integração com banco real
