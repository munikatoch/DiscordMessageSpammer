# Who's that pokemon

This a self host bot which can predict pokemon spawned by pokemon discord bot

## Features

- Spam messages to spawn pokemon
- Delete messages
- Predict pokemon
- Will only run on one server, if you want to run in 2 servers then run two container docker

## Get Started

To start the bot install [Docker Desktop] and go the the directory you have cloned the project and run command **`docker build -t [Docker Image Name] -f Dockerfile .`**.  This will create an docker image in the docker desktop. Now open docker desktop, go to images and click on the run icon. Now expand the optional settings and enter environment variables:-
1. "DiscordUserAuthToken": (give your discord user token here)
2. "DiscordBotToken": (give you discord bot token here)
3. Open the [constants file] and change the below:
	1. PokemonRarePingRoleId to the rare ping role ID if any else make it 0
	2. PokemonShadowPingRoleId with rare ping role ID if any else make it 0
	3. GuildId with the server ID where you want the bot
	4. BotLatencyChannel, BotGuildJoinChannel, BotShardConnectedChannel, BotShardDisconnectedChannel these are optional if you want to see the logs. Create a channel for the logs and change them with that channel ID

   [constants file]: <https://github.com/munikatoch/WhosThatPokemonBot/blob/master/Models/Constants.cs>
   [Docker Desktop]: <https://www.docker.com/products/docker-desktop/>