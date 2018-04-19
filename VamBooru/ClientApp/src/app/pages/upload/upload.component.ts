import { Component, ViewChild, ElementRef } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { IUploadResponse } from "../../model/upload-response";

@Component({
	selector: "upload",
	templateUrl: "./upload.component.html"
})
export class UploadComponent {
	@ViewChild("filesInput") filesInput: ElementRef;
	files: File[] = [];
	errorMessage: string;
	uploading: boolean;

	constructor(private readonly http: HttpClient, private readonly router: Router) {
	}

	updateSelection() {
		this.errorMessage = null;
		const files: FileList = this.filesInput.nativeElement.files;
		for (let i = 0; i < files.length; i++) {
			const file = files[i];
			this.remove(file.name);
			this.files.push(file);
		}
	}

	remove(filename) {
		if (!filename) return;
		for (let i = 0; i < this.files.length; i++) {
			if (this.files[i].name === filename) {
				this.files.splice(i, 1);
				return;
			}
		}
	}

	upload() {
		if (this.uploading) return;
		this.uploading = true;

		const formData = new FormData();
		for (let i = 0; i < this.files.length; i++) {
			const file = this.files[i];
			formData.append("file", file, file.name);
		}

		this.http.post<IUploadResponse>("/api/upload", formData, {}).subscribe(
			result => {
				this.router.navigate(["/posts", result.id, "edit"]);
			},
			error => {
				this.errorMessage = (error.error ? error.error.code : error.message) || "Upload failed";
				this.uploading = false;
			});
	}
}
