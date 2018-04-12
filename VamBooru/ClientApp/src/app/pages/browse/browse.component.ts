import { Component, OnInit, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { IPost } from "../../model/post";

@Component({
	selector: "browse",
	templateUrl: "./browse.component.html"
})
export class BrowseComponent implements OnInit {
	posts: IPost[];

	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.http.get<IPost[]>(this.baseUrl + "api/posts").subscribe(result => {
			this.posts = result;
		}, error => console.error(error));
	}
}
