# Callouts

## Big TODOs

* If you cancel on the discord login page an exception is raised.
* Only consider people authenticated if they are in a server that the bot is in
* Think about simplifying users to only have the primary account. Who doesn't have that these days?

## appsettings.json Example

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "Discord": {
    "ClientId": "",
    "ClientSecret": "",
    "BotToken": "",
    "prefix": ""
  },
  "Bungie": {
    "ApiKey": "",
    "ClientId": "",
    "ClientSecret": "",
    "RateLimit":  ""
  }
}
```