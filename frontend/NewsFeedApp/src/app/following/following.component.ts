import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TileData } from '../services/news.service';
import { LikeService } from '../services/like.service';
import { FollowService } from '../services/follow.service';
import { NewsService } from '../services/news.service';
import { SavedService } from '../services/saved.service';
import { CommentService } from '../services/comment.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-following',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './following.component.html',
  styleUrls: ['./following.component.css'],
  providers: [FollowService, NewsService, LikeService, SavedService, CommentService]
})
export class FollowingComponent implements OnInit {
  @Input() newsItem: TileData | null = null;
  @Output() tileClicked = new EventEmitter<TileData>();

  u_id: number = parseInt(localStorage.getItem('userId') || '0');
  followedNewsList: (TileData & {
    showComments?: boolean;
    newComment?: string;
    successMessage?: string;
    hasLiked?: boolean;
    hasUnliked?: boolean;
    hasSaved?: boolean;
  })[] = [];

  loading: boolean = true;

  constructor(
    private newsService: NewsService,
    private likeService: LikeService,
    private followService: FollowService,
    private savedService: SavedService,
    private commentService: CommentService
  ) {}

  ngOnInit(): void {
    this.loadFollowedNews();
  }

  loadFollowedNews(): void {
    this.loading = true;
    this.newsService.getFollowedNews(this.u_id).subscribe({
      next: (data) => {
        this.followedNewsList = data.map(news => ({
          ...news,
          isFollowed: true,
          showComments: false,
          newComment: '',
          successMessage: '',
          hasLiked: false,
          hasUnliked: false,
          hasSaved: false
        }));

        this.followedNewsList.forEach(news => {
          this.likeService.getStatus(news.news_id, this.u_id).subscribe({
            next: (status: { hasLiked: boolean; hasUnliked: boolean }) => {
              news.hasLiked = status.hasLiked;
              news.hasUnliked = status.hasUnliked;
            },
            error: err => console.error('Error getting like status:', err)
          });

          this.savedService.getStatus(news.news_id, this.u_id).subscribe({
            next: (status: { hasSaved: boolean }) => {
              news.hasSaved = status.hasSaved;
            },
            error: err => console.error('Error getting saved status:', err)
          });
        });

        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load followed news:', err);
        this.loading = false;
      }
    });
  }

  toggleComments(news: any): void {
    news.showComments = !news.showComments;
  }

  addComment(news: any): void {
    if (!news.newComment?.trim()) return;

    const commentPayload = {
      news_id: news.news_id,
      u_id: this.u_id,
      comments: news.newComment
    };

    this.commentService.addComment(commentPayload).subscribe({
      next: () => {
        news.successMessage = 'Comment added successfully';
        news.newComment = '';
      },
      error: (err: any) => {
        console.error('Error adding comment:', err);
      }
    });
  }

  selecttile(news: TileData): void {
    this.tileClicked.emit(news);
  }

  like(news: TileData): void {
    this.likeService.like(news.news_id, this.u_id).subscribe({
      next: () => {
        news.likes += 1;
        news.hasLiked = true;
        news.hasUnliked = false;
        if (news.unlikes > 0) news.unlikes -= 1;
      },
      error: err => console.error(err)
    });
  }

  unlike(news: TileData): void {
    this.likeService.unlike(news.news_id, this.u_id).subscribe({
      next: () => {
        news.unlikes += 1;
        news.hasUnliked = true;
        news.hasLiked = false;
        if (news.likes > 0) news.likes -= 1;
      },
      error: err => console.error(err)
    });
  }

  save(news: TileData): void {
    this.savedService.save(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = true;
      },
      error: err => console.error(err)
    });
  }

  toggleFollow(news: TileData): void {
    if (!news.isFollowed) {
      this.followService.followUser(news.u_id, this.u_id).subscribe({
        next: () => {
          news.isFollowed = true;
        },
        error: err => console.error('Failed to follow:', err)
      });
    }
  }
}
