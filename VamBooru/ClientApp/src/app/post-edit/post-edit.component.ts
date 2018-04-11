import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { ActivatedRoute, Router } from "@angular/router";
import { IPost } from "../model/post";

@Component({
	selector: "post-edit",
	templateUrl: "./post-edit.component.html"
})
export class PostEditComponent implements OnInit, OnDestroy {
	post: IPost;
	routeSub: Subscription;

	constructor(private readonly http: HttpClient, private readonly router: Router, private readonly route: ActivatedRoute, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			const id = params["id"];
			this.http.get<IPost>(this.baseUrl + "api/posts/" + id).subscribe(result => {
				this.post = result;
				if (!this.post.tags) this.post.tags = [];
			}, error => console.error(error));
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
		delete this.post;
	}

	save() {
		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" })
		};

		this.http.put("/api/posts/" + this.post.id, this.post, httpOptions).subscribe(result => {
			this.router.navigate(["/posts", this.post.id]);
		});
	}

	publish(state: boolean) {
		this.post.published = state;
		this.save();
	}
}
