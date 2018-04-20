import { Component, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { ActivatedRoute, Router } from "@angular/router";
import { Observable } from "rxjs/Observable";
import "rxjs/add/operator/map";
import { PostsService } from "../../services/posts-service";
import { TagsService } from "../../services/tags-service";
import { IPost } from "../../model/post";
import { ITag } from "../../model/tag";

@Component({
	selector: "post-edit",
	templateUrl: "./post-edit.component.html"
})
export class PostEditComponent implements OnInit, OnDestroy {
	post: IPost;
	routeSub: Subscription;
	errorMessage: string;
	saving: boolean;

	constructor(private readonly tagsService: TagsService, private readonly postsService: PostsService, private readonly router: Router, private readonly route: ActivatedRoute) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			this.postsService.getPost(params["id"]).subscribe(result => {
				this.post = result;
				if (!this.post.tags) this.post.tags = [];
			});
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
		delete this.post;
	}

	save() {
		if (this.saving) return;
		this.saving = true;
		this.errorMessage = null;

		this.postsService.savePost(this.post).subscribe(
			() => {
				this.router.navigate(["/posts", this.post.id]);
			},
			error => {
				this.errorMessage = error.toString();
				this.saving = false;
			});
	}

	publish(state: boolean) {
		this.post.published = state;
		this.save();
	}

	autocompleteTags = (text: string): Observable<ITag[]> => {
		return this.tagsService.searchTags(text);
	};
}
