#include "AI.h"
#include "logic.h"
#include "structures.h"
#include <tclap/CmdLine.h>
#include <array>
#include <string_view>
#include <memory>
#undef GetMessage
#undef SendMessage
#undef PeekMessage

#ifdef _MSC_VER
#pragma warning(disable : 4996)
#endif

using namespace std::literals::string_view_literals;

// Generated by http://www.network-science.de/ascii/ with font "standard"
// 待定 画画
static constexpr std::string_view welcomeString = R"welcome(
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
)welcome"sv;

int THUAI8Main(int argc, char** argv, CreateAIFunc AIBuilder)
{
    int pID = 0;
    int tID;
    std::string sIP = "172.22.32.1";
    std::string sPort = "8888";
    bool file = false;
    bool print = false;
    bool warnOnly = false;
    int side_flag = 0;
    extern const std::array<THUAI8::CharacterType, 6> BuddhistsCharacterTypeDict;
    extern const std::array<THUAI8::CharacterType, 6> MonstersCharacterTypeDict;

    // 使用cmdline的正式版本
    try
    {
        TCLAP::CmdLine cmd("THUAI8 C++ interface commandline parameter introduction");

        TCLAP::ValueArg<std::string> serverIP("I", "serverIP", "Server`s IP 127.0.0.1 in default", false, "127.0.0.1", "string");
        cmd.add(serverIP);

        TCLAP::ValueArg<std::string> serverPort("P", "serverPort", "Port the server listens to 7777 in default", false, "7777", "USORT");
        cmd.add(serverPort);

        std::vector<int> validTeamIDs{0, 1};  // 取经队伍0 妖怪队伍1
        TCLAP::ValuesConstraint<int> teamIdConstraint(validTeamIDs);
        TCLAP::ValueArg<int> teamID("t", "teamID", "Team ID 0,1 valid only", true, -1, &teamIdConstraint);
        cmd.add(teamID);

        std::vector<int> validPlayerIDs{0, 1, 2, 3, 4, 5, 6};  // 0代表每队的Hero
        TCLAP::ValuesConstraint<int> playerIdConstraint(validPlayerIDs);
        TCLAP::ValueArg<int> playerID("p", "playerID", "Player ID 0,1,2,3,4,5,6 valid only", true, -1, &playerIdConstraint);
        cmd.add(playerID);

        std::string DebugDesc = "Set this flag to save the debug log to ./logs folder.\n";
        TCLAP::SwitchArg debug("d", "debug", DebugDesc);
        cmd.add(debug);

        std::string OutputDesc = "Set this flag to print the debug log to the screen.\n";
        TCLAP::SwitchArg output("o", "output", OutputDesc);
        cmd.add(output);

        std::vector<int> valid_side_flag{0, 1};  // 0代表取经队伍 1代表妖怪队伍
        TCLAP::ValuesConstraint<int> sideFlagConstraint(valid_side_flag);
        TCLAP::ValueArg<int> sideFlag("s", "side_flag", "Side flag 0,1 valid only", true, -1, &sideFlagConstraint);
        cmd.add(sideFlag);

        TCLAP::SwitchArg warning("w", "warning", "Set this flag to only print warning on the screen.\n"
                                                 "This flag will be ignored if the output flag is not set\n");
        cmd.add(warning);

        cmd.parse(argc, argv);
        tID = teamID.getValue();
        side_flag = (tID == 1);
        pID = playerID.getValue();
        sIP = serverIP.getValue();
        sPort = serverPort.getValue();
        side_flag = sideFlag.getValue();
        file = debug.getValue();
        print = output.getValue();
        if (print)
            warnOnly = warning.getValue();
    }
    catch (TCLAP::ArgException& e)
    {
        std::cerr << "Parsing error: " << e.error() << " for arg " << e.argId() << std::endl;
        return 1;
    }
    try
    {
        THUAI8::PlayerType playerType;
        THUAI8::CharacterType CharacterType = THUAI8::CharacterType::NullCharacterType;
        if (pID == 0)
            playerType = THUAI8::PlayerType::Team;
        else
        {
            playerType = THUAI8::PlayerType::Character;
            if (!side_flag)
                CharacterType = BuddhistsCharacterTypeDict[pID - 1];
            else
                CharacterType = MonstersCharacterTypeDict[pID - 1];
        }
#ifdef _MSC_VER
        std::cout
            << welcomeString << std::endl;
#endif
        Logic logic(pID, tID, playerType, CharacterType, side_flag);
        logic.Main(AIBuilder, sIP, sPort, file, print, warnOnly, side_flag);
    }
    catch (const std::exception& e)
    {
        std::cerr << "C++ Exception: " << e.what() << '\n';
    }
    catch (...)
    {
        std::cerr << "Unknown Exception\n";
    }
    return 0;
}

std::unique_ptr<IAI> CreateAI(int32_t pID)
{
    return std::make_unique<AI>(pID);
}

int main(int argc, char* argv[])
{
    return THUAI8Main(argc, argv, CreateAI);
}
