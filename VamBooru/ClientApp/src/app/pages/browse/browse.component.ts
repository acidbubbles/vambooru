import { Component, OnInit  } from "@angular/core";
import { PostsService, PostSortBy, PostSortDirection, PostedSince } from "../../services/posts-service";
import { IPost } from "../../model/post";

@Component({
	selector: "browse",
	templateUrl: "./browse.component.html"
})
export class BrowseComponent implements OnInit {
	posts: IPost[];
	error: string;

	constructor(private readonly postsService: PostsService) {
	}

	ngOnInit() {
		this.postsService
			.searchPosts({
				sort: PostSortBy.created,
				direction: PostSortDirection.down,
				since: PostedSince.forever,
				page: 0,
				pageSize: 0
			})
			.subscribe(
				result => {
					this.posts = result;
				},
				error => {
					this.error = error.message;
				}
			);
	}
}
