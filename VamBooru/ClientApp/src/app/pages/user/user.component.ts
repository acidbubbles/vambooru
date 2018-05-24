import { Component, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs";
import { ActivatedRoute } from "@angular/router";
import { UsersService } from "../../services/users-service";
import { PostsService, PostSortBy, PostSortDirection, PostedSince, IPostQuery } from "../../services/posts-service";
import { IUser } from "../../model/user";
import { IPost } from "../../model/post";

@Component({
  selector: "user",
  templateUrl: "./user.component.html",
})
export class UserComponent implements OnInit, OnDestroy {
	user: IUser;
	posts: IPost[];
	postsError:string ;
	routeSub: Subscription;

	constructor(private readonly route: ActivatedRoute, private readonly usersService: UsersService, private readonly postsService: PostsService) { }

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			this.user = null;
			this.posts = null;
			this.postsError = null;
			this.usersService.getUser(params["id"]).subscribe(result => {
				this.user = result;

				var query: IPostQuery = {
					sort: PostSortBy.votes,
					direction: PostSortDirection.down,
					since: PostedSince.forever,
					page: 0,
					pageSize: 100,
					tags: [],
					author: this.user.username,
					text: ""
				};
				this.postsService
					.searchPosts(query)
					.subscribe(
						posts => {
							this.posts = posts;
						},
						error => {
							this.postsError = error.message;
						}
					);
			});
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
	}
}
