using Microsoft.Identity.Client;

var tenantId = "a205873a-af56-439f-a39d-e53d68e2fe4d";
var clientId = "85ca79ea-a165-495d-a742-e1759af5fe83";
var apiClientId = "33043fba-9677-43cc-bd5b-c0173f5a1fef";

var scopes = new[]
{
    $"api://{apiClientId}/jobs.submit"
};

var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
    .Build();

var result = await app
    .AcquireTokenWithDeviceCode(
        scopes,
        deviceCode =>
        {
            Console.WriteLine(deviceCode.Message);
            return Task.CompletedTask;
        })
    .ExecuteAsync();

Console.WriteLine();
Console.WriteLine("Access token:");
Console.WriteLine(result.AccessToken);
Console.ReadLine();