using Preparation.Utility;
using Preparation.Utility.Value;
using System.Threading.Tasks.Dataflow;

namespace GameClass.GameObj;

public abstract class Immovable(XY initPos, int initRadius, GameObjType initType)
    : GameObj(initPos, initRadius, initType)
{
    public override XY Position => position;
}
