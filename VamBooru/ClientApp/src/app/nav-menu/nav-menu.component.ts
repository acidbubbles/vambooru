import { Component } from "@angular/core";
import { ConfigurationService } from "../services/configuration-service";
import { IStartupConfiguration } from "../model/startup-configuration";

@Component({
	selector: "app-nav-menu",
	templateUrl: "./nav-menu.component.html",
	styleUrls: ["./nav-menu.component.css"]
})
export class NavMenuComponent {
	isExpanded = false;
	config: IStartupConfiguration;

	constructor(configService: ConfigurationService) {
		this.config = configService.config;
	}

	collapse() {
		this.isExpanded = false;
	}

	toggle() {
		this.isExpanded = !this.isExpanded;
	}
}
