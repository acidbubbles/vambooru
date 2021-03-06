import { Component, Input } from "@angular/core";
import { IPost } from "../../model/post";

@Component({
	selector: "post-gallery",
	templateUrl: "./post-gallery.component.html",
	styleUrls: ["./post-gallery.component.css"]
})
export class PostGalleryComponent {
	@Input() posts: IPost[];
	@Input() error: string;
}
