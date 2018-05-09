import { Injectable, Inject } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Observable } from "rxjs";
import { IMyAccount, IUpdateAccount } from "../model/my-account";

@Injectable()
export class MyAccountService {
	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {}

	getAccount(): Observable<IMyAccount> {
		return this.http.get<IMyAccount>(`${this.baseUrl}api/account`, {});
	};

	saveAccount(account: IUpdateAccount): Observable<IUpdateAccount> {
		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" })
		};

		return this.http.put<IMyAccount>(`/api/account`, account, httpOptions);
	}
}
