import { async, ComponentFixture, TestBed } from "@angular/core/testing";
import { Observable } from "rxjs";
import "rxjs/add/observable/of";

import { PostsService, PostSortBy, PostSortDirection, PostedSince, IPostQuery } from "../../services/posts-service";
import { TagsService } from "../../services/tags-service";
import { IPost } from "../../model/post";
import { ITag } from "../../model/tag";

import { RouterLinkDirectiveStub, QueryParamsDirectiveStub } from "../../../../test/stubs/angular/core/router-directives.stubs";
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
				QueryParamsDirectiveStub,
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
				},
				{
					provide: TagsService,
					useClass: class {
						loadTopTags(): Observable<ITag[]> {
							return Observable.of([{ name: "tag1", postsCount: 2 } as ITag]);
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
			expect(query.pageSize).toEqual(6);
			expect(query.tags).toEqual([]);
			expect(query.author).toEqual("");
			expect(query.text).toEqual("");

			if(query.sort === PostSortBy.votes)
				return Observable.of([{ title: "Good post", author: { username: "user1" } } as IPost]);
			else if (query.sort === PostSortBy.created)
				return Observable.of([{ title: "New post", author: { username: "user2" } } as IPost]);
			else
				throw new Error(`Unexpected sort: ${query.sort}`);
		};
		fixture = TestBed.createComponent(HomeComponent);
		component = fixture.componentInstance;
		fixture.detectChanges();
	});

	it("should show the top tags", async(() => {
		const firstTag = fixture.nativeElement.querySelector(".tags .badge");
		expect(firstTag.textContent).toEqual("tag1 (2)");
	}));
});
