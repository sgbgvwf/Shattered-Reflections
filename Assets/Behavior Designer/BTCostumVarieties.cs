using Core.Data;
using Core.Input;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedPlayerBlackboard : SharedVariable<PlayerBlackboard>
    {
        public static implicit operator SharedPlayerBlackboard(PlayerBlackboard value)
        {
            return new SharedPlayerBlackboard { mValue = value };
        }
    }

    [System.Serializable]
    public class SharedInputIntention : SharedVariable<InputIntention>
    {
        public static implicit operator SharedInputIntention(InputIntention value)
        {
            return new SharedInputIntention { mValue = value };
        }
    }
}
