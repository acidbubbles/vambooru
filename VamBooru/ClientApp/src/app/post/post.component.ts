import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { HttpClient } from "@angular/common/http";
import { ActivatedRoute } from "@angular/router";
import { IPost } from "../model/post";

@Component({
	selector: "post",
	templateUrl: "./post.component.html"
})
export class PostComponent implements OnInit, OnDestroy {
	post: IPost;
	routeSub: Subscription;

	constructor(private readonly http: HttpClient, private readonly route: ActivatedRoute, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			const id = params["id"];
			this.http.get<IPost>(this.baseUrl + "api/Posts/" + id).subscribe(result => {
				this.post = result;
			});
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
	}
}
