import { Component, Inject } from "@angular/core";
import { Http } from "@angular/http";
import { IScene } from "../../model/scene";

@Component({
	selector: "scenes",
	templateUrl: "./scenes.component.html"
})
export class ScenesComponent {
	scenes: IScene[];

	constructor(http: Http, @Inject("BASE_URL") baseUrl: string) {
		http.get(baseUrl + "api/scenes").subscribe(result => {
			this.scenes = result.json() as IScene[];
		}, error => console.error(error));
	}
}