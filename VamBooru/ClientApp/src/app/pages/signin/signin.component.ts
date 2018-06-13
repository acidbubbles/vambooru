import { Component } from "@angular/core";
import { ConfigurationService } from "../../services/configuration-service";

@Component({
  selector: "signin",
  templateUrl: "./signin.component.html",
})
export class SignInComponent {
	signInButtons: SignInButton[] = [];

	constructor(configService: ConfigurationService) {
		if (!configService.config.authSchemes || !configService.config.authSchemes.length) {
			this.signInButtons.push({
				label: "Error! No sign in providers registered.",
				link: "/error",
				css: "fas fa-exclamation-triangle"
			});
			return;
		}

		for (let i = 0; i < configService.config.authSchemes.length; i++) {
			switch (configService.config.authSchemes[i]) {
				case "GitHub":
					this.signInButtons.push({
						label: "Sign in with GitHub",
						link: "/auth/GitHub/login",
						css: "fab fa-github"
					});
					break;
				case "Stub":
					this.signInButtons.push({
						label: "Sign in (Stub)",
						link: "/auth/Stub/login",
						css: "far fa-smile"
					});
					break;
			}
		}
	}
}

class SignInButton {
	label: string;
	link: string;
	css: string;
}
