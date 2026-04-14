import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './booking.html',
  styleUrls: ['./booking.css']
})
export class BookingComponent {

  doctorName: string = '';
  specialization: string = '';
  appointmentDate: string = '';
  appointmentTime: string = '';
  notes: string = '';
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  specializations: string[] = [
    'General Practice',
    'Cardiology',
    'Dermatology',
    'Neurology',
    'Orthopedics',
    'Pediatrics',
    'Psychiatry'
  ];

  timeSlots: string[] = [
    '09:00 AM', '09:30 AM', '10:00 AM', '10:30 AM',
    '11:00 AM', '11:30 AM', '02:00 PM', '02:30 PM',
    '03:00 PM', '03:30 PM', '04:00 PM', '04:30 PM'
  ];

  constructor(public router: Router, private http: HttpClient) {}

  get minDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.doctorName || !this.specialization || !this.appointmentDate || !this.appointmentTime) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isLoading = true;

    const payload = {
      doctorName: this.doctorName,
      specialization: this.specialization,
      appointmentDate: this.appointmentDate,
      appointmentTime: this.appointmentTime,
      notes: this.notes
    };

    this.http.post<any>('http://localhost:5232/api/appointments', payload).subscribe({
      next: () => {
        this.isLoading = false;
        this.successMessage = 'Appointment booked successfully!';
        this.resetForm();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Booking failed. Please try again.';
      }
    });
  }

  resetForm(): void {
    this.doctorName = '';
    this.specialization = '';
    this.appointmentDate = '';
    this.appointmentTime = '';
    this.notes = '';
  }
}