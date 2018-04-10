import { Component, OnInit, OnDestroy, Inject } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { ActivatedRoute, Router } from "@angular/router";
import { IScene } from "../model/scene";

@Component({
	selector: "scene-edit",
	templateUrl: "./scene-edit.component.html"
})
export class SceneEditComponent implements OnInit, OnDestroy {
	scene: IScene;
	routeSub: Subscription;

	constructor(private readonly http: HttpClient, private readonly router: Router, private readonly route: ActivatedRoute, @Inject("BASE_URL") private readonly baseUrl: string) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			const id = params["id"];
			this.http.get<IScene>(this.baseUrl + "api/scenes/" + id).subscribe(result => {
				this.scene = result;
				if (!this.scene.tags) this.scene.tags = [];
			}, error => console.error(error));
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
		delete this.scene;
	}

	save() {
		const httpOptions = {
			headers: new HttpHeaders({ "Content-Type": "application/json" })
		};

		this.http.put("/api/scenes/" + this.scene.id, this.scene, httpOptions).subscribe(result => {
			this.router.navigate(["/scenes", this.scene.id]);
		});
	}

	publish(state: boolean) {
		this.scene.published = state;
		this.save();
	}
}
