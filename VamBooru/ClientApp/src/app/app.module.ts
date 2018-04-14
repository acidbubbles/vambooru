import { NgModule, APP_INITIALIZER } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { RouterModule } from "@angular/router";

import { TagInputModule } from "ngx-chips";
import { MarkdownModule } from "ngx-md";

import { ConfigurationService } from "./services/configuration-service";

import { AppComponent } from "./app.component";
import { NavMenuComponent } from "./nav-menu/nav-menu.component";

import { HomeComponent } from "./pages/home/home.component";
import { ErrorComponent } from "./pages/error/error.component";
import { BrowseComponent } from "./pages/browse/browse.component";
import { PostComponent } from "./pages/post/post.component";
import { PostEditComponent } from "./pages/post-edit/post-edit.component";
import { UploadComponent } from "./pages/upload/upload.component";
import { AccountComponent } from "./pages/account/account.component";

@NgModule({
	declarations: [
		AppComponent,
		NavMenuComponent,
		HomeComponent,
		ErrorComponent,
		BrowseComponent,
		PostComponent,
		PostEditComponent,
		UploadComponent,
		AccountComponent
	],
	imports: [
		BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
		BrowserAnimationsModule,
		HttpClientModule,
		FormsModule,
		ReactiveFormsModule,
		TagInputModule,
		MarkdownModule.forRoot(),
		RouterModule.forRoot([
			{ path: "", component: HomeComponent, pathMatch: "full" },
			{ path: "error", component: ErrorComponent,  },
			{ path: "browse", component: BrowseComponent },
			{ path: "posts/:id", component: PostComponent },
			{ path: "posts/:id/edit", component: PostEditComponent },
			{ path: "upload", component: UploadComponent },
			{ path: "account", component: AccountComponent },
			{ path: "**", redirectTo: "error" }
		])
	],
	providers: [
		ConfigurationService,
		{
			provide: APP_INITIALIZER,
			useFactory: (configurationService: ConfigurationService) => () => configurationService.load(),
			deps: [ConfigurationService],
			multi: true
		}
	],
	bootstrap: [AppComponent]
})
export class AppModule { }
