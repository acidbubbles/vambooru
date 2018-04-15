import { ITag } from "./tag";
import { IUser } from "./user";

export interface IPost {
	id: string;
	title: string;
	text: string;
	imageUrl: number;
	published: boolean;
	tags: ITag[];
	author: IUser;
	votes: number;
}
