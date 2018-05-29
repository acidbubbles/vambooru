# VamBooru Heroku Setup

1. Create a dyno, and set it up so that is uses this git repo
2. Create a Heroku Postgres database
3. Create the following environment variables

* `Authentication:Scheme`
* `Authentication:GitHub:ClientId`
* `Authentication:GitHub:ClientSecret`
* `DOTNET_SKIP_FIRST_TIME_EXPERIENCE` to `true` to avoid `dotnet` commands to take forever
* `Logging:LogLevel:Default`, `Logging:LogLevel:Microsoft` and `Logging:LogLevel:System` if you want easier control on logging
* `Repository:EFPostgres:ConnectionString` to `$DATABASE_URL` - This will read the `DATABASE_URL` created by Heroku, which should look like `postgres://(username):(password)@(server):(port)/(db)`, and transform it to something like `Server=(server);Port=(port);Database=(db);User Id=(user);Password=(password);`
* `DataProtection:Redis:Url` to `$REDIS_URL`

Once everything is ready, run this command to get the database up: `heroku run "cd VamBooru; dotnet restore; dotnet ef database update"`

You may want to add Papertrail or another logging service.

## Updating

After pushing a new version, if there's a database change run:

```
heroku run "cd VamBooru; dotnet restore; dotnet ef database update"
```
