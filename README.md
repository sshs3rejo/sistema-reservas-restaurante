# SistemaReservas — Reservas de Mesas para Restaurantes

Sistema de console em **C# (.NET 8)** que implementa a modelagem de um sistema de reservas de mesas para restaurantes, desenvolvido como TDE da disciplina de Análise e Projeto Orientado a Objetos. As classes espelham o diagrama de classes do trabalho e os dados são persistidos em **SQLite**.

## Funcionalidades

- Cadastro de clientes, funcionários, gerentes e mesas
- Consulta de disponibilidade por data, horário e número de pessoas
- Realizar, confirmar, cancelar, concluir e reagendar reservas
- Agenda do dia e relatório gerencial do dia
- Persistência em banco de dados SQLite (`reservas.db`, criado automaticamente)
- Carregamento de dados de exemplo quando o banco está vazio

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Como executar

```bash
dotnet build
dotnet run
```

Na primeira execução, o programa pergunta se deseja carregar dados de exemplo (resposta padrão: `s`). O arquivo `reservas.db` é criado na pasta do projeto e o caminho é exibido na tela.

## Estrutura

```
SistemaReservas/
├── Program.cs                # Menu interativo
├── Data/BancoDeDados.cs      # Persistência em SQLite (CRUD)
├── Models/                   # Classes do diagrama de classes
│   ├── Pessoa.cs             # Classe abstrata (herança)
│   ├── Cliente.cs
│   ├── Funcionario.cs
│   ├── Gerente.cs
│   ├── Mesa.cs
│   ├── Reserva.cs
│   ├── Restaurante.cs
│   ├── Enums.cs
└── diagramas/                # Casos de uso e diagrama de classes (SVG/PNG)
```

## Diagramas

Os diagramas de casos de uso e de classes utilizados no relatório estão disponíveis na pasta `diagramas/` (formatos SVG e PNG).