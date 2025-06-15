import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-newpost',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './newpost.component.html',
  styleUrls: ['./newpost.component.css']
})
export class NewpostComponent {
  title: string = '';
  contents: string = '';
  pref_name: string = '';
  message: string = '';
  selectedFile: File | null = null;  
  prefOptions: string[] = [
    'Technology','Artificial Intelligence','Virtual Reality','Data Science','Programming',
    'Cloud Computing','Internet of Things','Big Data','Machine Learning','Cybersecurity',
    'Sports','Politics','Health','Business','Entertainment'
  ];

  constructor(private http: HttpClient) {}

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  createPost() {
    const userId = localStorage.getItem('userId');
    if (!userId) {
      this.message = "User not logged in.";
      return;
    }

    const formData = new FormData();
    formData.append('news_title', this.title.trim());
    formData.append('contents', this.contents.trim());
    formData.append('pref_name', this.pref_name.trim());
    formData.append('u_id', userId);
    formData.append('active', '1');

    if (this.selectedFile) {
      formData.append('image', this.selectedFile);   
    }

    this.http.post('https://localhost:7077/api/NewPost', formData).subscribe({
      next: (res: any) => {
        this.message = 'Post created successfully!';
        
        this.title = '';
        this.contents = '';
        this.pref_name = '';
        this.selectedFile = null;
      },
      error: (err) => {
        console.error(err);
        if (err.status === 400 && err.error?.message) {
          this.message = err.error.message;
        } else {
          this.message = 'Failed to create post.';
        }
      }
    });
  }
}
