import { Component } from "@angular/core";
import { ConfigurationService } from "../services/configuration-service";

@Component({
	selector: "app-nav-menu",
	templateUrl: "./nav-menu.component.html",
	styleUrls: ["./nav-menu.component.css"]
})
export class NavMenuComponent {
	isExpanded = false;
	isAuthenticated: boolean;

	constructor(private readonly configService: ConfigurationService) {
		this.isAuthenticated = configService.config.isAuthenticated;
	}

	collapse() {
		this.isExpanded = false;
	}

	toggle() {
		this.isExpanded = !this.isExpanded;
	}
}
