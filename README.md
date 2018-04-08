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

## Things to do

* Authentication (OAuth)
* User profile pages (public and edit)
* Create and assign tags
* Admin role (ability to assign moderators and other admins)
* Moderator role (ability to disable users and delete/merge tags)
* Some e2e tests
* Unit tests
* Voting
* Some browsing functionality (popular, votes)
* More meta data: uploaded date
* Ability to overwrite and existing scene (last updated) with maybe release notes? So we can have "new version" in VaM.
* Check all places we can have a running operations and still click on e.g. Save
* Get rid of embedded css (e.g. scenes.component.html)
* Max upload size (both json and image)
* CDN / Storage URL
* Max female/male count in tags
* Tag autocomplete component (both find and add)