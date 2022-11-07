using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchParticle : MonoBehaviour
{
    ParticleSystem particles;

    // Start is called before the first frame update
    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        StartCoroutine(destroyAfterDone());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator destroyAfterDone()
    {
        yield return new WaitForSecondsRealtime(particles.startLifetime + .1f);
        Destroy(gameObject);
    }
}
