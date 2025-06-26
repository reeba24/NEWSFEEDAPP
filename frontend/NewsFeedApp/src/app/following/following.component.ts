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
    isFollowed?: boolean;
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
          this.likeService.getFullStatus(news.news_id, this.u_id).subscribe({
            next: (res) => {
              news.hasLiked = res.hasLiked;
              news.hasUnliked = res.hasUnliked;
              news.hasSaved = res.hasSaved;
            },
            error: err => console.error('Error getting status:', err)
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
      next: (res: any) => {
        news.successMessage = 'Comment added successfully';
        news.newComment = '';
        news.comments = res.comments;
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
    if (news.hasLiked) return;

    this.likeService.performAction(news.news_id, this.u_id, 'like').subscribe({
      next: () => {
        news.likes += 1;
        if (news.hasUnliked) news.unlikes = Math.max(news.unlikes - 1, 0);
        news.hasLiked = true;
        news.hasUnliked = false;
      },
      error: err => console.error('Like failed:', err)
    });
  }

  unlike(news: TileData): void {
    if (news.hasUnliked) return;

    this.likeService.performAction(news.news_id, this.u_id, 'unlike').subscribe({
      next: () => {
        news.unlikes += 1;
        if (news.hasLiked) news.likes = Math.max(news.likes - 1, 0);
        news.hasLiked = false;
        news.hasUnliked = true;
      },
      error: err => console.error('Unlike failed:', err)
    });
  }

  save(news: TileData): void {
    if (news.hasSaved) return;

    this.savedService.save(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = true;
      },
      error: err => console.error('Save failed:', err)
    });
  }
    unsave(news: TileData): void {
    if (!news.hasSaved) return;

    this.savedService.unsave(news.news_id, this.u_id).subscribe({
      next: () => {
        news.hasSaved = false;
      },
      error: err => console.error('Unsave failed:', err)
    });
  }

  toggleFollow(news: TileData): void {
    this.followService.followUser(news.u_id, this.u_id).subscribe({
      next: (res: any) => {
        news.isFollowed = !news.isFollowed;
        console.log(res.message);
      },
      error: err => console.error('Follow/unfollow failed:', err)
    });
  }
}
