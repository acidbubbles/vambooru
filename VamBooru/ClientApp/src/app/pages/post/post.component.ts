import { Component, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { ActivatedRoute } from "@angular/router";
import { ConfigurationService } from "../../services/configuration-service";
import { PostsService } from "../../services/posts-service";
import { PostCommentsService } from "../../services/post-comments-service";
import { VotesService, IVoteValue } from "../../services/votes-service";
import { IPost } from "../../model/post";
import { IPostComment } from "../../model/post-comment";

@Component({
	selector: "post",
	templateUrl: "./post.component.html"
})
export class PostComponent implements OnInit, OnDestroy {
	post: IPost;
	comments: IPostComment[];
	routeSub: Subscription;
	loggedInUsername: string;
	ownedByCurrentUser: boolean;
	vote: IVoteValue;
	voting: boolean;
	currentComment: IPostComment;

	constructor(private readonly route: ActivatedRoute, private readonly postsService: PostsService, private readonly commentsService: PostCommentsService, private readonly votesService: VotesService, configService: ConfigurationService) {
		this.loggedInUsername = configService.config.username;
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			this.post = null;
			this.ownedByCurrentUser = false;
			this.vote = null;
			this.voting = false;
			this.resetComment();

			this.postsService.getPost(params["id"]).subscribe(result => {
				this.post = result;
				this.ownedByCurrentUser = this.post.author.username === this.loggedInUsername;
			});

			this.commentsService.load(params["id"]).subscribe(result => {
				this.comments = result || [];
			});

			if (this.loggedInUsername) {
				this.votesService.getVote(params["id"]).subscribe(result => {
					this.vote = result;
				});
			}
		});
	}

	ngOnDestroy() {
		this.routeSub.unsubscribe();
	}

	changeVote(value: number) {
		if (this.voting) return;
		this.voting = true;
		if (value === this.vote.value) value = 0; // Cancelling the current vote
		this.vote.value = value;
		this.votesService.setVote(this.post.id, value).subscribe(result => {
			this.post.votes += result.difference;
			this.voting = false;
		});
	}

	resetComment() {
		this.currentComment = { text: "", author: { username: this.loggedInUsername }, dateCreated: "just now" } as IPostComment;
	}

	sendComment() {
		const comment = this.currentComment;
		this.currentComment = null;
		this.commentsService.send(this.post.id, comment.text).subscribe(
			() => {
				this.comments.unshift(comment);
				this.resetComment();
			},
			error => {
				this.currentComment = comment;
				this.comments.unshift({ text: `Error: ${error.message}` } as IPostComment);
			});
	}
}
