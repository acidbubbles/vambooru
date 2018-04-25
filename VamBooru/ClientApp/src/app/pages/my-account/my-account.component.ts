import { Component, OnInit } from "@angular/core";
import { IMyAccount, IUpdateAccount } from "../../model/my-account";
import { MyAccountService } from "../../services/my-account-service";
import { ConfigurationService } from "../../services/configuration-service";

@Component({
  selector: "my-account",
  templateUrl: "./my-account.component.html",
})
export class MyAccountComponent implements OnInit {
	account: IMyAccount;
	editable: IUpdateAccount;
	errorMessage: string;
	saving: boolean = false;
	saved: boolean = false;

	constructor(private readonly myAccountService: MyAccountService, private readonly configService: ConfigurationService) { }

	ngOnInit() {
		this.myAccountService.getAccount().subscribe(
			result => {
				this.account = result;
				this.editable = { username: result.username };
			},
			error => {
				this.errorMessage = error.message;
			});
	}

	save() {
		if(this.saving) return;

		this.saved = false;
		this.saving = true;
		this.errorMessage = null;

		this.myAccountService.saveAccount(this.editable).subscribe(
			result => {
				this.saving = false;
				this.account.username = result.username;
				this.editable.username = result.username;
				this.configService.config.username = this.account.username;
				this.saved = true;
			},
			error => {
				this.errorMessage = error.message;
				this.saving = false;
				this.saved = true;
			});
	}
}
