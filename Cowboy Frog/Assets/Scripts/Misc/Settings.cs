using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
    #region ROOM SETTINGS
    public const int maxChildCorridors = 3; //max number of child corridors leading from a room. maximum should be 3 although this is not recommended since it can cause dungeon building
                                            //to fail since the rooms are more likely to not fit together
    #endregion
}
