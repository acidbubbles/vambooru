import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
	selector: 'scenes',
	templateUrl: './scenes.component.html'
})
export class ScenesComponent {
	public scenes: Scene[];

	constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
		http.get(baseUrl + 'api/Scenes/Browse').subscribe(result => {
			this.scenes = result.json() as Scene[];
		}, error => console.error(error));
	}
}

interface Scene {
	title: string;
	imageUrl: number;
}
