import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-editprofile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './editprofile.component.html',
  styleUrl: './editprofile.component.css'
})
export class EditprofileComponent implements OnInit {

  @Output() backClicked = new EventEmitter<void>();

  firstName = '';
  lastName = '';
  about = '';
  userId: number = 0;
  successmsg='';

  prefOptions: string[] = [
    'Technology','Artificial Intelligence','Virtual Reality','Data Science','Programming',
    'Cloud Computing','Internet of Things','Big Data','Machine Learning','Cybersecurity',
    'Sports','Politics','Health','Business','Entertainment'
  ];

  selectedPreferences: number[] = [];
  unselectedPreferences: number[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    const storedUserId = localStorage.getItem('userId');
    if (storedUserId) {
      this.userId = parseInt(storedUserId, 10);
      this.loadProfile();
    } else {
      alert('User ID not found. Please login again.');
    }
  }

  loadProfile() {
    this.http.get<any>(`https://localhost:7077/api/EditProfile/GetProfile/${this.userId}`).subscribe({
      next: (data) => {
        this.firstName = data.firstName;
        this.lastName = data.lastName;
        this.about = data.about;
        this.selectedPreferences = data.preferenceIds;
        this.calculateUnselectedPreferences();
      },
      error: (err) => {
        console.error(err);
        alert('Error loading profile data');
      }
    });
  }

  calculateUnselectedPreferences() {
    this.unselectedPreferences = [];
    for (let i = 1; i <= this.prefOptions.length; i++) {
      if (!this.selectedPreferences.includes(i)) {
        this.unselectedPreferences.push(i);
      }
    }
  }

  togglePreference(prefId: number) {
    if (this.selectedPreferences.includes(prefId)) {
      this.selectedPreferences = this.selectedPreferences.filter(id => id !== prefId);
      this.unselectedPreferences.push(prefId);
    } else {
      this.selectedPreferences.push(prefId);
      this.unselectedPreferences = this.unselectedPreferences.filter(id => id !== prefId);
    }
  }

  saveProfile() {
    const payload = {
      userId: this.userId,
      firstName: this.firstName,
      lastName: this.lastName,
      about: this.about,
      preferenceIds: this.selectedPreferences
    };

    this.http.post('https://localhost:7077/api/EditProfile/Update', payload).subscribe({
      next: () => {
        this.successmsg="Profile Updated Successfully";
      },
      error: (err) => {
        console.error(err);
        this.successmsg="Error";
      }
    });
  }

  cancel(): void {
    this.backClicked.emit();
  }
}
