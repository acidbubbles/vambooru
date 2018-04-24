import { Component, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { ActivatedRoute } from "@angular/router";
import { IUser } from "../../model/user";
import { UsersService } from "../../services/users-service";

@Component({
  selector: "user",
  templateUrl: "./user.component.html",
})
export class UserComponent implements OnInit, OnDestroy {
	user: IUser;
	routeSub: Subscription;

	constructor(private readonly route: ActivatedRoute, private readonly usersService: UsersService) { }

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			this.user = null;
			this.usersService.getUser(params["id"]).subscribe(result => {
				this.user = result;
			});
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
	}
}
