import { ITag } from "./tag";
import { IUser } from "./user";
import { IScene } from "./scene";
import { IFile } from "./file";

export interface IPost {
	id: string;
	title: string;
	text: string;
	thumbnailUrl: string;
	version: number;
	published: boolean;
	tags: ITag[];
	author: IUser;
	scenes: IScene[];
	files: IFile[];
	votes: number;
}
