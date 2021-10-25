﻿# Callouts


## TODO: Feature

### Major

* !raid command
* DM Reports: Allow a user to request a raid report in a private channel. This worked in my old python version
* Redo Create Event Page: make it pretty / add Event RSVP
* Fix Roles: I need to figure out more of how this works before turning it on.
* Login is not viewable on mobile
* reduce api calls. I think there are a few times where it gets throttled

### Minor

* Web Login: Only complete web login if they are in a clan that they bot is in
* callouts.db needs a permanent location (Startup.cs)
* !restart command (Core.cs)
* !about command (Core.cs)
* Remove old accept buttons from DM event reminders (SendReminderCallback)
* Rethink !stats command. It could probably be simplified and thus more extensible (Stats.cs)
* Robust !help commands throughout
* Clean channel for bot-commands and periodic task for it
* add url button to the clan website in RequireBungieLink.cs
* usernames show up multiple times when tagged in a raid report if they swapped characters
* Don't delete the last raid report even if it is old because it has the get report button

## TODO: Bugs

* Events Exception: Events Created by removed users while bot is offline causes it to crash
* !stats command called without subcommand (Stats.cs)

## TODO: QOL

* Documentation/Comments throughout
* proper build/deploy steps to create/update DB and all that jazz
* Logging: Add more debug logging in lots of places. It is currently commented out
* Add text to Index Page
* Rethink stats mapper class. It seems messy (BungieService.cs) Automapper?
* Clean up BungieService unused functions and think if it can be done simpler. Extentions?

## Possible TODOs

* There might be an issue linking bungie accounts if a user has multiple platforms but does not use cross-save (GetPrimaryDestinyAccountFromUniqueName)
* Callouts.Extentions DateTime extentions could probably combine these by passing in current and destination timezone or something
* Break up EventManager into EventManager and UserEventManager
* Manager base class to make adding singletons at startup simpler
* Clean up startup ConfigureServices
* Clean up / Simplify EventManager ComponentInteractionCreatedCallback (if it is not split)
* More game state like Rumble, doubles, scorched, mayhem
* Weird issue where some people don't have pvp stats (trials mainly)
* Rethink how required channels are registered with channel manager
* Rethink message manager and use it more?
* Consider restricting event attendees positive or null
* Add ability to change server prefix (Guild.cs and probably Core.cs)
* Add ability to set  event create / delete roles (Guild.cs and probably Core.cs)
* Find 2 more stats to fill out last row of pvp stats embed
* Rename Bungie web page to register or something
* Consider changing the unmapped `Accepted`, `Standby`, `Declined`, `Maybe` in Event.cs to be 1 get function
* Could simplify event created if UserEvents were sored by last update and then just iterated over once
* EventManager.cs CreateEventReminderMessage the button doesn't need attempt number
* Remove event reminders if all people are confirmed (SchedulingService)
* Remove reminders for deleted events (SchedulingService)
* 2 second sleep in ReportManager.GetRaidReportFromWeb.

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