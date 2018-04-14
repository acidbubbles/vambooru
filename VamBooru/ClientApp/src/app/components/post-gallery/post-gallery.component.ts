import { Component, Input } from "@angular/core";
import { IPost } from "../../model/post";

@Component({
	selector: "post-gallery",
	templateUrl: "./post-gallery.component.html"
})
export class PostGalleryComponent {
	@Input() posts: IPost[];
}
