export interface HatoAttachment {
  attachmentId: string;
  filename: string;
  url: string;
  contentType: string;
  size: number;
  isAvailable: boolean;
}

export interface RecentMessageViewItem{
  channelId: string;
  channelTitle: string;
  msgId?: string;
  bodyPlain?: string;
  title?: string;
  senderEmail?: string;
  senderNickname?: string;
}

export interface HatoMessage {
  msgId: string;
  channelId: string;
  // 仅在最近消息中使用
  _Channel?: HatoChannel;
  timestamp: Date;
  senderEmail: string;
  senderNickname?: string;
  title?: string;
  bodyPlain: string;
  bodyHtml?: string;
  attachments: HatoAttachment[];
  tags: string[];
}

export interface HatoChannel {
  channelId: string;
  channelUsername: string;
  channelTitle: string;
  isPublic: boolean;
  defaultPermission: number;
}

export interface InvitationLink {
  channelId: string;
  linkId: string;
  defaultPermission: number;
  expires: Date;
}

export interface SendMessage {
  title?: string;
  bodyPlain: string;
  bodyHtml?: string;
  attachments: string[];
  tags: string[];
}

export interface SendMessageResult {
  msgId: string;
  timestamp: Date;
  failedChannels: {
    channel: string;
    reason: string
  };
}
