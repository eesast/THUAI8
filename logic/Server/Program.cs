using CommandLine;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Preparation.Utility;
using Preparation.Utility.Logging;
using Protobuf;

namespace Server
{
    public class Program
    {
        /// <summary>
        /// 通过网站 http://www.network-science.de/ascii/ 使用字体"standard"生成
        /// </summary>
        private const string WelcomeMessage = """
                    ______________ ___  ____ ___  _____  .___  ______  
                    \__    ___/   |   \|    |   \/  _  \ |   |/  __  \ 
                      |    | /    ~    \    |   /  /_\  \|   |>      < 
                      |    | \    Y    /    |  /    |    \   /   --   \
                      |____|  \___|_  /|______/\____|__  /___\______  /
                                    \/                 \/           \/
                         ____.                                           __          
                        |    | ____  __ _________  ____   ____ ___.__. _/  |_  ____  
                        |    |/  _ \|  |  \_  __ \/    \_/ __ <   |  | \   __\/  _ \ 
                    /\__|    (  <_> )  |  /|  | \/   |  \  ___/\___  |  |  | (  <_> )
                    \________|\____/|____/ |__|  |___|  /\___  > ____|  |__|  \____/ 
                                                      \/     \/\/                    
                      __  .__              __      __                 __   
                    _/  |_|  |__   ____   /  \    /  \ ____   _______/  |_ 
                    \   __\  |  \_/ __ \  \   \/\/   // __ \ /  ___/\   __\
                     |  | |   Y  \  ___/   \        /\  ___/ \___ \  |  |  
                     |__| |___|  /\___  >   \__/\  /  \___  >____  > |__|  
                               \/     \/         \/       \/     \/        
        """;

        private static (ServerBase, AdvancedLoggerFactory.Logger) CreateServer(ArgumentOptions options)
            => options.Playback
                ? (new PlaybackServer(options), PlaybackServerLogging.logger)
                : (new GameServer(options), GameServerLogging.logger);

        public static int Main(string[] args)
        {
            var loggerMain = AdvancedLoggerFactory.CreateLogger("Main");
            var argsStr = string.Join(' ', args);
            loggerMain.LogRaw(argsStr);

            ArgumentOptions? options = null;
            _ = Parser.Default.ParseArguments<ArgumentOptions>(args).WithParsed(o => { options = o; });
            if (options == null)
            {
                loggerMain.LogError("Argument parsing failed!");
                return 1;
            }

            if (options.StartLockFile == "114514")
            {
                loggerMain.LogRaw(WelcomeMessage);
            }
            loggerMain.LogInfo($"Server begins to run: {options.ServerPort}");

            try
            {
                var (server, logger) = CreateServer(options);
                Grpc.Core.Server rpcServer = new([new ChannelOption(ChannelOptions.SoReuseport, 0)])
                {
                    Services = { AvailableService.BindService(server) },
                    Ports = { new ServerPort(options.IP, options.ServerPort, ServerCredentials.Insecure) }
                };
                rpcServer.Start();

                logger.LogInfo("Server begins to listen!");
                server.WaitForEnd();
                logger.LogInfo("Server end!");
                rpcServer.ShutdownAsync().Wait();

                logger.LogInfo("===================  Final Score  ====================");
                logger.LogInfo($"Team0: {server.GetScore()[0]}"); //红队
                logger.LogInfo($"Team1: {server.GetScore()[1]}"); //蓝队
            }
            catch (Exception ex)
            {
                loggerMain = AdvancedLoggerFactory.CreateLogger("Main");
                loggerMain.LogError(ex.ToString());
                if (ex.StackTrace is not null)
                    loggerMain.LogError(ex.StackTrace);
            }
            return 0;
        }
    }
}