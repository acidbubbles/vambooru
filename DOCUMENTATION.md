# Documentation

## Database Maintenance

To refresh all calculated columns (e.g. if the vote value changes)

```
UPDATE "Tags" SET "PostsCount" = COALESCE((SELECT COUNT(*) FROM "PostTags" WHERE "PostTags"."TagId" = "Tags"."Id"), 0)
UPDATE "Posts" SET "Votes" = (SELECT COALESCE(SUM("UserPostVotes"."Votes"), 0) FROM "UserPostVotes" WHERE "UserPostVotes"."PostId" = "Posts"."Id")
```
