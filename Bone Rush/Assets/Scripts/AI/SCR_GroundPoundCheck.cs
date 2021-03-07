using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_GroundPoundCheck : MonoBehaviour
{

    ParticleSystem groundPoundPS;
    PlayerStats ps;
    //PlayerHealth ph;
    bool collided;
    List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle> { };

    // Start is called before the first frame update
    void Start()
    {
        groundPoundPS = GetComponent<ParticleSystem>();
        ps = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        collided = false;
    }


    public void GroundPound()
    {
        StartCoroutine(StartAttack());
    }

    IEnumerator StartAttack()
    {
        collided = false;
        groundPoundPS.Play();
        ps.canCollide = true;
        yield return new WaitForSeconds(.6f);
        ps.canCollide = false;
        groundPoundPS.Stop();
    }
}
