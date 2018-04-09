import { NgModule } from "@angular/core";
import { BrowserModule } from "@angular/platform-browser";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { ReactiveFormsModule  } from "@angular/forms";
import { AppModuleShared } from "./app.shared.module";
import { AppComponent } from "./components/app/app.component";


@NgModule({
	bootstrap: [AppComponent],
	imports: [
		BrowserModule,
		BrowserAnimationsModule,
		ReactiveFormsModule,
		AppModuleShared
	],
	providers: [
		{ provide: "BASE_URL", useFactory: getBaseUrl }
	]
})
export class AppModule {
}

export function getBaseUrl() {
	return document.getElementsByTagName("base")[0].href;
}
