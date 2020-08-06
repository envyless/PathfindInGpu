using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScreenToWorldPlane 
{
    public static Vector3 GetWorldPlanePos(Camera gameCamera = null)
    {
        if (gameCamera == null)
            gameCamera = Camera.main;

        //it's pixel coordinate like resolution 
        var pixel_coordinate = Input.mousePosition;

        //this camera look down, that mean swap position z and y
        if(gameCamera.transform.forward.y < 0)
            pixel_coordinate.z = gameCamera.transform.position.y;
        
        var worldPos = gameCamera.ScreenToWorldPoint(pixel_coordinate);

        //make that int
        worldPos.x = (int)worldPos.x;
        worldPos.y = (int)worldPos.y;
        worldPos.z = (int)worldPos.z;

        return worldPos;
    }
}
