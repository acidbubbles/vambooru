import { Component, OnInit  } from "@angular/core";
import { PostsService, PostSortBy, PostedSince } from "../../services/posts-service";
import { IPost } from "../../model/post";

@Component({
	selector: "browse",
	templateUrl: "./browse.component.html"
})
export class BrowseComponent implements OnInit {
	posts: IPost[];

	constructor(private readonly postsService: PostsService) {
	}

	ngOnInit() {
		this.postsService
			.load({
				sort: PostSortBy.newest,
				since: PostedSince.forever,
				page: 0,
				pageSize: 0
			})
			.subscribe(result => {
				this.posts = result;
			});
	}
}
