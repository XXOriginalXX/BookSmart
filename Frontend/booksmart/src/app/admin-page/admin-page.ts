import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-admin-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-page.html',
  styleUrl: './admin-page.css'
})
export class AdminPage implements OnInit {

  fullName: string = '';
  activeSection: string = 'dashboard';

  stats = [
    { icon: '📅', label: 'Total Appointments', value: '0' },
    { icon: '👨‍⚕️', label: 'Doctors', value: '0' },
    { icon: '👥', label: 'Patients', value: '0' },
    { icon: '⚠️', label: 'No-Show Risk', value: '0' },
  ];

  appointments: any[] = [];
  doctors: any[] = [];
  patients: any[] = [];

  constructor(public router: Router, private http: HttpClient) {}

  ngOnInit(): void {
    const role = localStorage.getItem('role');
    if (role !== 'Admin') {
      this.router.navigate(['/login']);
      return;
    }

    this.fullName = localStorage.getItem('fullName') || 'Admin';
    this.loadDashboard();
  }

  private headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${localStorage.getItem('token')}` });
  }

  loadDashboard(): void {
    this.http.get<any[]>('http://localhost:5232/api/admin/appointments', { headers: this.headers() })
      .subscribe(data => {
        this.appointments = data;
        this.stats[0].value = data.length.toString();
        const noShowRisk = data.filter(a => a.status === 'Pending').length;
        this.stats[3].value = noShowRisk.toString();
      });

    this.http.get<any[]>('http://localhost:5232/api/doctors', { headers: this.headers() })
      .subscribe(data => {
        this.doctors = data;
        this.stats[1].value = data.length.toString();
      });

    this.http.get<any[]>('http://localhost:5232/api/admin/patients', { headers: this.headers() })
      .subscribe(data => {
        this.patients = data;
        this.stats[2].value = data.length.toString();
      });
  }

  setSection(section: string): void {
    this.activeSection = section;
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Confirmed': return 'status-confirmed';
      case 'Pending':   return 'status-pending';
      case 'Cancelled': return 'status-noshow';
      case 'Completed': return 'status-confirmed';
      default:          return '';
    }
  }

  signOut(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}