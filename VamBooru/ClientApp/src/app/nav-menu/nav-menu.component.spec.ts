import { async, ComponentFixture, TestBed } from "@angular/core/testing";
import { RouterLinkDirectiveStub, RouterLinkActiveDirectiveStub, RouterLinkActiveOptionsDirectiveStub } from "../../../test/stubs/angular/core/router-directives.stubs";

import { NavMenuComponent } from "./nav-menu.component";
import { ConfigurationService } from "../services/configuration-service";
import { IStartupConfiguration } from "../model/startup-configuration";

describe("NavMenuComponent", () => {
	let component: NavMenuComponent;
	let fixture: ComponentFixture<NavMenuComponent>;
	let startupConfiguration: IStartupConfiguration;

	beforeEach(async(() => {
		TestBed.configureTestingModule({
			declarations: [
				RouterLinkDirectiveStub,
				RouterLinkActiveDirectiveStub,
				RouterLinkActiveOptionsDirectiveStub,
				NavMenuComponent
			],
			providers: [
				{
					provide: ConfigurationService,
					useClass: class {
						get config() {
							return startupConfiguration;
						}
					}
				}
			]
		}).compileComponents();
	}));

	describe("not authenticated", () => {

		beforeEach(() => {
			startupConfiguration = {
				isAuthenticated: false,
				username: null
			};
			fixture = TestBed.createComponent(NavMenuComponent);
			component = fixture.componentInstance;
			fixture.detectChanges();
		});

		it("should show the login link only", async(() => {
			expect(fixture.nativeElement.querySelector(".glyphicon-log-in")).not.toBeNull();

			expect(fixture.nativeElement.querySelector(".glyphicon-cloud-upload")).toBeNull();
			expect(fixture.nativeElement.querySelector(".glyphicon-user")).toBeNull();
		}));

	});

	describe("authenticated", () => {

		beforeEach(() => {
			startupConfiguration = {
				isAuthenticated: true,
				username: "john.doe"
			};
			fixture = TestBed.createComponent(NavMenuComponent);
			component = fixture.componentInstance;
			fixture.detectChanges();
		});

		it("should show the upload and account links", async(() => {
			expect(fixture.nativeElement.querySelector(".glyphicon-log-in")).toBeNull();

			expect(fixture.nativeElement.querySelector(".glyphicon-cloud-upload")).not.toBeNull();
			expect(fixture.nativeElement.querySelector(".glyphicon-user").parentNode.innerText).toEqual("@john.doe");
		}));

	});

});
