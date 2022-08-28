using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActivateRooms : MonoBehaviour
{
    [SerializeField] private Camera minimapCamera;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("EnableRooms", 0.5f, .75f);
    }

    private void EnableRooms()
    {
        if (GameManager.Instance.gameState == GameState.dungeonOverviewMap) return;
        foreach(KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            Room room = keyValuePair.Value;
            HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniMapCameraWorldPositionLowerBounds, out Vector2Int miniMapCameraWorldPositionUpperBounds, minimapCamera);
            
            //If room is within miniMap camera viewport then activate room game object
            if((room.lowerBounds.x <= miniMapCameraWorldPositionUpperBounds.x && room.lowerBounds.y <= miniMapCameraWorldPositionUpperBounds.y) &&
                (room.upperBounds.x >= miniMapCameraWorldPositionLowerBounds.x && room.upperBounds.y >= miniMapCameraWorldPositionLowerBounds.y))
            {
                room.instantiatedRoom.gameObject.SetActive(true);
            }
            else
                room.instantiatedRoom.gameObject.SetActive(false);
        }
    }

}
