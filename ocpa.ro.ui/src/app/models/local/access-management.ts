import { Application, User, ApplicationUser } from 'src/app/models/swagger/access-management';


export interface AppMenuData {
    menuId: number;
    menuName: string;
    appData: AppData[];
}

export interface AppData {
    appName: string;
    appId: number;
    active: boolean;
}

export interface ApplicationInfo extends Application {
    selected: boolean;
}

export interface UserInfo extends User {
    appsForUser: ApplicationUser[];
}

export interface UserSessionInformation {
    loginId: string;
    token: string;
    anonymizedEmail: string;
    tokenExpiration: Date;
}