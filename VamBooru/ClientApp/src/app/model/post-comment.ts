import { IUser } from "./user";

export interface IPostComment {
	dateCreated: string;
	author: IUser;
	text: string;
}
