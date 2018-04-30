import { NgModule, APP_INITIALIZER } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { RouterModule } from "@angular/router";

// External modules
import { TagInputModule } from "ngx-chips";
import { MarkdownModule } from "ngx-md";

// Services
import { ConfigurationService } from "./services/configuration-service";
import { PostsService } from "./services/posts-service";
import { UsersService } from "./services/users-service";
import { MyAccountService } from "./services/my-account-service";
import { VotesService } from "./services/votes-service";
import { TagsService } from "./services/tags-service";
import { PostCommentsService } from "./services/post-comments-service";

// Application
import { AppComponent } from "./app.component";
import { NavMenuComponent } from "./nav-menu/nav-menu.component";

// Directives
import { PostGalleryComponent } from"./components/post-gallery/post-gallery.component";

// Pages
import { HomeComponent } from "./pages/home/home.component";
import { ErrorComponent } from "./pages/error/error.component";
import { SignInComponent } from "./pages/signin/signin.component";
import { WelcomeComponent } from "./pages/welcome/welcome.component";
import { BrowseComponent } from "./pages/browse/browse.component";
import { PostComponent } from "./pages/post/post.component";
import { PostEditComponent } from "./pages/post-edit/post-edit.component";
import { UploadComponent } from "./pages/upload/upload.component";
import { MyAccountComponent } from "./pages/my-account/my-account.component";
import { UserComponent } from "./pages/user/user.component";

@NgModule({
	declarations: [
		AppComponent,
		NavMenuComponent,
		PostGalleryComponent,
		HomeComponent,
		ErrorComponent,
		SignInComponent,
		WelcomeComponent,
		BrowseComponent,
		PostComponent,
		PostEditComponent,
		UploadComponent,
		MyAccountComponent,
		UserComponent
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
			{ path: "signin", component: SignInComponent, },
			{ path: "welcome", component: WelcomeComponent, },
			{ path: "browse", component: BrowseComponent },
			{ path: "posts/:id", component: PostComponent },
			{ path: "posts/:id/edit", component: PostEditComponent },
			{ path: "upload", component: UploadComponent },
			{ path: "me", component: MyAccountComponent },
			{ path: "users/:id", component: UserComponent },
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
		},
		PostsService,
		UsersService,
		MyAccountService,
		VotesService,
		TagsService,
		PostCommentsService
	],
	bootstrap: [AppComponent]
})
export class AppModule { }
