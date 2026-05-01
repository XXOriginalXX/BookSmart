import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {

  totalAppointments: number = 0;
  todayAppointments: number = 0;
  noShowRate: number = 0;
  highDemandSlot: string = '';

  recentAppointments: any[] = [];

  stats = [
    { label: 'Total Appointments', value: '0', icon: '📅', key: 'total' },
    { label: "Today's Appointments", value: '0', icon: '🕐', key: 'today' },
    { label: 'No-Show Rate', value: '0%', icon: '⚠️', key: 'noshow' },
    { label: 'High Demand Slot', value: '—', icon: '🔥', key: 'slot' }
  ];

  constructor(public router: Router, private http: HttpClient) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.recentAppointments = [
      { patient: 'Sarah Johnson', doctor: 'Dr. Ahmed', time: '09:00 AM', date: '2024-06-10', status: 'Confirmed' },
      { patient: 'Mark Williams', doctor: 'Dr. Patel', time: '10:30 AM', date: '2024-06-10', status: 'Pending' },
      { patient: 'Aisha Rauf', doctor: 'Dr. Chen', time: '11:00 AM', date: '2024-06-10', status: 'No Show' },
      { patient: 'Tom Brady', doctor: 'Dr. Ahmed', time: '02:00 PM', date: '2024-06-10', status: 'Confirmed' },
      { patient: 'Priya Nair', doctor: 'Dr. Patel', time: '03:30 PM', date: '2024-06-10', status: 'Confirmed' }
    ];

    this.stats[0].value = '124';
    this.stats[1].value = '18';
    this.stats[2].value = '20.2%';
    this.stats[3].value = '10:00 AM';
  }

  getStatusClass(status: string): string {
    if (status === 'Confirmed') return 'status-confirmed';
    if (status === 'Pending') return 'status-pending';
    if (status === 'No Show') return 'status-noshow';
    return '';
  }

  signOut(): void {
    this.router.navigate(['/login']);
  }
}