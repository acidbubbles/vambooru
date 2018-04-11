import { Component, OnInit, Inject } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { DOCUMENT } from "@angular/common"
import { IUser } from "../model/user"

@Component({
  selector: "account",
  templateUrl: "./account.component.html",
})
export class AccountComponent implements OnInit {
	user: IUser;
	saved: boolean = false;

	constructor( @Inject(DOCUMENT) private readonly document: any, private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) { }

	ngOnInit() {
		this.http.get<IUser>(this.baseUrl + "api/users/me").subscribe(result => {
			this.user = result;
		});
	}

	save() {
		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" })
		};

		this.http.put("/api/users/me", this.user, httpOptions).subscribe(result => {
			this.saved = true;
		});
	}
}
