import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { ActivatedRoute, Router } from "@angular/router";
import { Observable } from "rxjs/Observable";
import "rxjs/add/operator/map";
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

	constructor(private readonly http: HttpClient, private readonly router: Router, private readonly route: ActivatedRoute, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			const id = params["id"];
			this.http.get<IPost>(`${this.baseUrl}api/posts/${id}`).subscribe(result => {
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

		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" })
		};

		this.http.put(`/api/posts/${this.post.id}`, this.post, httpOptions).subscribe(
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
		const url = `/api/tags?q=${text}`;
		return this.http
			.get<ITag[]>(url)
			.map(data => data.map(item => item));;
	};
}
