import { ITag } from "./tag";
import { IUser } from "./user";

export interface IPost {
	id: string;
	title: string;
	imageUrl: number;
	published: boolean;
	tags: ITag[];
	author: IUser;
}
