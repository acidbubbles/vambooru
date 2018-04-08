import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpModule } from "@angular/http";
import { RouterModule } from "@angular/router";

import { AppComponent } from "./components/app/app.component";
import { NavMenuComponent } from "./components/navmenu/navmenu.component";
import { HomeComponent } from "./components/home/home.component";
import { ScenesComponent } from "./components/scenes/scenes.component";
import { UploadComponent } from "./components/upload/upload.component";

@NgModule({
	declarations: [
		AppComponent,
		NavMenuComponent,
		UploadComponent,
		ScenesComponent,
		HomeComponent
	],
	imports: [
		CommonModule,
		HttpModule,
		FormsModule,
		RouterModule.forRoot([
			{ path: "", redirectTo: "home", pathMatch: "full" },
			{ path: "home", component: HomeComponent },
			{ path: "upload", component: UploadComponent },
			{ path: "scenes", component: ScenesComponent },
			{ path: "**", redirectTo: "home" }
		])
	]
})
export class AppModuleShared {
}
