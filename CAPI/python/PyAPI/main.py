import argparse
import os
import platform
import sys
from typing import Callable, List

sys.path.append(os.path.dirname(os.path.dirname(os.path.realpath(__file__))))
sys.path.append(os.path.dirname(os.path.dirname(os.path.realpath(__file__))) + "/proto")

import PyAPI.structures as THUAI8  # NOQA: E402
from PyAPI.AI import AI, Setting  # NOQA: E402
from PyAPI.Interface import IAI  # NOQA: E402
from PyAPI.logic import Logic  # NOQA: E402


def PrintWelcomeString() -> None:
    # Generated by http://www.network-science.de/ascii/ with font "standard"
    welcomeString = r"""
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
    """
    print(welcomeString)


def THUAI8Main(argv: List[str], AIBuilder: Callable) -> None:
    pID: int = 0
    sIP: str = "127.0.0.1"
    sPort: str = "8888"
    file: bool = True
    screen: bool = True
    warnOnly: bool = False
    side_flag: int = 0
    parser = argparse.ArgumentParser(
        description="THUAI8 Python Interface Commandline Parameter Introduction"
    )
    parser.add_argument(
        "-I",
        type=str,
        required=True,
        help="Server`s IP 127.0.0.1 in default",
        dest="sIP",
        default="127.0.0.1",
    )
    parser.add_argument(
        "-P",
        type=str,
        required=True,
        help="Server`s Port 8888 in default",
        dest="sPort",
        default="8888",
    )
    parser.add_argument(
        "-t",
        type=int,
        required=True,
        help="Team`s ID",
        dest="tID",
        choices=[0, 1],
    )
    parser.add_argument(
        "-p",
        type=int,
        required=True,
        help="Player`s ID",
        dest="pID",
        choices=[0, 1, 2, 3, 4, 5, 6],
    )
    parser.add_argument(
        "-d",
        action="store_true",
        help="Set this flag to save the debug log to ./logs folder",
        dest="file",
    )
    parser.add_argument(
        "-o",
        action="store_true",
        help="Set this flag to print the debug log to the screen",
        dest="screen",
    )
    parser.add_argument(
        "-w",
        action="store_true",
        help="Set this flag to only print warning on the screen",
        dest="warnOnly",
    )
    parser.add_argument(
        "-s",
        type=int,
        required=False,
        help="Set this flag to set the side flag",
        dest="side",
        choices=[0, 1],
    )
    args = parser.parse_args()
    tID = args.tID
    pID = args.pID
    sIP = args.sIP
    sPort = args.sPort
    file = args.file
    screen = args.screen
    warnOnly = args.warnOnly
    side_flag = args.side
    playerType = THUAI8.PlayerType.NullPlayerType
    characterType = THUAI8.CharacterType.NullCharacterType
    if pID == 0:
        playerType = THUAI8.PlayerType.Team
    elif tID == 0:
        playerType = THUAI8.PlayerType.Character
        characterType = Setting.BuddhistsCharacterTypes()[pID - 1]
    elif tID == 1:
        playerType = THUAI8.PlayerType.Character
        characterType = Setting.MonsterCharacterTypes()[pID - 1]

    if platform.system().lower() == "windows":
        PrintWelcomeString()

    logic = Logic(pID, tID, playerType, characterType, side_flag)
    logic.Main(AIBuilder, sIP, sPort, file, screen, warnOnly, side_flag)


def CreateAI(pID: int) -> IAI:
    # print(f"Creating AI for player {pID}...")
    ai = AI(pID)
    # print(f"✔ Successfully created AI instance for player {pID}")
    return ai


if __name__ == "__main__":
    THUAI8Main(sys.argv, CreateAI)
