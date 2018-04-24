import { Component, OnInit } from "@angular/core";
import { IUser } from "../../model/user";
import { UsersService } from "../../services/users-service";
import { ConfigurationService } from "../../services/configuration-service";

@Component({
  selector: "account",
  templateUrl: "./account.component.html",
})
export class AccountComponent implements OnInit {
	user: IUser;
	errorMessage: string;
	saving: boolean = false;
	saved: boolean = false;

	constructor(private readonly usersService: UsersService, private readonly configService: ConfigurationService) { }

	ngOnInit() {
		this.usersService.getUser("me").subscribe(result => {
			this.user = result;
		});
	}

	save() {
		if(this.saving) return;

		this.saved = false;
		this.saving = true;
		this.errorMessage = null;

		this.usersService.saveUser(this.user).subscribe(
			result => {
				this.saving = false;
				this.user.username = result.username;
				this.configService.config.username = this.user.username;
				this.saved = true;
			},
			error => {
				this.errorMessage = error.toString();
				this.saving = false;
				this.saved = true;
			});
	}
}
