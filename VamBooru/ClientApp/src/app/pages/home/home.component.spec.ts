import { async, ComponentFixture, TestBed } from "@angular/core/testing";
import { Observable } from "rxjs/Observable";
import "rxjs/add/observable/of";

import { PostsService, PostSortBy, PostSortDirection, PostedSince, IPostQuery } from "../../services/posts-service";
import { IPost } from "../../model/post";

import { RouterLinkDirectiveStub } from "../../../../test/stubs/angular/core/router-directives.stubs";
import { PostGalleryComponent } from "../../components/post-gallery/post-gallery.component";

import { HomeComponent } from "./home.component";

describe("HomeComponent", () => {
	let component: HomeComponent;
	let fixture: ComponentFixture<HomeComponent>;
	let searchQueryResult: (query: IPostQuery) => Observable<IPost[]>;

	beforeEach(async(() => {
		searchQueryResult = null;
		TestBed.configureTestingModule({
			declarations: [
				RouterLinkDirectiveStub,
				PostGalleryComponent,
				HomeComponent
			],
			providers: [
				{
					provide: PostsService,
					useClass: class {
						searchPosts(query: IPostQuery): Observable<IPost[]> {
							return searchQueryResult(query);
						}
					}
				}
			]
		}).compileComponents();
	}));

	beforeEach(() => {
		searchQueryResult = query => {
			expect(query.direction).toEqual(PostSortDirection.down);
			expect(query.since).toEqual(PostedSince.forever);
			expect(query.page).toEqual(0);
			expect(query.pageSize).toEqual(8);

			if(query.sort === PostSortBy.highestRated)
				return Observable.of([{ title: "Good post" } as IPost]);
			else if (query.sort === PostSortBy.created)
				return Observable.of([{ title: "New post" } as IPost]);
			else
				throw new Error(`Unexpected sort: ${query.sort}`);
		};
		fixture = TestBed.createComponent(HomeComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it("should show the home page", async(() => {
		expect(fixture.nativeElement.querySelector("h1").textContent).toEqual("Virt-A-Mate");
	}));
});
