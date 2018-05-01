# Roadmap

## Stability

* Make `ng e2e` work and write some tests
* There are many tests missing for this to become more than a prototype
* Privacy policy, terms of service (yes, it would be nice to be transparent about exactly what we store)

## Scalability

This was built to allow scaling, but it was implemented for simplicity. Things to do to scale much more:

1. Use redis to cache the home page search result
2. Use rabbitmq to queue uploads, and process them in another worker service
3. Use a file storage that provides a CDN, and upload there instead of in postgres directly
4. Full text search of post text and title instead of "contains"

## Features

* User profiles (bio, avatar)
* Admin/Moderator roles (ability to assign moderators and other admins, create/merge tags)
* Synonymous tags (multiple tags meaning the same thing should all refer to the same entry)
* Paging
* Ability to modify/overwrite existing scene files (last updated)
* Delete posts and users
* Edit / Delete comments
* Show the scenes you have voted for, and show who voted for your scene (do not show downvotes)
* Track downloads (popularity) and clicks (Redis)
* Notifications (new comment, like)
* Ability for anyone with enough reputation to tag anything
* Reputation system (user ranking)

## Improvements

* Review how votes work (right now, negative = -2, positive = 10)
* Branding and UI improvements (right now, it's a barebone bootstrap 4 site)
* Show the scene images in a lightbox when clicking
* Allow selecting the preferred scene, and upload/embed animated gifs or videos for showing off the work
* Validate all scene paths, make sure all dependencies are there, and auto tag animations
* Use built-in .NET Authenticate/Roles instead of checking manually
* Limit and validate comments and post description text size and content
* Comments paging
* Ctrl-enter to comment
* Convert comments date time to "X time ago"
* Show creating date of posts
