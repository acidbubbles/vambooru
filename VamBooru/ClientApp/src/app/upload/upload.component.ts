import { Component, ViewChild, ElementRef } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { IUploadResponse } from "../model/upload-response";

@Component({
	selector: "upload",
	templateUrl: "./upload.component.html"
})
export class UploadComponent {
	@ViewChild("filesInput") filesInput: ElementRef;
	filenames: string[];
	errorMessage: string;

	constructor(private readonly http: HttpClient, private readonly router: Router) {
	}

	updateSelection() {
		this.errorMessage = null;
		const filenames: string[] = [];
		const files: FileList = this.filesInput.nativeElement.files;
		for (let i = 0; i < files.length; i++) {
			const file = files[i];
			filenames.push(file.name);
		}
		this.filenames = filenames;
	}

	upload() {
		const formData = new FormData();
		const files = this.filesInput.nativeElement.files;
		for (let i = 0; i < files.length; i++) {
			const file = files[i];
			formData.append("file", file, file.name);
		}

		this.http.post<IUploadResponse>("/api/upload", formData, {}).subscribe(
			result => {
				this.router.navigate(["/posts", result.id, "edit"]);
			},
			error => {
				this.errorMessage = error.error.code;
			});
	}
}
