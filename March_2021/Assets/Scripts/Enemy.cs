using UnityEngine;
using UnityTemplateProjects;

public class Enemy : MonoBehaviour
{
    public Rigidbody Rigid;
    public float Force = 1;
    private Simulation sim;
    
    void Start()
    {
        sim = FindObjectOfType<Simulation>();
    }

    void Update()
    {
        var delta = sim.Player.transform.position - Rigid.position;
        Rigid.AddForce(delta.normalized * Force, ForceMode.Force);
    }
}
