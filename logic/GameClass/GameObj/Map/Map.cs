using GameClass.GameObj.Areas;
using GameClass.MapGenerator;
using Preparation.Interface;
using Preparation.Utility;
using Preparation.Utility.Value;
using Preparation.Utility.Value.SafeValue;
using System;
using System.Collections.Generic;

namespace GameClass.GameObj.Map
{
    public partial class Map : IMap
    {
        private readonly Dictionary<GameObjType, LockedClassList<IGameObj>> gameObjDict;
        public Dictionary<GameObjType, LockedClassList<IGameObj>> GameObjDict => gameObjDict;
        private readonly uint height;
        public uint Height => height;
        private readonly uint width;
        public uint Width => width;
        public readonly PlaceType[,] protoGameMap;
        public PlaceType[,] ProtoGameMap => protoGameMap;

        private readonly MyTimer timer = new();
        public IMyTimer Timer => timer;

        #region GetPlaceType
        public PlaceType GetPlaceType(IGameObj obj)
        {
            try
            {
                var (x, y) = GameData.PosGridToCellXY(obj.Position);
                return protoGameMap[x, y];
            }
            catch
            {
                return PlaceType.NULL_PLACE_TYPE;
            }
        }
        public PlaceType GetPlaceType(XY pos)
        {
            try
            {
                var (x, y) = GameData.PosGridToCellXY(pos);
                return protoGameMap[x, y];
            }
            catch
            {
                return PlaceType.NULL_PLACE_TYPE;
            }
        }
        #endregion

        public bool IsOutOfBound(IGameObj obj)
        {
            return obj.Position.x >= GameData.MapLength - obj.Radius
                || obj.Position.x <= obj.Radius
                || obj.Position.y >= GameData.MapLength - obj.Radius
                || obj.Position.y <= obj.Radius;
        }
        public IOutOfBound GetOutOfBound(XY pos)
        {
            return new OutOfBoundBlock(pos);
        }

        public Character? FindCharacterInID(long ID)
        {
            return (Character?)GameObjDict[GameObjType.Character].Find(gameObj => (ID == ((Character)gameObj).ID));
        }
        public Character? FindCharacterInPlayerID(long teamID, long playerID)
        {
            return (Character?)GameObjDict[GameObjType.Character].Find(gameObj => (teamID == ((Character)gameObj).TeamID) && playerID == ((Character)gameObj).PlayerID);
        }
        public GameObj? OneForInteract(XY Pos, GameObjType gameObjType)
        {
            return (GameObj?)GameObjDict[gameObjType].Find(gameObj => GameData.ApproachToInteract(gameObj.Position, Pos));
        }
        public GameObj? OneInTheSameCell(XY Pos, GameObjType gameObjType)
        {
            return (GameObj?)GameObjDict[gameObjType].Find(gameObj => (GameData.IsInTheSameCell(gameObj.Position, Pos)));
        }
        public GameObj? PartInTheSameCell(XY Pos, GameObjType gameObjType)
        {
            return (GameObj?)GameObjDict[gameObjType].Find(gameObj => (GameData.PartInTheSameCell(gameObj.Position, Pos)));
        }
        public GameObj? OneForInteractInACross(XY Pos, GameObjType gameObjType)
        {
            return (GameObj?)GameObjDict[gameObjType].Find(gameObj =>
                GameData.ApproachToInteractInACross(gameObj.Position, Pos));
        }
        public GameObj? OneInTheRange(XY Pos, int range, GameObjType gameObjType)
        {
            return (GameObj?)GameObjDict[gameObjType].Find(gameObj =>
                GameData.IsInTheRange(gameObj.Position, Pos, range));
        }
        public List<Character>? CharacterInTheRangeNotTeamID(XY Pos, int range, long teamID)
        {
            return GameObjDict[GameObjType.Character].Cast<Character>()?.FindAll(character =>
                (GameData.IsInTheRange(character.Position, Pos, range) && character.TeamID != teamID));
        }
        public List<Character>? CharacterOnTheSameLineNotTeamID(XY Pos, double theta, long teamID)
        {
            return GameObjDict[GameObjType.Character].Cast<Character>()?.FindAll(character =>
            (GameData.IsOnTheSameLine(Pos, character.Position, theta) && character.TeamID != teamID));
        }
        public List<Character>? CharacterInTheRangeInTeamID(XY Pos, int range, long teamID)
        {
            return GameObjDict[GameObjType.Character].Cast<Character>()?.FindAll(character =>
                (GameData.IsInTheRange(character.Position, Pos, range) && character.TeamID == teamID));
        }
        public List<Character>? CharacterInTheList(List<CellXY> PosList)
        {
            return GameObjDict[GameObjType.Character].Cast<Character>()?.FindAll(character =>
                PosList.Contains(GameData.PosGridToCellXY(character.Position)));
        }
        public bool CanSee(Character character, GameObj gameObj)
        {
            XY pos1 = character.Position;
            XY pos2 = gameObj.Position;
            XY del = pos1 - pos2;
            if (del * del > character.ViewRange * character.ViewRange)
                return false;
            if (del.x > del.y)
            {
                var beginx = GameData.PosGridToCellX(pos1) + GameData.NumOfPosGridPerCell;
                var endx = GameData.PosGridToCellX(pos2);
                if (GetPlaceType(pos1) == PlaceType.BUSH && GetPlaceType(pos2) == PlaceType.BUSH)
                {
                    for (int x = beginx; x < endx; x += GameData.NumOfPosGridPerCell)
                    {
                        if (GetPlaceType(pos1 + del * (x / del.x)) != PlaceType.BUSH)
                            return false;
                    }
                }
                else
                {
                    for (int x = beginx; x < endx; x += GameData.NumOfPosGridPerCell)
                    {
                        if (GetPlaceType(pos1 + del * (x / del.x)) == PlaceType.BARRIER)
                            return false;
                    }
                }
            }
            else
            {
                var beginy = GameData.PosGridToCellY(pos1) + GameData.NumOfPosGridPerCell;
                var endy = GameData.PosGridToCellY(pos2);
                if (GetPlaceType(pos1) == PlaceType.BUSH && GetPlaceType(pos2) == PlaceType.BUSH)
                {
                    for (int y = beginy; y < endy; y += GameData.NumOfPosGridPerCell)
                    {
                        if (GetPlaceType(pos1 + del * (y / del.y)) != PlaceType.BUSH)
                            return false;
                    }
                }
                else
                {
                    for (int y = beginy; y < endy; y += GameData.NumOfPosGridPerCell)
                    {
                        if (GetPlaceType(pos1 + del * (y / del.y)) == PlaceType.BARRIER)
                            return false;
                    }
                }
            }
            return true;
        }
        public bool Remove(GameObj gameObj)
        {
            GameObj? ans = (GameObj?)GameObjDict[gameObj.Type].RemoveOne(obj => gameObj.ID == obj.ID);
            if (ans != null)
            {
                ans.TryToRemove();
                return true;
            }
            return false;
        }
        public bool RemoveJustFromMap(GameObj gameObj)
        {
            if (GameObjDict[gameObj.Type].Remove(gameObj))
            {
                gameObj.TryToRemove();
                return true;
            }
            return false;
        }
        public void Add(IGameObj gameObj)
        {
            GameObjDict[gameObj.Type].Add(gameObj);
        }
        public Map(MapStruct mapResource, A_ResourceType type = A_ResourceType.NULL)
        {
            gameObjDict = [];
            foreach (GameObjType idx in Enum.GetValues(typeof(GameObjType)))
            {
                if (idx != GameObjType.Null)
                    gameObjDict.TryAdd(idx, new LockedClassList<IGameObj>());
            }
            height = mapResource.height;
            width = mapResource.width;
            protoGameMap = mapResource.map;
            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    switch (mapResource.map[i, j])
                    {
                        case PlaceType.BARRIER:
                            Add(new BARRIER(GameData.GetCellCenterPos(i, j)));
                            break;
                        case PlaceType.BUSH:
                            Add(new Bush(GameData.GetCellCenterPos(i, j)));
                            break;
                        case PlaceType.ADDITION_RESOURCE:
                            Add(new A_Resource(GameData.AResourceRadius, type, GameData.GetCellCenterPos(i, j)));
                            break;
                        case PlaceType.CONSTRUCTION:
                            Add(new Construction(GameData.GetCellCenterPos(i, j)));
                            break;
                    }
                }
            }
        }
    }
}