# FluentGraphQL

**FluentGraphQL** is a lightweight, fluent C# library for dynamically building GraphQL queries. It allows developers to construct queries using a clean, chainable syntax—perfect for strongly typed scenarios or custom query generation needs.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/FluentGraphQL.svg)](https://www.nuget.org/packages/FluentGraphQL)

## ✨ Features

- ✅ Fluent API to build queries and mutations
- ✅ Nested field selection with arguments and aliases
- ✅ **Type-safe LINQ-style filters** — `.Where(x => x.City == "Paris" && x.Age >= 18)` compiled to the GraphQL `where` argument
- ✅ Lightweight — a single dependency (System.Text.Json)
- ✅ **High performance** — no per-field expression trees ([see benchmarks](#-performance))

## 🤝 Comparison

There is already a great alternative available: [`graphql-query-builder-dotnet`](https://github.com/charlesdevandiere/graphql-query-builder-dotnet) by Charles Devandiere. This project is not meant to discredit or replace it.

**FluentGraphQL** simply explores a different architectural approach, with a focus on fluent chaining, dynamic nested field construction, and performance fine-tuning. It was born independently and out of curiosity and learning, not competition.

## ⚡ Performance

Benchmarked with [BenchmarkDotNet](https://benchmarkdotnet.org/) against [`graphql-query-builder-dotnet`](https://github.com/charlesdevandiere/graphql-query-builder-dotnet) on the same nested query (`accounts → contacts → tasks`). Run it yourself:

```bash
dotnet run -c Release --project src/FluentGraphQL.Benchmark
```

| Scenario | Library | Mean | Allocated |
|---|---|---:|---:|
| Field selection | **FluentGraphQL** | **~0.7 µs** | **1.8 KB** |
| Field selection | graphql-query-builder-dotnet | ~5.2 µs | 11.3 KB |
| + `where` filter | **FluentGraphQL** — `.Where(x => …)` | **~2.7 µs** | **5.8 KB** |
| + `where` filter | graphql-query-builder-dotnet — manual args | ~7.9 µs | 19.1 KB |

- **~7× faster, ~6× less memory** selecting fields — FluentGraphQL reads member names via `Func` + `[CallerArgumentExpression]` instead of allocating an `Expression<Func<>>` per field.
- **~3× faster, ~3× less memory** on a filtered query — and the filter stays **type-safe**: `.Where(x => x.City == "Paris" && x.Contacts.Any(c => c.FirstName == "Jo"))` rather than a hand-written `where` object.

<sub>Apple M1 Max, .NET 9. Absolute numbers vary by machine — the ratios are the point. `.Where` parses one expression tree per query (read, never `.Compile()`d); field selection uses none.</sub>

## 📦 Installation

You can install via NuGet (once published):

```bash
dotnet add package FluentGraphQL
```

## 🚀 Quick Start

```csharp
public class Account
{
    public Guid Id { get; set; }
    public string SocietyName { get; set; }
    public Adresse Adresse { get; set; }
    public IEnumerable<Contact> Contacts { get; set; }
}

public class Contact
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public IEnumerable<Task> Tasks { get; set; }
}

public class Task
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? DueDate { get; set; }
}
```

```csharp
using static FluentGraphQL.GraphQL;

var builder = new GraphQLQueryBuilder();

var cities = new[] { "Paris", "London" };

builder
    .AddVariable("firstName", "Paul")
    .AddQuery(new GraphQLQueryObject<Account>("accounts")
        .AddEveryFields()
        .AddCollectionField(
          account => account.Contacts,
          contact => contact
            .AddEveryFields()
            .AddCollectionField(
              c => c.Tasks,
              task => task.AddEveryFields()
            )
        )
        .Where(account =>
            cities.Contains(account.Adresse.City)
            && account.Contacts.Any(c => c.FirstName == Var<string>("firstName"))));

var result = yourapi.Query(builder.Request);
```

Resulting query:

```graphql
query ($firstName: String!) {
  accounts(
    where: {
      adresse: { city: { in: ["Paris", "London"] } }
      contacts: { some: { firstName: { eq: $firstName } } }
    }
  ) {
    id
    societyName
    contacts {
      id
      firstName
      lastName
      email
      phoneNumber
      tasks {
        id
        name
        description
        startDate
        dueDate
      }
    }
  }
}
```

## 🔑 Variables and literal values

Inside a `.Where(...)` filter, a value is treated one of two ways:

- **A C# literal or captured value is literal data.** It is JSON-escaped before being inlined, so quotes, backslashes and newlines from user input cannot break out of the query. `x.City == "Paris"` renders `city: { eq: "Paris" }`; `x.City == userInput` is always safely escaped.
- **`Var<T>("name")` is a reference to a declared variable.** It renders `$name` and must match a variable added with `AddVariable`.

```csharp
using static FluentGraphQL.GraphQL;

builder
    .AddVariable("city", "Paris")
    .AddQuery(new GraphQLQueryObject<Account>("accounts")
        .AddEveryFields()
        .Where(x => x.Adresse.City == Var<string>("city")));
```

`AddVariable(name, value)` infers the GraphQL type from the value's type. Use the explicit `AddVariable(name, GraphQLParameterType.X, value)` overload when you need full control over the declared type.

Need an operator the fluent form doesn't cover (pagination, a custom argument)? `WithArguments(new { ... })` is still available as an escape hatch and follows the same variable/literal rules.

## 🧪 Testing

Tests are written with xUnit and cover query generation scenarios. To run:

```bash
dotnet test
```

## 📄 License

MIT — see the [LICENSE](LICENSE) file for details.

## 🙌 Contribution

Feel free to open issues or submit pull requests to improve the library!

---

**FluentGraphQL** is maintained by [@Nayruuu](https://github.com/Nayruuu).
