import { Component, ViewChild, ElementRef } from "@angular/core";
import { Http, Headers, RequestOptions } from "@angular/http";

@Component({
	selector: "upload",
	templateUrl: "./upload.component.html"
})
export class UploadComponent {
	@ViewChild("sceneJsonFileInput") sceneJsonFileInput: ElementRef;
	@ViewChild("sceneThumbnailFileInput") sceneThumbnailFileInput: ElementRef;

	constructor(private readonly http: Http) {
	}

	upload() {
		const formData = new FormData();
		formData.append("json", this.sceneJsonFileInput.nativeElement.files[0], "project.json");
		formData.append("thumbnail", this.sceneThumbnailFileInput.nativeElement.files[0], "project.jpg");

		const headers = new Headers({});
		headers.append('Accept', 'application/json');

		const options = new RequestOptions({ headers });

		const url = "/api/scenes/upload";

		this.http.post(url, formData, options).subscribe(res => {
			const body = res.json();
			console.log(body.success);
		});
	}
}
