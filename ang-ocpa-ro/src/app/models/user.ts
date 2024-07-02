export enum UserType {
    ApiUser = 0,
    Admin = 1,
    Patient = 2
}
export interface User {
    id: number;
    loginId: string;
    passwordHash: string;
    userType: UserType;
}
export interface AuthenticationResponse {
    loginId: string;
    token?: string;
    type: UserType;
    validity: number;
}