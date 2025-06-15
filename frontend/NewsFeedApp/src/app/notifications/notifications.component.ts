import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { NotificationService } from '../services/notification.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.css']
})
export class NotificationsComponent implements OnInit {
  notifications: any[] = [];
  userId: number = parseInt(localStorage.getItem('userId')!);
  @Output() close = new EventEmitter<void>();

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.notificationService.getNotifications(this.userId).subscribe(data => {
      this.notifications = data;
    });
  }

  markAsRead(notificationId: number): void {
    this.notificationService.markAsRead(notificationId).subscribe(() => {
      this.notifications = this.notifications.filter(n => n.notification_id !== notificationId);
    });
  }

  clearAll(): void {
    this.notificationService.clearAll(this.userId).subscribe(() => {
      this.notifications = [];
    });
  }

}
