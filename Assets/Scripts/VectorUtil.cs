using UnityEngine;

public enum VectorAxis { X, Y, Z }

public class VectorUtil
{
    public static Vector3 RandomVector3(Vector3 pos, float minRange, float maxRange, VectorAxis axis)
    {
        Vector3 generatePosVec = pos;
        
        switch (axis)
        {
            case VectorAxis.X:
                generatePosVec.x += Random.Range(minRange, maxRange);
                break;
            case VectorAxis.Y:
                generatePosVec.y += Random.Range(minRange, maxRange);
                break;
            case VectorAxis.Z:
                generatePosVec.z += Random.Range(minRange, maxRange);
                break;
            default:
                return Vector3.zero;
        }
        
        return generatePosVec;
    }
}