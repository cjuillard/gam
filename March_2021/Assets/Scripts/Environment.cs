using UnityEngine;

public class Environment : MonoBehaviour
{
    public WindZone wind;
    
    void Start()
    {
        wind.windMain = 0;
    }

    
    void Update()
    {
        
    }

    public void SetWind(Vector3 windDir, float force)
    {
        if (windDir == Vector3.zero)
        {
            wind.windMain = 0;
            return;
        }
            
        windDir.Normalize();
        wind.transform.forward = windDir;
        wind.windMain = 1;
    }
}
