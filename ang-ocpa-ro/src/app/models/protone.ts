export enum BuildType {
    Legacy,
    Experimental,
    Release,
}

export interface Version {
    major: number;
    minor: number;
    build: number;
    revision: number;
    majorRevision: number;
    minorRevision: number;
}

export interface BuildInfo {
    title: string;
    version: Version;
    buildDate: string;
    isRelease: boolean;
    comment: string;
    url: string;
}