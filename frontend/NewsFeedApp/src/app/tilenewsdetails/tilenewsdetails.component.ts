import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TileData } from '../services/news.service';
import { CommentService } from '../services/comment.service';
import { LikeService } from '../services/like.service';
import { SavedService } from '../services/saved.service';
import { FollowService } from '../services/follow.service';

@Component({
  selector: 'app-tilenewsdetails',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [CommentService, LikeService, SavedService, FollowService],
  templateUrl: './tilenewsdetails.component.html',
  styleUrls: ['./tilenewsdetails.component.css']
})
export class TilenewsdetailsComponent implements OnInit {
  @Input() newsItem: TileData | null = null;
  @Output() backClicked = new EventEmitter<void>();

  showComments = false;
  newComment: string = '';
  successMessage: string = '';
  u_id: number = parseInt(localStorage.getItem('userId') || '0');

  constructor(
    private commentService: CommentService,
    private likeService: LikeService,
    private savedService: SavedService,
    private followService: FollowService
  ) {}

  ngOnInit(): void {
    if (!this.newsItem) return;

    this.likeService.getFullStatus(this.newsItem.news_id, this.u_id).subscribe({
      next: res => {
        this.newsItem!.hasLiked = res.hasLiked;
        this.newsItem!.hasUnliked = res.hasUnliked;
        this.newsItem!.hasSaved = res.hasSaved;
      },
      error: err => console.error('Failed to get full status', err)
    });

    this.commentService.getCommentsByNewsId(this.newsItem.news_id).subscribe({
      next: (comments) => {
        this.newsItem!.comments = comments;
      },
      error: (err) => {
        console.error('Failed to load comments:', err);
      }
    });
  }

  goBack(): void {
    this.backClicked.emit();
  }

  follow(): void {
    if (!this.newsItem || !this.newsItem.u_id || this.newsItem.u_id === 0) {
      alert("Error: Post author ID is missing or invalid.");
      return;
    }

    if (this.u_id === 0) {
      alert("Error: Logged-in user ID is missing.");
      return;
    }

    const isCurrentlyFollowed = this.newsItem.isFollowed;

    this.followService.followUser(this.newsItem.u_id, this.u_id).subscribe({
      next: (res: any) => {
        this.successMessage = res.message || (isCurrentlyFollowed ? 'Unfollowed successfully!' : 'Followed successfully!');
        this.newsItem!.isFollowed = !isCurrentlyFollowed;
      },
      error: (err: any) => {
        console.error('Error following/unfollowing user:', err);
        alert(err?.error?.message || "Failed to follow/unfollow user.");
      }
    });
  }

  toggleComments(): void {
    this.showComments = !this.showComments;
  }

  addComment(): void {
    if (!this.newsItem || !this.newComment.trim()) return;

    const commentPayload = {
      news_id: this.newsItem.news_id,
      u_id: this.u_id,
      comments: this.newComment
    };

    this.commentService.addComment(commentPayload).subscribe({
      next: () => {
        this.successMessage = 'Comment added successfully';
        this.newComment = '';

        this.commentService.getCommentsByNewsId(this.newsItem!.news_id).subscribe({
          next: (comments) => {
            this.newsItem!.comments = comments;
          },
          error: (err) => {
            console.error('Failed to reload comments:', err);
          }
        });
      },
      error: (err: any) => {
        console.error('Error adding comment:', err);
      }
    });
  }

  like(): void {
    if (!this.newsItem || this.newsItem.hasLiked) return;

    this.likeService.performAction(this.newsItem.news_id, this.u_id, 'like').subscribe({
      next: () => {
        this.newsItem!.likes += 1;
        if (this.newsItem!.hasUnliked) {
          this.newsItem!.unlikes = Math.max(this.newsItem!.unlikes - 1, 0);
        }
        this.newsItem!.hasLiked = true;
        this.newsItem!.hasUnliked = false;
      },
      error: (err: any) => console.error(err)
    });
  }

  unlike(): void {
    if (!this.newsItem || this.newsItem.hasUnliked) return;

    this.likeService.performAction(this.newsItem.news_id, this.u_id, 'unlike').subscribe({
      next: () => {
        this.newsItem!.unlikes += 1;
        if (this.newsItem!.hasLiked) {
          this.newsItem!.likes = Math.max(this.newsItem!.likes - 1, 0);
        }
        this.newsItem!.hasLiked = false;
        this.newsItem!.hasUnliked = true;
      },
      error: (err: any) => console.error(err)
    });
  }

  save(): void {
    if (!this.newsItem || this.newsItem.hasSaved) return;

    this.savedService.save(this.newsItem.news_id, this.u_id).subscribe({
      next: () => {
        this.newsItem!.hasSaved = true;
        alert('Saved!');
      },
      error: (err: any) => console.error(err)
    });
  }

  unsave(): void {
    if (!this.newsItem || !this.newsItem.hasSaved) return;

    this.savedService.unsave(this.newsItem.news_id, this.u_id).subscribe({
      next: () => {
        this.newsItem!.hasSaved = false;
        alert('Unsaved!');
      },
      error: (err: any) => console.error('Error unsaving:', err)
    });
  }
}
