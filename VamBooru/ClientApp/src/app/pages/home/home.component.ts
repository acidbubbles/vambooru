import { Component, OnInit  } from "@angular/core";
import { PostsService, PostSortBy, PostSortDirection, PostedSince, IPostQuery } from "../../services/posts-service";
import { IPost } from "../../model/post";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
})
export class HomeComponent implements OnInit {
	highestRatedQuery: IPostQuery = {
				sort: PostSortBy.votes,
				direction: PostSortDirection.down,
				since: PostedSince.forever,
				page: 0,
				pageSize: 0
			};
	highestRated: IPost[];
	highestRatedError: string;

	recentlyCreatedQuery: IPostQuery = {
				sort: PostSortBy.created,
				direction: PostSortDirection.down,
				since: PostedSince.forever,
				page: 0,
				pageSize: 0
			};
	recentlyCreated: IPost[];
	recentlyCreatedError: string;

	constructor(private readonly postsService: PostsService) {
	}
	ngOnInit() {
		this.highestRatedError = null;
		const highestRatedQuery = { ...this.highestRatedQuery };
		highestRatedQuery.pageSize = 8;
		this.postsService
			.searchPosts(this.highestRatedQuery)
			.subscribe(
				result => {
					this.highestRated = result;
				},
				error => {
					this.highestRatedError = error.message;
				}
			);

		this.recentlyCreatedError = null;
		const recentlyCreatedQuery = { ...this.recentlyCreatedQuery };
		recentlyCreatedQuery.pageSize = 8;
		this.postsService
			.searchPosts(this.recentlyCreatedQuery)
			.subscribe(
				result => {
					this.recentlyCreated = result;
				},
				error => {
					this.recentlyCreatedError = error.message;
				}
			);
	}
}
