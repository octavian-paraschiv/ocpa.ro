import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, ResolveFn, Route, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { AppsMenusComponent } from 'src/app/components/auth/apps-menus/apps-menus.component';
import { LoginComponent } from 'src/app/components/auth/login/login.component';
import { LogoutComponent } from 'src/app/components/auth/logout/logout.component';
import { MeteoDatabaseComponent } from 'src/app/components/auth/meteo-database/meteo-database.component';
import { OtpComponent } from 'src/app/components/auth/otp/otp.component';
import { UsersComponent } from 'src/app/components/auth/users/users.component';
import { MeteoComponent } from 'src/app/components/non-auth/meteo/meteo.component';
import { ProTONEComponent } from 'src/app/components/non-auth/protone/protone.component';
import { UnavailablePageComponent, UnavailablePageKind } from 'src/app/components/non-auth/unavailable-page/unavailable-page.component';
import { WikiContainerComponent } from 'src/app/components/shared/wiki-container/wiki-container.component';
import { AuthenticationService } from 'src/app/services/api/authentication.services';
import { MenuService, UrlKind } from 'src/app/services/api/menu.service';

export function translateTitle(route: ActivatedRouteSnapshot, translate: TranslateService): string {
  let rawTitle = 'unavailable';
  
  const url = route.url;
  const path = route.routeConfig?.path;
  if (url?.length > 0 && path !== '**')
    rawTitle = url.map(s => s.path).join('.');

  return translate.instant(`title.${rawTitle}`);
};

const titleResolver: ResolveFn<string> = (route, _) => translateTitle(route, inject(TranslateService));

const otpGuard: CanActivateFn = (_, __) => {
  const authService = inject(AuthenticationService);
  const router = inject(Router);
  const activate = authService.shouldSendOtp$.getValue() && !authService.isUserLoggedIn();
  if (!activate) {
    router.navigate(['/meteo']);
  }
  return activate;
}

const authGuard: CanActivateFn = (_, state) => {
    const authService = inject(AuthenticationService);
    const menuService = inject(MenuService);
    const router = inject(Router);
    const url = (state?.url ?? '/').toLowerCase();
  
    if (url !== '/' && !url.startsWith('/wiki-container')) {
      const matchingRoutes = routes.filter(r => `/${r.path.toLowerCase()}` === url);
      if (!(matchingRoutes?.length > 0)) {
        router.navigate(['/unavailable'], 
          { queryParams: { kind: UnavailablePageKind.Unauthorized, url }});
        return false;
      }
    }
  
    const urlKind = menuService.getUrlKind(state.url);
  
    switch(urlKind) {
      case UrlKind.Unavailable:
        router.navigate(['/unavailable'], 
          { queryParams: { kind: UnavailablePageKind.Unauthorized, url }});
        return false;
  
      case UrlKind.App:
        if (!authService.isUserLoggedIn()) {
          router.navigate(['/login']);
          return false;
        }
        return true;
    }
  
    return true;
  };

export const routes = [
    { path: 'meteo', component: MeteoComponent, title: titleResolver } as Route,
    { path: 'protone', component: ProTONEComponent, title: titleResolver } as Route,    
    { path: 'login', component: LoginComponent, title: titleResolver } as Route,    
    { path: 'otp', component: OtpComponent, canActivate: [otpGuard], title: titleResolver } as Route,    
    { path: 'logout', component: LogoutComponent, title: titleResolver } as Route,    
    { path: 'unavailable-page', component: UnavailablePageComponent, title: titleResolver } as Route,
  
    { path: 'admin/users', component: UsersComponent, canActivate: [authGuard], title: titleResolver } as Route,
    { path: 'admin/meteo-database', component: MeteoDatabaseComponent, canActivate: [authGuard], title: titleResolver } as Route,
    { path: 'admin/apps-menus', component: AppsMenusComponent, canActivate: [authGuard], title: titleResolver } as Route,
  
    { path: 'wiki-container/:a', component: WikiContainerComponent, canActivate: [authGuard], title: titleResolver } as Route,
    { path: 'wiki-container/:a/:b', component: WikiContainerComponent, canActivate: [authGuard], title: titleResolver } as Route,
    { path: 'wiki-container/:a/:b/:c', component: WikiContainerComponent, canActivate: [authGuard], title: titleResolver } as Route,
    { path: 'wiki-container/:a/:b/:c/:d', component: WikiContainerComponent, canActivate: [authGuard], title: titleResolver } as Route,
    { path: 'wiki-container/:a/:b/:c/:d/:e', component: WikiContainerComponent, canActivate: [authGuard], title: titleResolver } as Route,
    { path: 'wiki-container/:a/:b/:c/:d/:e/:f', component: WikiContainerComponent, canActivate: [authGuard], title: titleResolver } as Route,
  
    { path: '', redirectTo: '/meteo', pathMatch: 'full' } as Route,

    { path: '**', component: UnavailablePageComponent, title: titleResolver } as Route,
  ];