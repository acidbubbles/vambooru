import { Injectable, Inject } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import { IUser } from "../model/user";

@Injectable()
export class UsersService {
	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {}

	getUser(username: string): Observable<IUser> {
		return this.http.get<IUser>(`${this.baseUrl}api/users/${username}`, {});
	};
}
