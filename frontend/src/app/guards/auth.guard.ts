import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserStateService } from '../services/user-state.service';

/**
 * Blocks protected routes for unauthenticated users, sending them to the tours
 * landing page where the header exposes the login/register panel.
 */
export const authGuard: CanActivateFn = () => {
  const userState = inject(UserStateService);
  const router = inject(Router);

  if (userState.isAuthenticated$()) {
    return true;
  }

  router.navigate(['/tours']);
  return false;
};
