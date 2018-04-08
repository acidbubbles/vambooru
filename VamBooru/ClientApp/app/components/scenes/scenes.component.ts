import { Component, OnInit, Inject } from "@angular/core";
import { Http } from "@angular/http";
import { IScene } from "../../model/scene";

@Component({
	selector: "scenes",
	templateUrl: "./scenes.component.html"
})
export class ScenesComponent implements OnInit {
	scenes: IScene[];

	constructor(private readonly http: Http, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.http.get(this.baseUrl + "api/scenes").subscribe(result => {
			this.scenes = result.json() as IScene[];
		}, error => console.error(error));
	}
}