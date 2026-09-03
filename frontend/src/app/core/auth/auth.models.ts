export interface LoginDto {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterDto {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  password: string;
  confirmPassword: string;
  acceptedTerms: boolean;
}

export interface ForgotPasswordDto {
  email: string;
}

export interface ResetPasswordDto {
  email: string;
  token: string;
  newPassword: string;
}
