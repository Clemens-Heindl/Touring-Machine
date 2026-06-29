import { Component, inject, Input } from '@angular/core';
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
export class LoginComponent {
  @Input() mode: 'login' | 'register' = 'login';

  private authService = inject(AuthService);
  private userState = inject(UserStateService);

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

    if (!this.email.trim()) {
      this.userState.setError('Enter your email.');
      return;
    }

    if (!this.password.trim()) {
      this.userState.setError('Enter your password.');
      return;
    }

    this.isLoading = true;
    this.authService.login(this.email.trim(), this.password).subscribe({
      next: user => {
        this.isLoading = false;
        if (user) {
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
