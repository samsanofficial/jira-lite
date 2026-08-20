export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserDto {
  id: number;
  fullName: string;
  email: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserDto;
}
