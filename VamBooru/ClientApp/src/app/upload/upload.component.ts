import { Component, ViewChild, ElementRef } from "@angular/core";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Router } from "@angular/router";
import { IUploadResponse } from "../model/upload-response";

@Component({
	selector: "upload",
	templateUrl: "./upload.component.html"
})
export class UploadComponent {
	@ViewChild("sceneJsonFileInput") sceneJsonFileInput: ElementRef;
	@ViewChild("sceneThumbnailFileInput") sceneThumbnailFileInput: ElementRef;

	constructor(private readonly http: HttpClient, private readonly router: Router) {
	}

	upload() {
		const formData = new FormData();
		formData.append("json", this.sceneJsonFileInput.nativeElement.files[0], "project.json");
		formData.append("thumbnail", this.sceneThumbnailFileInput.nativeElement.files[0], "project.jpg");

		const httpOptions = {};

		this.http.post<IUploadResponse>("/api/upload/scene", formData, httpOptions).subscribe(result => {
			this.router.navigate(["/scenes", result.id, "edit"]);
		});
	}
}
