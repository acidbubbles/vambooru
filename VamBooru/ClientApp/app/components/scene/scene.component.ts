import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { Http } from "@angular/http";
import { ActivatedRoute } from "@angular/router";
import { IScene } from "../../model/scene";

@Component({
	selector: "scene",
	templateUrl: "./scene.component.html"
})
export class SceneComponent implements OnInit, OnDestroy {
	scene: IScene;
	routeSub: Subscription;

	constructor(private readonly http: Http, private readonly route: ActivatedRoute, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			const id = params["id"];
			this.http.get(this.baseUrl + "api/Scenes/" + id).subscribe(result => {
				this.scene = result.json();
			}, error => console.error(error));
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
	}
}
