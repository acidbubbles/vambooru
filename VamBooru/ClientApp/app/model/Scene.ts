import { ITag } from "./tag";

export interface IScene {
	id: string;
	title: string;
	imageUrl: number;
	published: boolean;
	tags: ITag[];
}