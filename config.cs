using System.Security.Cryptography.X509Certificates;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using MusicBot;
using System.Threading.Tasks;

namespace MusicBot.config
{
    public class jsonParser
    { 

        //handles the config configuration
        public static async Task setConfig()
        {
            string option;
            string caseb = "1";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("A. Create a new config file");
            Console.WriteLine("B. Replace a config value");
            Console.WriteLine("C. Continue");
            do
            {
                Console.ForegroundColor = ConsoleColor.Red;
                option = Console.ReadLine().ToLower();
                if (option == "a")
                {
                    await jsonSerializer(option, caseb);
                    return;
                }
                else if (option == "b")
                {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("A. guildId");
                        Console.WriteLine("B. voice channel");
                        Console.WriteLine("C. chat channel");
                        Console.WriteLine("D. bot token");
                        
                        do
                        {
                            caseb = Console.ReadLine().ToLower();

                            if (caseb == "a")
                            {
                                await jsonSerializer(option, caseb);
                                return;
                            }
                            else if (caseb == "b")
                            {
                                await jsonSerializer(option, caseb);
                                return;
                            }
                            else if (caseb == "c")
                            {
                                await jsonSerializer(option, caseb);
                                return;
                            }
                            else if (caseb == "d")
                            {
                                await jsonSerializer(option, caseb);
                                return;
                            }
                        }while(caseb != "a");
                }
                else if (option == "c")
                {
                    await jsonDeserializer(option);
                    return;      
                }
            }while(option == "a" || option == "b" || option == "c");
        }

        //serialization
        public static async Task jsonSerializer(string option, string caseb)
        {
            Config config;
            if (File.Exists("config.Json"))
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            }
            else
            {
                config = new Config();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            if (option == "a" || caseb == "a")
            {
                Console.WriteLine("Enter the guild Id (server) that your bot will be accessing");
                Console.ForegroundColor = ConsoleColor.Red;
                config.guildId = ulong.Parse(Console.ReadLine());
            }
            if (option == "a" || caseb == "b")
            {
                Console.WriteLine("Enter the voice channel that your bot will be playing music on");
                Console.ForegroundColor = ConsoleColor.Red;
                config.voiceChannel = ulong.Parse(Console.ReadLine());
            }
            if (option == "a" || caseb == "c")
            {
                Console.WriteLine("Enter the chatroom channel that your bot will be sending messages to");
                Console.ForegroundColor = ConsoleColor.Red;
                config.chatChannel = ulong.Parse(Console.ReadLine());
            }
            if (option == "a" || caseb == "d" )
            {
                Console.WriteLine("Enter your Discord bot token");
                Console.ForegroundColor = ConsoleColor.Red;
                config.discordToken = Console.ReadLine();
            }

            string configJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("config.json", configJson);

            if (caseb != null)
            {
                await setConfig();
            }
        }

        //deserializer
        public static async Task<Config> jsonDeserializer(string option)
        {
            if (File.Exists("config.json"))
            {
                string json = File.ReadAllText("config.json");
                Config config = JsonConvert.DeserializeObject<Config>(json); 
                if (config.voiceChannel == default(ulong) || config.chatChannel == default(ulong) || config.guildId == default(ulong) || config.discordToken == null) 
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine
(@$"A value is Missing:
    Guild Id = {config.guildId}
    Voice Channel Id = {config.voiceChannel}
    Chatroom Id = {config.chatChannel}
    Discord Token = {config.discordToken}");
                    await setConfig();
                }
                return config;
            }   
            return null;
        }
    }
    public class Config
    {
        public ulong guildId {get; set;}
        public ulong voiceChannel {get; set;}
        public ulong chatChannel {get; set;}
        public string discordToken {get; set;}
    }
}