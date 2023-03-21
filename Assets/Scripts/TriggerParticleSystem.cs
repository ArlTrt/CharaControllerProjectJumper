using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerParticleSystem : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public Animator anim;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetTrigger("Explosionevent");
            anim.Play("NewExplosion");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            particleSystem.Stop();
        }
    }
}

 /*AnimationClip clip;
        Animator anim;

        // new event created
        AnimationEvent evt;
        evt = new AnimationEvent();

        // put some parameters on the AnimationEvent
        //  - call the function called PrintEvent()
        //  - the animation on this object lasts 2 seconds
        //    and the new animation created here is
        //    set up to happen 1.3s into the animation
        evt.intParameter = 12345;
        evt.time = 1.3f;
        evt.functionName = "PrintEvent";

        // get the animation clip and add the AnimationEvent
        anim = GetComponent<Animator>();
        clip = anim.runtimeAnimatorController.animationClips[0];
        clip.AddEvent(evt);*/