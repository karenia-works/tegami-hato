export const apiConfig = {
  endpoints: {
    account: {
      verification: '/api/user/code/request',
      login: '/connect/token',
      info: {
        current: '/user/me',
        any: '/uer/{id}',
      },
    },
    channel: {
      create: '/api/channel',
      join: '/api/channel/{id}/join',
      info: '/api/channel/{id}',
      message: {
        send: '/api/channel/{id}/message',
        get: {
          single: '/api/channel/{id}/message',
          many: '/api/channel/many/message'
        }
      },
      invitation: {
        create: '/api/channel/{id}/invitation/create',
        get: '/api/channel/{id}/invitation',
        info: '/api/channel/{id}/invitation/{invitationId}',
        channel: '/api/invitation/{id}',
        accept: '/api/invitation/{id}'
      }
    },
    attachment: {
      upload: '/api/attachment/upload'
    }
  },
};
