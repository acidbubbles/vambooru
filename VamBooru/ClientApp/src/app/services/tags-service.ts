import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs/Observable";
import { ITag } from "../model/tag";

@Injectable()
export class TagsService {
	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {}

	searchTags = (text: string): Observable<ITag[]> => {
		const url = `/api/tags?q=${text}`;
		return this.http
			.get<ITag[]>(url)
			.map(data => data.map(item => item));;
	};
}
