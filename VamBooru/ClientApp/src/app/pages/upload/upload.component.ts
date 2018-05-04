import { Component, ViewChild, ElementRef, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { HttpClient } from "@angular/common/http";
import { Router, ActivatedRoute } from "@angular/router";
import { IUploadResponse } from "../../model/upload-response";

@Component({
	selector: "upload",
	templateUrl: "./upload.component.html"
})
export class UploadComponent implements OnInit {
	@ViewChild("filesInput") filesInput: ElementRef;
	routeSub: Subscription;
	postId: string;
	files: File[] = [];
	errorMessage: string;
	uploading: boolean;

	constructor(private readonly http: HttpClient, private readonly router: Router, private readonly route: ActivatedRoute) {
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			this.postId = params["id"];
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
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

		const url = this.postId ? `/api/upload/posts/${this.postId}` : "/api/upload";
		this.http.post<IUploadResponse>(url, formData, {}).subscribe(
			result => {
				this.router.navigate(["/posts", result.id, "edit"]);
			},
			error => {
				this.errorMessage = (error.error ? error.error.code : error.message) || "Upload failed";
				this.uploading = false;
			});
	}
}
