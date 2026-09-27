import { bootstrapApplication } from '@angular/platform-browser';
import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withInterceptorsFromDi
} from '@angular/common/http';
import { provideBrowserGlobalErrorListeners } from '@angular/core';

import {
  BrowserCacheLocation,
  InteractionType,
  IPublicClientApplication,
  PublicClientApplication
} from '@azure/msal-browser';

import {
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService,
  MsalInterceptor,
  MsalInterceptorConfiguration,
  MsalService
} from '@azure/msal-angular';

import { App } from './app/app';

const apiScope = 'api://33043fba-9677-43cc-bd5b-c0173f5a1fef/jobs.submit';


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


export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {

  const protectedResourceMap = new Map<string, Array<string>>();

  protectedResourceMap.set(
    'https://localhost:7221/api/jobs',
    [apiScope]
  );

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap
  };
}


bootstrapApplication(App, {
  providers: [
    provideBrowserGlobalErrorListeners(),

    provideHttpClient(
      withInterceptorsFromDi()
    ),

    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory
    },

    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: MSALInterceptorConfigFactory
    },

    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },

    MsalService,
    MsalBroadcastService
  ]
})
.catch(err => console.error(err));
