using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryUtility {

    public static List<Vector3> CalculatePath(Vector3 startPosition, Vector3 direction, float radius)
    {
        List<Vector3> collisionPoints = new List<Vector3>();

        RaycastHit hit;
        if(Physics.SphereCast(startPosition, radius, direction, out hit))
        {
            Vector3 collisionPoint = RadiusAdjustedCollisionPoint(startPosition, hit.point, radius);
            collisionPoints.Add(collisionPoint);

            while (hit.collider.tag == "Wall")
            {
                Vector3 reflectionVector = Vector3.Reflect(direction, hit.normal);
                direction = reflectionVector;
                if (Physics.SphereCast(collisionPoint, radius, direction, out hit))
                {
                    collisionPoint = RadiusAdjustedCollisionPoint(collisionPoint, hit.point, radius);
                    collisionPoints.Add(collisionPoint);
                }
                else
                {
                    break;
                }
            }

        }

        return collisionPoints;
    }

    private static Vector3 RadiusAdjustedCollisionPoint(Vector3 startPosition, Vector3 endPoint, float radius)
    {
        Vector3 direction = endPoint - startPosition;
        float distance = direction.magnitude;
        Vector3 unitDirection = direction.normalized;
        Vector3 adjustedEndPoint = startPosition + unitDirection * (distance - radius);

        return adjustedEndPoint;
    }




}
