import { Injectable, Inject } from "@angular/core";
import { HttpClient, HttpParams, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";

@Injectable()
export class VotesService {
	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {}

	setVote(postId: string, value: number): Observable<IVoteResult> {
		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" }),
		};
		return this.http.post<IVoteResult>(`${this.baseUrl}api/votes/${postId}`, { value: value }, httpOptions);
	};

	getVote(postId: string): Observable<IVoteValue> {
		return this.http.get<IVoteValue>(`${this.baseUrl}api/votes/${postId}`);
	};
}

export interface IVoteValue {
	value: number;
}

export interface IVoteResult {
	difference: number;
}
