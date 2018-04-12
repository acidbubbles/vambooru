import { HomePage } from "./pages/home.po";

describe("Home", () => {
	let page: HomePage;

	beforeEach(() => {
		page = new HomePage();
	});

	it("should display welcome message", () => {
		page.navigateTo();
		expect(page.getMainHeading()).toEqual("Virt-A-Mate");
	});
});
