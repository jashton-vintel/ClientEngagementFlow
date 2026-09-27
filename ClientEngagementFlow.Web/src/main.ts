import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideBrowserGlobalErrorListeners } from '@angular/core';

import {
  BrowserCacheLocation,
  IPublicClientApplication,
  PublicClientApplication
} from '@azure/msal-browser';

import {
  MSAL_INSTANCE,
  MsalService
} from '@azure/msal-angular';

import { App } from './app/app';

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: 'fa5c8985-7029-48d5-908c-7dfdcef25542',
      authority: 'https://login.microsoftonline.com/a205873a-af56-439f-a39d-e53d68e2fe4d',
      redirectUri: 'http://localhost:49860'
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage
    }
  });
}

bootstrapApplication(App, {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),

    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory
    },

    MsalService
  ]
})
.catch(err => console.error(err));
