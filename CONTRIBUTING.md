# Contributing

Make sure you run all tests before doing a pull request. Also make sure your PR has a single purpose, follows the style, and does not compromise the stability or security of the platform.

https://blog.github.com/2015-01-21-how-to-write-the-perfect-pull-request/

## Technology

This project requires:

* .NET Core 2
* NodeJS 8
* Postgres

## Setup (Postgres)

Install Postgres, and create two database: `vambooru` and `vambooru_tests` (for unit tests). Make sure you have a user with write access.

## Setup (Visual Studio 2017)

Open `Vambooru.sln`, and build. Right-click on the VamBooru, select `Manage User Secrets` and use this template:

```
{
  "Repository": {
    "EFPostgres": {
      "ConnectionString": "Server=localhost;Database=vambooru;Username=vambooru;Password=..."
    }
  },
  "Authentication": {
    "GitHub": {
      "ClientId": "...",
      "ClientSecret": "..."
    }
  }
}
```

In the `Package Manager Console`:

```
Update-Database
```

Select the `VamBooru.Tests` project and again in the `Package Manager Console`:

```
dotnet restore
dotnet user-secrets set "Repository:EFPostgres:ConnectionString" "vambooru_tests-connection-string"
Update-Database
```

## Setup (Manual)

```
dotnet restore
dotnet build
cd VamBooru
dotnet user-secrets set "Repository:EFPostgres:ConnectionString" "vambooru-connection-string"
dotnet user-secrets set "Authentication:GitHub:ClientId" "your-client-id"
dotnet user-secrets set "Authentication:GitHub:ClientSecret" "your-client-secret"
dotnet database update
cd ..
cd VamBooru.Tests
dotnet user-secrets set "Repository:EFPostgres:ConnectionString" "vambooru_tests-connection-string"
dotnet database update
```

## Migrations

If you need to touch the database, use `Add-Migration MigrationName` and make sure it works in both directions.

## Testing

Run C# unit tests with:

```
dotnet test VamBooru.Tests/
```

Run [unit](https://karma-runner.github.io) and [e2e](http://www.protractortest.org/) tests with [Angular 5](https://github.com/angular/angular-cli):

```
cd VamBooru/ClientApp
ng test
ng e2e # Broken for now
```
