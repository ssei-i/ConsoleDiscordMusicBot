using System.Security.Cryptography.X509Certificates;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using MusicBot;
using System.Threading.Tasks;

namespace MusicBot.config
{
    public class jsonParser
    { 
        public static async Task setConfig()
        {
            string option;
            Console.WriteLine("A. Create a new config file");
            Console.WriteLine("B. Replace config file values");
            Console.WriteLine("C. Continue");
            do
            {
                option = Console.ReadLine().ToLower();
                
                if (option == "a" || option == "b")
                {
                    await jsonSerializer(option);
                    return;
                }
                else if (option == "c")
                {
                    await jsonDeserializer(option);
                    return;
                }
            }while(option == "a" || option == "b" || option == "c");
        }

        public static async Task jsonSerializer(string option)
        {
            var config = new Config();
            Console.WriteLine("Enter the guild Id (server) that your bot will be accessing");
            config.guildId = ulong.Parse(Console.ReadLine());
            Console.WriteLine("Enter the voice channel that your bot will be playing music on");
            config.voiceChannel = ulong.Parse(Console.ReadLine());
            Console.WriteLine("Enter the chatroom channel that your bot will be sending messages to");
            config.chatChannel = ulong.Parse(Console.ReadLine());
            Console.WriteLine("Enter your Discord bot token");
            config.discordToken = Console.ReadLine();
            var configJson = JsonConvert.SerializeObject(config);
            File.WriteAllText("config.json", configJson);
        }

        public static async Task<Config> jsonDeserializer(string option)
        {
            if (File.Exists("config.json"))
            {
                string json = File.ReadAllText("config.json");
                Config config = JsonConvert.DeserializeObject<Config>(json);  
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