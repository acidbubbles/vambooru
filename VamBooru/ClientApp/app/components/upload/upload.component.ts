import { Component, ViewChild, ElementRef } from '@angular/core';
import { Http, Headers, RequestOptions } from '@angular/http';

@Component({
	selector: 'upload',
	templateUrl: './upload.component.html'
})
export class UploadComponent {
	@ViewChild('sceneJsonFileInput') sceneJsonFileInput: ElementRef;
	@ViewChild('sceneImageFileInput') sceneImageFileInput: ElementRef;

	constructor(private http: Http) {
	}

	upload() {
		var jsonFile = this.sceneJsonFileInput.nativeElement.files[0];
		var imageFile = this.sceneImageFileInput.nativeElement.files[0];
		const formData = new FormData();
		formData.append("json", jsonFile);
		formData.append("image", imageFile);
		const headers = new Headers({});
		let options = new RequestOptions({ headers });
		let url = "/api/scenes/upload";

		this.http.post(url, formData, options).subscribe(res => {
			let body = res.json();
			console.log(body.success);
		});
	}
}
