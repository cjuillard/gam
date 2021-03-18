using UnityEngine;

public class Character : MonoBehaviour
{
    public float windForce = 1;
    public Environment env;
    public Rigidbody rigidbody;

    public float ExplosionForce = 5;
    public float ExplosionRadius = 5;
    public float UpwardsModifier = 1;

    public ConstantForce windForceControl;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay (Input. mousePosition);
            var hits = Physics.RaycastAll(ray);
            if (hits.Length > 0)
            {
                Vector3 impactPos = hits[0].point;
                Vector3 offset = transform.position - impactPos;

                foreach (var collider in Physics.OverlapSphere(impactPos, ExplosionRadius))
                {
                    if (collider.attachedRigidbody != null)
                    {
                        collider.attachedRigidbody.AddExplosionForce(ExplosionForce, impactPos, ExplosionRadius, UpwardsModifier, ForceMode.Impulse);
                    }
                }
            }
        }
        
        Vector3 windDir = new Vector3();
        if (Input.GetKey(KeyCode.RightArrow))
            windDir.x++;
        if (Input.GetKey(KeyCode.LeftArrow))
            windDir.x--;
        if (Input.GetKey(KeyCode.UpArrow))
            windDir.z++;
        if (Input.GetKey(KeyCode.DownArrow))
            windDir.z--;

        env.SetWind(windDir, windForce);

        if (windDir == Vector3.zero)
        {
            windForceControl.force = Vector3.zero;
            return;
        }
        else
        {
            windDir.Normalize();
            windForceControl.force = windDir * windForce;
        }
            
    }
}
