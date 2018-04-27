import { Component } from "@angular/core";
import { ConfigurationService } from "../../services/configuration-service";

@Component({
  selector: "welcome",
  templateUrl: "./welcome.component.html",
})
export class WelcomeComponent {
	username: string;

	constructor(configService: ConfigurationService) {
		this.username = configService.config.username;
	}
}
