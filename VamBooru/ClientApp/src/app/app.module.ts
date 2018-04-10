import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { RouterModule } from "@angular/router";

import { TagInputModule } from "ngx-chips";

import { AppComponent } from "./app.component";
import { NavMenuComponent } from "./nav-menu/nav-menu.component";

import { HomeComponent } from "./home/home.component";
import { ScenesComponent } from "./scenes/scenes.component";
import { SceneComponent } from "./scene/scene.component";
import { SceneEditComponent } from "./scene-edit/scene-edit.component";
import { UploadComponent } from "./upload/upload.component";

@NgModule({
	declarations: [
		AppComponent,
		NavMenuComponent,
		HomeComponent,
		UploadComponent,
		ScenesComponent,
		SceneComponent,
		SceneEditComponent,
	],
	imports: [
		BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
		BrowserAnimationsModule,
		HttpClientModule,
		FormsModule,
		ReactiveFormsModule,
		TagInputModule,
		RouterModule.forRoot([
			{ path: "", component: HomeComponent, pathMatch: "full" },
			{ path: "upload", component: UploadComponent },
			{ path: "scenes", component: ScenesComponent },
			{ path: "scenes/:id", component: SceneComponent },
			{ path: "scenes/:id/edit", component: SceneEditComponent },
			{ path: "**", redirectTo: "home" }
		])
	],
	providers: [],
	bootstrap: [AppComponent]
})
export class AppModule { }
