import { Component, OnInit, OnDestroy  } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { Subscription } from "rxjs/Subscription";
import { Observable } from "rxjs/Observable";
import { PostsService, PostSortBy, PostSortDirection, PostedSince, IPostQuery } from "../../services/posts-service";
import { TagsService } from "../../services/tags-service";
import { IPost } from "../../model/post";
import { ITag } from "../../model/tag";

@Component({
	selector: "browse",
	templateUrl: "./browse.component.html"
})
export class BrowseComponent implements OnInit, OnDestroy {
	routeSub: Subscription;
	posts: IPost[];
	error: string;
	query: IPostQuery;
	tags: ITag[] = [];

	sortValues = [
		{ value: PostSortBy.created, label: "Creation Date" },
		{ value: PostSortBy.updated, label: "Last Updated" },
		{ value: PostSortBy.votes, label: "Rating (votes)" }
	];

	directionValues = [
		{ value: PostSortDirection.down, label: "Descending" },
		{ value: PostSortDirection.up, label: "Ascending" }
	];

	sinceValues = [
		{ value: PostedSince.forever, label: "Forever" },
		{ value: PostedSince.lastYear, label: "Last Year" },
		{ value: PostedSince.lastMonth, label: "Last Month" },
		{ value: PostedSince.lastWeek, label: "Last Week" },
		{ value: PostedSince.lastDay, label: "Last Day" },
	];

	constructor(private readonly postsService: PostsService, private readonly tagsService: TagsService, private readonly router: Router, private readonly route: ActivatedRoute) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			this.query = {
				sort: params["sort"] || PostSortBy.created,
				direction: params["direction"] || PostSortDirection.down,
				since: params["since"] || PostedSince.forever,
				page: params["page"] || 0,
				pageSize: params["pageSize"] || 0,
				tags: this.tags.map(t => t.name)
		};
			this.go();
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
		delete this.posts;
	}

	go() {
		delete this.posts;
		this.query.tags = this.tags.map(t => t.name);
		this.postsService
			.searchPosts(this.query)
			.subscribe(
				result => {
					this.posts = result;
				},
				error => {
					this.error = error.message;
				}
			);
	}

	autocompleteTags = (text: string): Observable<ITag[]> => {
		return this.tagsService.searchTags(text);
	};
}
