import { AppComponent } from 'src/app/components/app.component';
import { AppDialogComponent } from 'src/app/components/auth/apps-menus/app-dialog/app-dialog.component';
import { AppsMenusComponent } from 'src/app/components/auth/apps-menus/apps-menus.component';
import { MenuDialogComponent } from 'src/app/components/auth/apps-menus/menu-dialog/menu-dialog.component';
import { ContentBrowserComponent } from 'src/app/components/auth/content-browser/content-browser.component';
import { CityDialogComponent } from 'src/app/components/auth/geography/city-dialog/city-dialog.component';
import { GeographyComponent } from 'src/app/components/auth/geography/geography.component';
import { LoginComponent } from 'src/app/components/auth/login/login.component';
import { LogoutComponent } from 'src/app/components/auth/logout/logout.component';
import { MeteoDatabaseDialogComponent } from 'src/app/components/auth/meteo-database/meteo-database-dialog/meteo-database-dialog.component';
import { MeteoDatabaseComponent } from 'src/app/components/auth/meteo-database/meteo-database.component';
import { OtpComponent } from 'src/app/components/auth/otp/otp.component';
import { DevicesDialogComponent } from 'src/app/components/auth/users/devices-dialog/devices-dialog.component';
import { UserDialogComponent } from 'src/app/components/auth/users/user-dialog/user-dialog.component';
import { UsersComponent } from 'src/app/components/auth/users/users.component';
import { DayRisksComponent } from 'src/app/components/non-auth/meteo/day-risks/day-risks.component';
import { MeteoDataBrowserComponent } from 'src/app/components/non-auth/meteo/meteo-data-browser/meteo-data-browser.component';
import { MeteoComponent } from 'src/app/components/non-auth/meteo/meteo.component';
import { ProTONEComponent } from 'src/app/components/non-auth/protone/protone.component';
import { UnavailablePageComponent } from 'src/app/components/non-auth/unavailable-page/unavailable-page.component';
import { ContentTreeComponent } from 'src/app/components/shared/content-tree/content-tree.component';
import { MessageBoxComponent } from 'src/app/components/shared/message-box/message-box.component';
import { NavMenuComponent } from 'src/app/components/shared/nav-menu/nav-menu.component';
import { OverlayComponent } from 'src/app/components/shared/overlay/overlay.component';
import { WikiContainerComponent } from 'src/app/components/shared/wiki-container/wiki-container.component';
import { WikiViewerComponent } from 'src/app/components/shared/wiki-viewer/wiki-viewer.component';

export const components = [
    // Top level
    AppComponent,
    NavMenuComponent,       
    OverlayComponent, 

    // Shared components
    MessageBoxComponent,
    UnavailablePageComponent,

    // Protone player
    ProTONEComponent,

    // Authentication
    LoginComponent,
    LogoutComponent,
    OtpComponent,

    // Apps / menus
    AppsMenusComponent,
    AppDialogComponent,
    MenuDialogComponent,

    // Users management
    UsersComponent,
    UserDialogComponent,
    DevicesDialogComponent,

    // Meteo (public / mangement)
    MeteoDataBrowserComponent,
    DayRisksComponent,
    MeteoComponent,
    MeteoDatabaseComponent,
    MeteoDatabaseDialogComponent,

    // Wiki pages (public)
    WikiViewerComponent,
    WikiContainerComponent,

    // Content management
    ContentBrowserComponent,
    ContentTreeComponent,

    // Geography
    GeographyComponent,
    CityDialogComponent
];