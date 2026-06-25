using UnityEngine;

// Lightweight marker automatically added to each spawned prefab by Spawn.cs.
// Stack.cs and Stickman.cs read this to know which side the prefab came from.
public class PrefabDirection : MonoBehaviour
{
    // true  = spawned from RIGHT, travelling left  (original behaviour)
    // false = spawned from LEFT,  travelling right (new behaviour)
    public bool comingFromRight = true;
}
