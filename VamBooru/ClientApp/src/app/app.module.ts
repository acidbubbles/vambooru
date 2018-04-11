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
import { BrowseComponent } from "./browse/browse.component";
import { PostComponent } from "./post/post.component";
import { PostEditComponent } from "./post-edit/post-edit.component";
import { UploadComponent } from "./upload/upload.component";

@NgModule({
	declarations: [
		AppComponent,
		NavMenuComponent,
		HomeComponent,
		UploadComponent,
		BrowseComponent,
		PostComponent,
		PostEditComponent,
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
			{ path: "browse", component: BrowseComponent },
			{ path: "posts/:id", component: PostComponent },
			{ path: "posts/:id/edit", component: PostEditComponent },
			{ path: "**", redirectTo: "home" }
		])
	],
	providers: [],
	bootstrap: [AppComponent]
})
export class AppModule { }
