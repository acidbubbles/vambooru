import { Injectable, Inject } from "@angular/core";
import { HttpClient, HttpParams, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import { IPost } from "../model/post";

@Injectable()
export class PostsService {
	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {}

	searchPosts(query: IPostQuery): Observable<IPost[]> {
		const httpParams = new HttpParams({
			fromObject: query as any
		});
		return this.http.get<IPost[]>(`${this.baseUrl}api/posts`, { params: httpParams });
	};

	getPost(postId: string): Observable<IPost> {
		return this.http.get<IPost>(`${this.baseUrl}api/posts/${postId}`, {});
	};

	savePost(post: IPost): Observable<Object> {
		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" })
		};

		return this.http.put(`/api/posts/${post.id}`, post, httpOptions);
	}
}

export interface IPostQuery {
	sort: string;
	direction: string;
	since: string;
	page: number;
	pageSize: number;
	tags: string[];
	author: string;
	text: string;
}

export class PostSortBy {
	static created = "created";
	static updated = "updated";
	static votes = "votes";
}

export class PostSortDirection {
	static up = "up";
	static down = "down";
}

export class PostedSince {
	static forever = "forever";
	static lastDay = "lastDay";
	static lastWeek = "lastWeek";
	static lastMonth = "lastMonth";
	static lastYear = "lastYear";
}
