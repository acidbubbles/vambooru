import { Directive, Input } from "@angular/core";

@Directive({
	selector: "[routerLink]",
	host: { '(click)': "onClick()" }
})
export class RouterLinkDirectiveStub {
	@Input("routerLink") linkParams: any;
	navigatedTo: any = null;

	onClick() {
		this.navigatedTo = this.linkParams;
	}
}

@Directive({
	selector: "[routerLinkActive]",
})
export class RouterLinkActiveDirectiveStub {
	@Input("routerLinkActive") linkParams: any;
	navigatedTo: any = null;
}

@Directive({
	selector: "[routerLinkActiveOptions]",
})
export class RouterLinkActiveOptionsDirectiveStub {
	@Input("routerLinkActiveOptions") linkParams: any;
	navigatedTo: any = null;
}
