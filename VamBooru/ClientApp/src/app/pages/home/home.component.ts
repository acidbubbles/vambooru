import { Component, OnInit } from "@angular/core";
import { PostsService, PostSortBy, PostSortDirection, PostedSince, IPostQuery } from "../../services/posts-service";
import { TagsService } from "../../services/tags-service";
import { IPost } from "../../model/post";
import { ITag } from "../../model/tag";

@Component({
	selector: "app-home",
	templateUrl: "./home.component.html",
	styleUrls: ["./home.component.css"]
})
export class HomeComponent implements OnInit {
	highestRatedQuery: IPostQuery = {
		sort: PostSortBy.votes,
		direction: PostSortDirection.down,
		since: PostedSince.forever,
		page: 0,
		pageSize: 6,
		tags: []
	};
	highestRated: IPost[];
	highestRatedError: string;
	topTags: ITag[];

	recentlyCreatedQuery: IPostQuery = {
		sort: PostSortBy.created,
		direction: PostSortDirection.down,
		since: PostedSince.forever,
		page: 0,
		pageSize: 6,
		tags: []
	};
	recentlyCreated: IPost[];
	recentlyCreatedError: string;

	constructor(private readonly postsService: PostsService, private readonly tagsService: TagsService) {
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

		this.tagsService.loadTopTags()
			.subscribe(
				result => {
					this.topTags = result;
				},
				error => {
					this.topTags = [{ name: "error", id: "", postsCount: 0 }];
				}
			);
	}
}
