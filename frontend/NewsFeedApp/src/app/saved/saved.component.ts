import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';
import { LikeService } from '../services/like.service';
import { SavedService } from '../services/saved.service';
import { FollowService } from '../services/follow.service';
import { CommentService } from '../services/comment.service';

@Component({
  selector: 'app-saved',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './saved.component.html',
  styleUrls: ['./saved.component.css'],
  providers:[LikeService, SavedService, FollowService, CommentService]
})
export class SavedComponent implements OnInit {
  @Input() newsItem: TileData | null = null;
  @Output() tileClicked = new EventEmitter<TileData>();

  u_id: number = parseInt(localStorage.getItem('userId') || '0');
  savedNewsList: TileData[] = [];

  showComments = false;
  newComment: string = '';
  successMessage: string = '';
  loading: boolean = true;

  constructor(
    private savedService: SavedService,
    private likeService: LikeService,
    private followService: FollowService,
    private commentService: CommentService
  ) {}

  ngOnInit(): void {
    this.loadSavedNews();
  }

  loadSavedNews(): void {
    this.loading = true;
    this.savedService.getSavedNews(this.u_id).subscribe({
      next: (data) => {
        this.savedNewsList = data.map(news => ({
          ...news,
          isFollowed: true,
          isSaved: true, // since it's saved tab
          comments: news.comments || [],
          hasLiked: false,
          hasUnliked: false
        }));

        this.savedNewsList.forEach(news => {
          this.likeService.getStatus(news.news_id, this.u_id).subscribe({
            next: (status) => {
              news.hasLiked = status.hasLiked;
              news.hasUnliked = status.hasUnliked;
            },
            error: err => console.error('Error getting like status:', err)
          });
        });

        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load saved news:', err);
        this.loading = false;
      }
    });
  }

  selecttile(news: TileData): void {
    this.tileClicked.emit(news);
  }

  like(news: TileData): void {
    if (news.hasLiked) return;
    this.likeService.like(news.news_id, this.u_id).subscribe({
      next: () => {
        news.likes += 1;
        if (news.hasUnliked) news.unlikes = Math.max(news.unlikes - 1, 0);
        news.hasLiked = true;
        news.hasUnliked = false;
      },
      error: err => console.error(err)
    });
  }

  unlike(news: TileData): void {
    if (news.hasUnliked) return;
    this.likeService.unlike(news.news_id, this.u_id).subscribe({
      next: () => {
        news.unlikes += 1;
        if (news.hasLiked) news.likes = Math.max(news.likes - 1, 0);
        news.hasUnliked = true;
        news.hasLiked = false;
      },
      error: err => console.error(err)
    });
  }

  save(news: TileData): void {
    this.savedService.save(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = true;
        alert('Saved!');
      },
      error: err => console.error(err)
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
      },
      error: (err: any) => {
        console.error('Error adding comment:', err);
      }
    });
  }
}
