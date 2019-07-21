using UnityEngine;

namespace Abilities
{
    // This interface should be used to identify objects which are collectible for a signal instance.
    // The name should be used when collection occurs to check what is being collected.
    public interface ICollectible 
    {
        // Name to be checked against when identifying what is being collected.
        string CollectableName
        {
            get;
        }
    }
}