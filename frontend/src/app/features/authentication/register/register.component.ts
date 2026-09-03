import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly registerForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]],
    acceptedTerms: [false, [Validators.requiredTrue]]
  }, {
    validators: this.passwordsMatchValidator
  });

  passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirm = control.get('confirmPassword')?.value;
    if (password && confirm && password !== confirm) {
      return { passwordsMismatch: true };
    }
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const formValues = this.registerForm.getRawValue();

    this.auth.register({
      firstName: formValues.firstName,
      lastName: formValues.lastName,
      email: formValues.email,
      phoneNumber: formValues.phoneNumber || undefined,
      password: formValues.password,
      confirmPassword: formValues.confirmPassword,
      acceptedTerms: formValues.acceptedTerms
    }).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.success) {
          this.router.navigate(['/']);
        } else {
          this.errorMessage.set(res.message || 'Registration failed. Please check your details.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.message || 'Unable to register account. Please try again.');
      }
    });
  }
}
