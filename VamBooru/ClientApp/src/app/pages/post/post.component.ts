import { Component, OnInit, OnDestroy } from "@angular/core";
import { Subscription } from "rxjs/Subscription";
import { ActivatedRoute } from "@angular/router";
import { IPost } from "../../model/post";
import { ConfigurationService } from "../../services/configuration-service";
import { PostsService } from "../../services/posts-service";
import { VotesService, IVoteValue } from "../../services/votes-service";

@Component({
	selector: "post",
	templateUrl: "./post.component.html"
})
export class PostComponent implements OnInit, OnDestroy {
	post: IPost;
	routeSub: Subscription;
	loggedInUsername: string;
	ownedByCurrentUser: boolean;
	vote: IVoteValue;
	voting: boolean;

	constructor(private readonly route: ActivatedRoute, private readonly postsService: PostsService, private readonly votesService: VotesService, configService: ConfigurationService) {
		this.loggedInUsername = configService.config.username;
	}

	ngOnInit() {
		this.routeSub = this.route.params.subscribe(params => {
			this.post = null;
			this.ownedByCurrentUser = false;
			this.vote = null;
			this.voting = false;
			this.postsService.getPost(params["id"]).subscribe(result => {
				this.post = result;
				this.ownedByCurrentUser = this.post.author.username === this.loggedInUsername;
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
}
