import { Injectable, Inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs/Observable";
import { IPost } from "../model/post";

@Injectable()
export class PostsService {
	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {}

	load(query: IPostQuery): Observable<IPost[]> {
		const httpParams = new HttpParams({
			fromObject: query as any
		});
		return this.http.get<IPost[]>(`${this.baseUrl}api/posts`, { params: httpParams });
	};
}

export interface IPostQuery {
	sort: string;
	since: string;
	page: number;
	pageSize: number;
}

export class PostSortBy {
	static newest = "newest";
	static highestRated = "highestRated";
}

export class PostedSince {
	static forever = "forever";
	static lastDay = "lastDay";
	static lastWeek = "lastWeek";
	static lastMonth = "lastMonth";
	static lastYear = "lastYear";
}
