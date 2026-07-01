export interface User {
  id: number;
  name: string;
  email: string;
  // Only present on objects built client-side before submission; the API never
  // returns it. Passwords are hashed server-side (BCrypt).
  passwordHash?: string;
}
