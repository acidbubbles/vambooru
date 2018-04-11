import { ITag } from "./tag";

export interface IPost {
	id: string;
	title: string;
	imageUrl: number;
	published: boolean;
	tags: ITag[];
}
