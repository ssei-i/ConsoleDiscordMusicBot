using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using System.Text;
using MusicBot.config;

namespace MusicBot
{
    internal class Program
    {
        public static DiscordClient client { get; private set; }

        public static CommandsNextExtension commands { get; private set; }

        public static Config _config;

        public static async Task Main(string[] args)
        {
         
            string option = "1";
            await jsonParser.setConfig();

            _config = await jsonParser.jsonDeserializer(option);

            DiscordConfiguration discordConfig = new()
            {
                Token = _config.discordToken,
                Intents = DiscordIntents.All,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
            };

            client = new(discordConfig);   
    
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "",//replace
                Port = 000, //replace
                Secured = true,
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password =  "",//replace
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint,
            };


            client.Ready += OnReady;

            var lavalink = client.UseLavalink();      
           
            await client.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }

        private static async Task OnReady(DiscordClient client, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            var lavalink = client.GetLavalink();
            var guildId = await client.GetGuildAsync(_config.guildId);
            var chatChannel = await client.GetChannelAsync(_config.chatChannel);
            var voiceChannel = await client.GetChannelAsync(_config.voiceChannel);

            if (voiceChannel.Type != ChannelType.Voice)
            {
                 await chatChannel.SendMessageAsync("invalid voice channel");
                 return;
            }

           
            if (!lavalink.ConnectedNodes.Any())
            {
                await chatChannel.SendMessageAsync("lavalink is dead lol");
                return;
            }

            var node = lavalink.ConnectedNodes.Values.First();
            await node.ConnectAsync(voiceChannel);
            var conn = node.GetGuildConnection(guildId);
            await chatChannel.SendMessageAsync($"Joined <{voiceChannel}>");

            if (!QueueList.ContainsKey(guildId))
            {
                QueueList[guildId] = new List<LavalinkTrack>();
                Playing[guildId] = false;
            }

            await Prompth(guildId,voiceChannel, chatChannel, conn);
        }

        private static bool _showPromptAfterPlayback = true;
        private static void ShowPrompt()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine
(@"Please Pick a Command
    play [query]
    skip
    queue
    exit");
            Console.ForegroundColor = ConsoleColor.Red;
            _showPromptAfterPlayback = true;
        }

        public static async Task Prompth(DiscordGuild guildId, DiscordChannel voiceChannel, DiscordChannel chatChannel, LavalinkGuildConnection conn)
        {
            string command;
            do
            { 
                ShowPrompt();
                Console.ForegroundColor = ConsoleColor.Red;
                command = Console.ReadLine().ToLower();
                if (command.Contains("play"))
                {
                    command = command.Remove(0, 5);
                    await PlayMusic(guildId, voiceChannel, chatChannel, conn, command);
                }
                else if (command == "skip")
                {
                    await Skip(guildId, voiceChannel, chatChannel, conn);
                }
                else if (command == "queue")
                {
                    await Queue(guildId, voiceChannel, chatChannel, conn);
                }
                else if (command == "exit")
                {
                    Environment.Exit(0);
                }
            }while(!command.Contains("Play") || command != "skip" || command != "queue" || command != "exit");
            
        }

        public static readonly Dictionary<DiscordGuild, List<LavalinkTrack>> QueueList = new();

        public static readonly Dictionary<DiscordGuild, bool> Playing = new();

        public static async Task PlayMusic(DiscordGuild guildId, DiscordChannel voiceChannel, DiscordChannel chatChannel, LavalinkGuildConnection conn, string command)
        {

            var lavalink = client.GetLavalink();
            var node = lavalink.ConnectedNodes.Values.First();
            var loadResult = await node.Rest.GetTracksAsync(command);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {           
                await chatChannel.SendMessageAsync($"Track search failed for {command}.");
                
            }
    
            var track = loadResult.Tracks.First();
        

            var queue = QueueList[guildId];
            queue.Add(track);

            if (!Playing[guildId])
            {
                await PlayNext(guildId, voiceChannel, chatChannel, conn);
            }
            else
            {
                await chatChannel.SendMessageAsync($"{track.Title} added to queue");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"{track.Title} added to queue");
            }
        }

        public static async Task PlayNext(DiscordGuild guildId, DiscordChannel voiceChannel, DiscordChannel chatChannel, LavalinkGuildConnection conn)
        {
            if (!QueueList.TryGetValue(guildId, out var queue))
            {
                Playing[guildId] = false;
                return;
            }

            var track = queue[0];
            queue.RemoveAt(0);
         
            Playing[guildId] = true; 
            await conn.PlayAsync(track);
            await chatChannel.SendMessageAsync($"{track.Title} is now playing");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{track.Title} is now playing");

            if (!_showPromptAfterPlayback)
            ShowPrompt();


            conn.PlaybackFinished -= PlaybackFinishedHandler;
            conn.PlaybackFinished += PlaybackFinishedHandler;

            async Task PlaybackFinishedHandler(LavalinkGuildConnection sender, TrackFinishEventArgs args)
            {
                if (args.Reason == TrackEndReason.Finished)
                {
                    Playing[guildId] = false;
                    await chatChannel.SendMessageAsync($"{args.Track.Title} finished playing");
                    _showPromptAfterPlayback = false;
                    await PlayNext(guildId, voiceChannel, chatChannel, sender);
                }
            }
        }


        public static async Task Skip(DiscordGuild guildId, DiscordChannel voiceChannel, DiscordChannel chatChannel, LavalinkGuildConnection conn)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            await conn.StopAsync();
        }

        public static async Task Queue(DiscordGuild guildId, DiscordChannel voiceChannel, DiscordChannel chatChannel, LavalinkGuildConnection conn)
        {

            var queue = QueueList[guildId];

            var queueList = new StringBuilder("QUEUE: \n");

            if (conn.CurrentState.CurrentTrack != null)
            queueList.AppendLine($"Now Playing: {conn.CurrentState.CurrentTrack.Title} - {conn.CurrentState.CurrentTrack.Length} / {conn.CurrentState.PlaybackPosition}");


            if (QueueList[guildId].Count > 0)
            {
                for (int i = 0; i < queue.Count; i++)
                {
                    queueList.AppendLine($"{i+1}. {queue[i].Title} ({queue[i].Length})");
                }
            }
             
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(queueList.ToString());
            await chatChannel.SendMessageAsync("Place Holder Message");
        }
    }
}