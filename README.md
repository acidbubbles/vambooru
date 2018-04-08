# VamBooru

A potential implementation of a sharing site for [Virt-A-Mate](https://www.patreon.com/meshedvr) scenes.

The goal is to create a site for the community to share scenes, and eventually get them in Virt-A-Mate directly: https://trello.com/c/O61WprH2/15-community-sharing-of-scenes-in-app

## Building

This project is using .NET Core 2.0, Entity Framework (SQL Server) and Angular 4.

Either use Visual Studio, or `dotnet build`.

Run the DB Migration using `Update-Database` in the Package Manager Console.

You'll also need to create the `VamBooru` data folder, and add any missing configuration entries in `appsettings.json`.

## Testing

Tests are done using Karma and NUnit.

Run C# unit tests with:

```
dotnet test VamBooru.Tests/
```

Run TypeScript unit tests with:

```
cd VamBooru
npm test
```