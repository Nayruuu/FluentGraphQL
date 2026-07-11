# FluentGraphQL

**FluentGraphQL** is a lightweight, fluent C# library for dynamically building GraphQL queries. It allows developers to construct queries using a clean, chainable syntax—perfect for strongly typed scenarios or custom query generation needs.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![NuGet](https://img.shields.io/nuget/v/FluentGraphQL.svg)](https://www.nuget.org/packages/FluentGraphQL)

## ✨ Features

- ✅ Fluent API to build queries and mutations
- ✅ Nested field selection with arguments and aliases
- ✅ Easy integration in .NET applications
- ✅ Lightweight — a single dependency (System.Text.Json)
- ✅ Built-in performance benchmarks (BenchmarkDotNet) — run `dotnet run -c Release --project src/FluentGraphQL.Benchmark`

## 🤝 Comparison

There is already a great alternative available: [`graphql-query-builder-dotnet`](https://github.com/charlesdevandiere/graphql-query-builder-dotnet) by Charles Devandiere. This project is not meant to discredit or replace it.

**FluentGraphQL** simply explores a different architectural approach, with a focus on fluent chaining, dynamic nested field construction, and performance fine-tuning. It was born independently and out of curiosity and learning, not competition.

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

var name = "Paul";
var cities = new[] { "Paris", "London", "Madrid", "New York" };

builder
    .AddVariable("firstName", name)
    .AddVariable("cities", cities)
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
        .WithArguments(new
        {
            where = new
            {
                and = new object[]
                {
                    new
                    {
                        city = new
                        {
                            @in = Var("cities")
                        }
                    },
                    new
                    {
                        contacts = new
                        {
                            firstName = new
                            {
                                eq = Var("firstName")
                            }
                        }
                    }
                }
            }
        }));

var result = yourapi.Query(builder.Request);
```

Resulting query:

```graphql
query ($firstName: String!, $cities: [String]!) {
  accounts(
    where: {
      and: [
        { city: { in: $cities } }
        { contacts: { firstName: { eq: $firstName } } }
      ]
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

Inside `WithArguments(...)`, a value is treated one of two ways:

- **A `string` (or any other value) is literal data.** It is JSON-escaped before being inlined, so quotes, backslashes and newlines coming from user input cannot break out of the query. `eq = "Paris"` renders `eq: "Paris"`; `eq = userInput` is always safely escaped.
- **`Var("name")` is a reference to a declared variable.** It renders `$name` and must match a variable added with `AddVariable`.

```csharp
using static FluentGraphQL.GraphQL;

builder
    .AddVariable("city", "Paris")
    .AddQuery(new GraphQLQueryObject<Account>("accounts")
        .AddEveryFields()
        .WithArguments(new { where = new { city = new { eq = Var("city") } } }));
```

`AddVariable(name, value)` infers the GraphQL type from the value's type. Use the explicit `AddVariable(name, GraphQLParameterType.X, value)` overload when you need full control over the declared type.

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
