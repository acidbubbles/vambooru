# VamBooru

A potential implementation of a sharing site for [Virt-A-Mate](https://www.patreon.com/meshedvr) scenes.

The goal is to create a site for the community to share scenes, and eventually get them in Virt-A-Mate directly: https://trello.com/c/O61WprH2/15-community-sharing-of-scenes-in-app

## Setup

This project requires .NET Core 2 (uses Postgres as the DB).

Either use Visual Studio, or `dotnet build` to compile.

Run the DB Migration using `Update-Database` in the Package Manager Console or `dotnet ef database update` using the .NET Core CLI. You need to first create the `vambooru` database and give the appropriate permissions.

You'll also need to add a data folder (and create it) and the connection string to the [secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?tabs=visual-studio) or tge environment variables See `appsettings.json`; when using environment variables, use `:` as the delimiter (e.g. `Repository:EFPostgres:ConnectionString`).

## Testing

Tests are done using Karma and NUnit.

Run C# unit tests with:

```
dotnet test VamBooru.Tests/
```

Run [unit](https://karma-runner.github.io) and [e2e](http://www.protractortest.org/) tests with [Angular 5](https://github.com/angular/angular-cli):

```
cd VamBooru/ClientApp
ng test
ng e2e
```

## Scalability

This was built to allow scaling, but it was implemented for simplicity. Things to do to scale much more:

1. Use redis to cache the home page search result
2. Use rabbitmq to queue uploads, and process them in another service
3. Use a file storage that provides a CDN, and upload there instead of in postgres directly

## Things to do

* User profile pages (public and edit)
* Admin role (ability to assign moderators and other admins)
* Moderator role (ability to disable users and delete/merge tags)
* Some e2e tests
* Unit tests
* Voting
* Some browsing functionality (popular, votes)
* Make an actual gallery (that shows the author, the date, votes, some tags and a link to view)
* Ability to overwrite and existing scene (last updated) with maybe release notes? So we can have "new version" in VaM.
* Max upload size (both json and image)
* CDN / Storage URL
* Max female/male count in tags
* Tag autocomplete component (both find and add)
* Upload support files (other images and sounds associated with a list of scenes)
* Download as zip (with the right folder structure, including the author folder)
* My Scenes
* User Avatar
* Error handling (global and per-page, e.g. httpclient.get())
* Ensure username are unique
* Implement queuing where possible (e.g. uploading)
* Use a MemoryCache for the home page stuff
* Use the database to test things on Heroku
* Compress the JSON files before storing them in the database!
* Update all dependencies (.NET, npm)
* Contributing guide / license
* Show the image in a lightbox when clicking
* Full text search of post text and title and tags
* Search by tag
* Make the post view page at least not completely ugly
