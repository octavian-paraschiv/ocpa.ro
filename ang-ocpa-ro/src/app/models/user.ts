export interface User {
    id?: number;
    loginId?: string | undefined;
    passwordHash?: string | undefined;
    type?: number;
}

export interface UserType {
    id?: number;
    code?: string | undefined;
    description?: string | undefined;
}

export interface AuthenticateResponse {
    loginId: string;
    token: string;
    type: number;
    expires: Date;
    validity: number;
}