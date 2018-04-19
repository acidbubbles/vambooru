import { Component, OnInit  } from "@angular/core";
import { PostsService, PostSortBy, PostSortDirection, PostedSince } from "../../services/posts-service";
import { IPost } from "../../model/post";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
})
export class HomeComponent implements OnInit {
	highestRated: IPost[];
	newest: IPost[];

	constructor(private readonly postsService: PostsService) {
	}
	ngOnInit() {
		this.postsService
			.searchPosts({
				sort: PostSortBy.highestRated,
				direction: PostSortDirection.down,
				since: PostedSince.forever,
				page: 0,
				pageSize: 8
			})
			.subscribe(result => {
				this.highestRated = result;
			});

		this.postsService
			.searchPosts({
				sort: PostSortBy.created,
				direction: PostSortDirection.down,
				since: PostedSince.forever,
				page: 0,
				pageSize: 8
			})
			.subscribe(result => {
				this.newest = result;
			});
	}
}
