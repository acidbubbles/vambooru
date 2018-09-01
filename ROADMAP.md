# Roadmap

## Stability

There should be no obvious bugs, but the stability can certainly be improved:

* Make `ng e2e` work and write some tests
  * http://www.dotnetcurry.com/aspnet-core/1433/end-to-end-testing-aspnet-core
* There should be a full code review and many tests added for:
  * ASP.Net Controllers
  * Angular Controllers
  * Non-happy-path cases (graceful failures)
* Privacy policy, terms of service (yes, it would be nice to be transparent about exactly what we store)
* Limit and sanitize username, comments, post titles and text

## Scalability

This was built to allow scaling, but it was implemented for simplicity. Things to do to scale much more:

* Use redis to cache the home page queries, search result, and anything that can benefit from caching
* Use redis to queue uploads, and process them in another worker service
* Use a file storage that provides a CDN, and upload there instead of in postgres directly
* Full text search of post text and title instead of "contains"
* Pre-generate the zip upfront and store it for direct download

## Features

There can be an infinite list of things to add, but here's the core features that can be expected:

* Sign out
* DMCA Takedown link
* Paging (right now, only the first page is shown)
* Ability to modify/overwrite existing scene files (last updated, version)
* Notifications (new comment, like, new version of post files, follow post)
* Reputation system (user ranking)
* User profiles (bio, avatar, links to other sites)
* Admin/Moderator roles (ability to assign moderators and other admins, create/merge tags, disable accounts, block posts with DMCA notices, etc.)
* Synonymous tags (multiple tags meaning the same thing should all refer to the same entry)
* Improved comments system
  * Reference scenes and other users
  * Prevent linking directly to another non-approved site (warning)
  * Edit a comment
  * Delete a comment (mark as deleted)
* Delete posts and user accounts
* Show the scenes you have voted for, and show who voted for your scene (do not show downvotes)
* Track downloads (popularity) and views (Redis)
* Ability for anyone with enough reputation to tag anything (auto-role assignation)
* Simple blog-like posts (tutorials, news, etc.)
* Get updates for a list of given posts (e.g. "new updates"
* Attach a message with new post versions (what's new)

## Improvements

* Make browse searching case insensitive
* Do not return unused tags (and eventually clean them up, e.g. in a deamon)
* Update the browse URL as you search, to allow favorites or sharing
* On login, return to the original url (unless you were on the login page)
* Review how votes work (right now, negative = -2, positive = 10)
* Branding and UI improvements (right now, it's a barebone bootstrap 4 site)
* Show the scene images in a lightbox when clicking
* Allow selecting the scene to use in the browse screen
  * Allow uploading a custom image or gif for showing off the work
  * Explain how to create and link an animation
* Validate all scene dependencies paths, make sure all dependencies are there
* Automatically tag: animations, toys, etc.
* Comments paging
* Convert comments date time to "X time ago"
* Show published date of posts
* Save a SHA-256 hash of each file, and check for duplicates when uploading

## Technical

* Use built-in .NET Authenticate/Roles instead of checking manually
* Docker image for quick tests (https://docs.docker.com/samples/library/postgres/, https://docs.docker.com/engine/examples/dotnetcore/)
* Some analytics to track usage, uploads, etc. (anonymous as much as possible)
* Faster Postgres unit and e2e tests: https://stackoverflow.com/questions/9407442/optimise-postgresql-for-fast-testing
