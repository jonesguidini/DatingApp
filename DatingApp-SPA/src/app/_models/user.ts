import { Photo } from './photo';

export interface User {
    id: number;
    username: string;
    knownAs: string;
    age: number;
    gender: string;
    created: Date;
    lastActive: Date;
    photoUrl: string;
    city: string;
    country: string;
    // the properties bellow are optionals because they can be used as UserDetailed situation (reuse of interface)
    interests?: string;
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];
}
