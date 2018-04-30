import { Injectable, Inject } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs/Observable";
import { IPostComment } from "../model/post-comment";

@Injectable()
export class PostCommentsService {
	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) { }

	load(postId: string): Observable<IPostComment[]> {
		return this.http
			.get<IPostComment[]>(`/api/posts/${postId}/comments`);
	}

	send(postId: string, text: string): Observable<Object> {
		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" })
		};

		return this.http
			.post(`/api/posts/${postId}/comments`, { text: text }, httpOptions);
	}
}
