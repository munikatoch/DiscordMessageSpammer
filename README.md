# Who's that pokemon

This a self host bot which can predict pokemon spawned by pokemon discord bot

## Features

- Spam messages to spawn pokemon
- Delete messages
- Predict pokemon

To start the bot you will need to have a dataset of pokemon images in the Assets input folder or if you have a trained
dataset then add it in the Assets output folder. For schema of the model please see the [Model input] and [Model output] file.
Create a new bot from the discord developers site. Copy the bot token and add it in "BotToken" in appsettings.json file. Now go to https://discord.com/app and send message to anyone and copy the user auth token from the network call in browser and add it in "DiscordUserAuthToken" in appsettings.json. In [constants file] replace the discord ID with the one you want.

```json
{
  "appSettings": {
    "DiscordUserAuthToken": "Discord User Token From the browser",
    "BotToken": "Bot token from the discord developer site",
    "TrainModelAgain": false,
    "RemoveFilesAndTrainModelAgain": false
  }
}
```

   [Model Input]: <https://github.com/munikatoch/WhosThatPokemonBot/blob/master/Models/MlModelTrainer/ImageData.cs>
   [Model Output]: <https://github.com/munikatoch/WhosThatPokemonBot/blob/master/Models/MlModelTrainer/ModelOutput.cs>
   [constants file]: <https://github.com/munikatoch/WhosThatPokemonBot/blob/master/Models/Constants.cs>