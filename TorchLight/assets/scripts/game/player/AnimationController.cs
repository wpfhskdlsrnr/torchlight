using UnityEngine;

        PlayAnimation(IdleAnimName);

        Debug.Log("Animation Controllor");
        LastSpecialAnimState = animation.CrossFadeQueued(AnimName, FadeIn, QueueMode.PlayNow);