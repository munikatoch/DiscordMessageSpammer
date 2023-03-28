# Who's that pokemon

This a self host bot which can predict pokemon spawned by pokemon discord bot

## Features

- Spam messages to spawn pokemon
- Delete messages
- Predict pokemon
- Will only run on one server, if you want to run in 2 servers then run to docker

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

## Get Started

To start the bot install [Docker Desktop] and go the the directory you have cloned the project and run command **`docker build .`**.  This will create an docker image in the docker desktop. Now open docker desktop, go to images and click on the run icon. Now expand the optional settings and enter environment variables:-
1. "DiscordUserAuthToken": (give your discord user token here)
2. "DiscordBotToken": (give you discord bot token here)
3. Open the [constants file] and change the below:
	1. PokemonRarePingRoleId to the rare ping role ID if any else make it 0
	2. PokemonShadowPingRoleId with rare ping role ID if any else make it 0
	3. GuildId with the server ID where you want the bot
	4. BotLatencyChannel, BotGuildJoinChannel, BotShardConnectedChannel, BotShardDisconnectedChannel these are optional if you want to see the logs. Create a channel for the logs and change them with that channel ID

   [Model Input]: <https://github.com/munikatoch/WhosThatPokemonBot/blob/master/Models/MlModelTrainer/ImageData.cs>
   [Model Output]: <https://github.com/munikatoch/WhosThatPokemonBot/blob/master/Models/MlModelTrainer/ModelOutput.cs>
   [constants file]: <https://github.com/munikatoch/WhosThatPokemonBot/blob/master/Models/Constants.cs>
   [Docker Desktop]: <https://www.docker.com/products/docker-desktop/>