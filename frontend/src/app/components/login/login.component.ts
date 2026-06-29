import { Component, inject, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { UserStateService } from '../../services/user-state.service';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  @Input() mode: 'login' | 'register' = 'login';

  private authService = inject(AuthService);
  private userState = inject(UserStateService);

  users: User[] = [];
  name = '';
  email = '';
  password = '';
  isLoading = false;

  get errorMessage() {
    return this.userState.errorMessage$();
  }

  get isRegisterMode() {
    return this.mode === 'register';
  }

  ngOnInit(): void {
    this.authService.getUsers().subscribe({
      next: users => {
        this.users = users;
      },
      error: () => {
        this.userState.setError('Unable to load users from the API.');
      }
    });
  }

  submit() {
    this.userState.clearError();

    if (this.isRegisterMode) {
      if (!this.name.trim()) {
        this.userState.setError('Enter your name.');
        return;
      }

      if (!this.email.trim()) {
        this.userState.setError('Enter your email.');
        return;
      }

      if (!this.password.trim()) {
        this.userState.setError('Enter your password.');
        return;
      }

      this.isLoading = true;
      const newUser: User = {
        id: 0,
        name: this.name.trim(),
        email: this.email.trim(),
        passwordHash: this.password
      };

      this.authService.register(newUser).subscribe({
        next: createdUser => {
          this.isLoading = false;
          this.users.push(createdUser);
          this.userState.setCurrentUser(createdUser);
          this.name = '';
          this.email = '';
          this.password = '';
        },
        error: () => {
          this.isLoading = false;
          this.userState.setError('Registration failed.');
        }
      });

      return;
    }

    const user = this.users.find(u => u.email.toLowerCase() === this.email.trim().toLowerCase());
    if (!user) {
      this.userState.setError('Unknown user email.');
      return;
    }

    if (!this.password.trim()) {
      this.userState.setError('Enter your password.');
      return;
    }

    this.isLoading = true;
    this.authService.login(user.email, this.password).subscribe({
      next: isValid => {
        this.isLoading = false;
        if (isValid) {
          this.userState.setCurrentUser(user);
        } else {
          this.userState.setError('Invalid credentials.');
        }
      },
      error: () => {
        this.isLoading = false;
        this.userState.setError('Login request failed.');
      }
    });
  }
}
