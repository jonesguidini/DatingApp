import { Injectable } from '@angular/core';
import { CanActivate, UrlTree, Router } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Injectable({
  providedIn: 'root'
})

// esse GUARD é usada para controlar acesso as páginas páginas que necessitam de um usuário logado

export class AuthGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router, private alertify: AlertifyService) {}

  canActivate(): boolean {

    if (this.authService.loggedIn()) {
      return true;
    }

    this.alertify.error('You shall not pass!!!');
    this.router.navigate(['/home']);
    return false;
  }
}
