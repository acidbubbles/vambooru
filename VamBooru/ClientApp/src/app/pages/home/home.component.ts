import { Component, OnInit  } from "@angular/core";
import { PostsService, PostSortBy, PostSortDirection, PostedSince } from "../../services/posts-service";
import { IPost } from "../../model/post";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
})
export class HomeComponent implements OnInit {
	highestRated: IPost[];
	highestRatedError: string;
	recentlyCreated: IPost[];
	recentlyCreatedError: string;

	constructor(private readonly postsService: PostsService) {
	}
	ngOnInit() {
		this.highestRatedError = null;
		this.postsService
			.searchPosts({
				sort: PostSortBy.votes,
				direction: PostSortDirection.down,
				since: PostedSince.forever,
				page: 0,
				pageSize: 8
			})
			.subscribe(
				result => {
					this.highestRated = result;
				},
				error => {
					this.highestRatedError = error.message;
				}
			);

		this.recentlyCreatedError = null;
		this.postsService
			.searchPosts({
				sort: PostSortBy.created,
				direction: PostSortDirection.down,
				since: PostedSince.forever,
				page: 0,
				pageSize: 8
			})
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
