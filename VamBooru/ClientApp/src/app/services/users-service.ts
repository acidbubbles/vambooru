import { Injectable, Inject } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs/Observable";
import { IUser } from "../model/user";

@Injectable()
export class UsersService {
	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {}

	getUser(userId: string): Observable<IUser> {
		return this.http.get<IUser>(`${this.baseUrl}api/users/${userId}`, {});
	};

	saveUser(user: IUser): Observable<IUser> {
		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" })
		};

		return this.http.put<IUser>(`/api/users/me`, user, httpOptions);
	}
}
