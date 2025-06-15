import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TileData } from '../services/news.service';
import { CommentService } from '../services/comment.service';
import { LikeService } from '../services/like.service';
import { SavedService } from '../services/saved.service';

@Component({
  selector: 'app-tilenewsdetails',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [CommentService, LikeService, SavedService],
  templateUrl: './tilenewsdetails.component.html',
  styleUrls: ['./tilenewsdetails.component.css']
})
export class TilenewsdetailsComponent {
  @Input() newsItem: TileData | null = null;
  @Output() backClicked = new EventEmitter<void>();

  showComments = false;
  newComment: string = '';
  successMessage: string = '';
  u_id: number = parseInt(localStorage.getItem('userId') || '0');

  constructor(
    private commentService: CommentService,
    private likeService: LikeService,
    private savedService: SavedService
  ) {}

  goBack(): void {
    this.backClicked.emit();
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
      },
      error: (err: any) => {
        console.error('Error adding comment:', err);
      }
    });
  }

  like(): void {
    if (!this.newsItem) return;

    this.likeService.like(this.newsItem.news_id, this.u_id).subscribe({
      next: () => {
        this.newsItem!.likes += 1;
      },
      error: (err: any) => console.error(err)
    });
  }

  unlike(): void {
    if (!this.newsItem) return;

    this.likeService.unlike(this.newsItem.news_id, this.u_id).subscribe({
      next: () => {
        this.newsItem!.unlikes += 1;
        if (this.newsItem!.likes > 0) {
          this.newsItem!.likes -= 1;
        }
      },
      error: (err: any) => console.error(err)
    });
  }

  save(): void {
    if (!this.newsItem) return;

    this.savedService.save(this.newsItem.news_id, this.u_id).subscribe({
      next: () => alert('Saved!'),
      error: (err: any) => console.error(err)
    });
  }
}
