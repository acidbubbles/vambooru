import { Component, ViewChild, ElementRef } from "@angular/core";
import { HttpClient } from "@angular/common/http";
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
		const sceneJsonFile = this.sceneJsonFileInput.nativeElement.files[0];
		const sceneJpgFile = this.sceneThumbnailFileInput.nativeElement.files[0];
		formData.append("json", sceneJsonFile,sceneJsonFile.name);
		formData.append("jpg", sceneJpgFile, sceneJpgFile.name);

		this.http.post<IUploadResponse>("/api/upload", formData, {}).subscribe(result => {
			this.router.navigate(["/posts", result.id, "edit"]);
		});
	}
}
