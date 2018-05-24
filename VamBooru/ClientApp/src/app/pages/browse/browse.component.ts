import { Component, OnInit, OnDestroy  } from "@angular/core";
import { Router, ActivatedRoute } from "@angular/router";
import { Subscription, Observable } from "rxjs";
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
	commonTags: ITag[] = [];

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
		{ value: PostedSince.lastDay, label: "Last Day" }
	];

	constructor(private readonly postsService: PostsService, private readonly tagsService: TagsService, private readonly router: Router, private readonly route: ActivatedRoute) {
	}

	ngOnInit() {
		this.routeSub = this.route.queryParams.subscribe(params => {
			let tagsParam: any = params["tags"];
			if (typeof tagsParam === "string") {
				tagsParam = [tagsParam];
			}
			this.query = {
				sort: params["sort"] || PostSortBy.created,
				direction: params["direction"] || PostSortDirection.down,
				since: params["since"] || PostedSince.forever,
				page: params["page"] || 0,
				pageSize: params["pageSize"] || 24,
				tags: tagsParam,
				author: params["author"] || "",
				text: params["text"] || ""
		};
			if (this.query.tags) {
				this.tags = this.query.tags.map<ITag>(t => ({ name: t } as ITag));
			}
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
					this.refreshCommonTags(this.posts);
				},
				error => {
					this.error = error.message;
				}
			);
	}

	refreshCommonTags(posts) {
		if (!posts || !posts.length) return;
		const commonTags: ITag[] = [];
		const commonTagNames: string[] = [];
		for (let i = 0; i < posts.length; i++) {
			const post = posts[i];
			if (!post.tags || !post.tags.length) return;
			for (let tagIndex = 0; tagIndex < post.tags.length; tagIndex++) {
				const tag = post.tags[tagIndex];
				if (commonTagNames.indexOf(tag.name) === -1) {
					commonTags.push(tag);
					commonTagNames.push(tag.name);
				}
			}
		}
		this.commonTags = commonTags;
	}

	addTag(tag: ITag) {
		if (!this.posts) return;
		this.tags.push(tag);
		this.go();
	}

	autocompleteTags = (text: string): Observable<ITag[]> => {
		return this.tagsService.searchTags(text);
	};
}
