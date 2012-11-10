using UnityEngine;using System.Collections;public class AnimationController : MonoBehaviour {    public string               RunAnimName   = "run";    public string               IdleAnimName  = "idle";    private AnimationState      LastSpecialAnimState = null;	// Use this for initialization	void Start ()     {        animation.Stop();        animation.wrapMode  = WrapMode.Once;        AnimationState AnimState = null;        AnimState           = animation[RunAnimName];        AnimState.wrapMode  = WrapMode.Loop;        AnimState.layer     = -1;        AnimState           = animation[IdleAnimName];        AnimState.wrapMode  = WrapMode.Loop;        AnimState.layer     = -1;        animation.SyncLayer(-1);

        PlayAnimation(IdleAnimName);

        Debug.Log("Animation Controllor");	}    public bool CheckAnimation(string AnimName)    {        if (animation.GetClip(AnimName) == null)        {            Debug.Log(AnimName + " Not Found");            return false;        }        return true;    }    public void PlaySpecialAnimation(string AnimName, float FadeIn = 0.3f)    {
        LastSpecialAnimState = animation.CrossFadeQueued(AnimName, FadeIn, QueueMode.PlayNow);    }    public bool IsSpecialAnimationFinished()    {        return LastSpecialAnimState == null || LastSpecialAnimState.time > LastSpecialAnimState.length - 0.1f;    }    public void PlayAnimation(string AnimName)    {        animation.CrossFade(AnimName);    }    public bool IsPlaying(string AnimName)    {        return animation.IsPlaying(AnimName);    }}