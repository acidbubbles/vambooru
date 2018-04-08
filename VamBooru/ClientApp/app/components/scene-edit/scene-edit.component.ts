import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { Http, Headers, RequestOptions } from "@angular/http";
import { ActivatedRoute, Router } from "@angular/router";
import { IScene } from "../../model/scene";

@Component({
	selector: "scene-edit",
	templateUrl: "./scene-edit.component.html"
})
export class SceneEditComponent implements OnInit, OnDestroy {
	scene: IScene;
	routeSub: Subscription;

	constructor(private readonly http: Http, private readonly router: Router, private readonly route: ActivatedRoute, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			const id = params["id"];
			this.http.get(this.baseUrl + "api/scenes/" + id).subscribe(result => {
				this.scene = result.json();
			}, error => console.error(error));
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
	}

	save() {
		const headers = new Headers({});
		headers.append("Accept", "application/json");

		const options = new RequestOptions({ headers });

		this.http.put("/api/scenes/" + this.scene.id, this.scene, options).subscribe(res => {
			this.router.navigate(["/scenes", this.scene.id]);
		});
	}

	publish() {
		this.scene.published = true;
		this.save();
	}
}
