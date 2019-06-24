export interface Message {
    id: number;
    senderId: number;
    senderKnownAs: string;
    senderPhotoURL: string;
    recipientId: number;
    recipientKnownAs: string;
    recipíentPhotoURL: string;
    content: string;
    isRead: boolean;
    dateRead: Date;
    messageSent: Date;
}
