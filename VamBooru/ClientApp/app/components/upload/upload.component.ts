import { Component } from '@angular/core';

@Component({
	selector: 'upload',
	templateUrl: './upload.component.html'
})
export class UploadComponent {
	public currentCount = 0;

	public incrementUpload() {
		this.currentCount++;
	}
}
