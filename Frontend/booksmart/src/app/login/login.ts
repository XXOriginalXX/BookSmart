import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';   
import { CommonModule } from '@angular/common'; 
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,                            
  imports: [FormsModule, CommonModule],       
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {

  email: string = '';
  password: string = '';
  errorMessage: string = '';
  showPassword: boolean = false;
  isLoading: boolean = false;

  //constructor(public router: Router) {}
  constructor(public router: Router, private http: HttpClient) {}
  

  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
  this.errorMessage = '';

  if (!this.email || !this.password) {
    this.errorMessage = 'Please fill in all fields.';
    return;
  }

  this.isLoading = true;

  this.http.post<any>('http://localhost:5232/api/auth/login', {
    email: this.email,
    password: this.password
  }).subscribe({
    next: (res) => {
  this.isLoading = false;

  //alert('Login successful 🎉');

  this.router.navigate(['/booking']);
},
    error: (err) => {
      this.isLoading = false;
      this.errorMessage = err.error?.message || 'Login failed';
    }
  });
}
}