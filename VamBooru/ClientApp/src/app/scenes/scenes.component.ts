import { Component, OnInit, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { IScene } from "../model/scene";

@Component({
	selector: "scenes",
	templateUrl: "./scenes.component.html"
})
export class ScenesComponent implements OnInit {
	scenes: IScene[];

	constructor(private readonly http: HttpClient, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.http.get<IScene[]>(this.baseUrl + "api/scenes").subscribe(result => {
			this.scenes = result;
		}, error => console.error(error));
	}
}
