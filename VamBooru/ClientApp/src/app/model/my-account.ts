import { IPost } from "./post";

export interface IMyAccount {
	username: string;
	myPosts: IPost[];
}

export interface IUpdateAccount {
	username: string;
}
