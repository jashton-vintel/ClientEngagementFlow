export const environment = {
  apiBaseUrl: 'https://localhost:7221',
  signalRHubUrl: 'https://localhost:7221/hubs/processing-jobs',
  entra: {
    tenantId: 'a205873a-af56-439f-a39d-e53d68e2fe4d',
    clientId: 'fa5c8985-7029-48d5-908c-7dfdcef25542',
    apiClientId: '33043fba-9677-43cc-bd5b-c0173f5a1fef',
    redirectUri: 'http://localhost:49860'
  },
  apiScope: 'api://33043fba-9677-43cc-bd5b-c0173f5a1fef/jobs.submit'
};
