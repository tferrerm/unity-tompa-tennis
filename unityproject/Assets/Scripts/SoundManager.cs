using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] ballBounces;
    public AudioClip[] crowdSounds;
    public AudioClip[] playerGrunts;
    public AudioClip[] playerServices;
    public AudioClip[] footsteps;
    public AudioClip[] shoeSqueaks;
    public AudioClip[] racquetHits;
    public AudioClip[] racquetSwishes;
    public AudioClip[] netHits;
    
    public AudioSource ballAudioSource;
    public AudioSource sceneAudioSource;

    private void Start()
    {
        sceneAudioSource.volume = 0.3f;
    }

    public void PlayCrowdSounds()
    {
        sceneAudioSource.PlayOneShot(ChooseRandom(crowdSounds));
    }

    public void PlayFootstep(AudioSource playerAudioSource)
    {
        playerAudioSource.PlayOneShot(ChooseRandom(footsteps));
    }

    public void PlayGrunt(AudioSource playerAudioSource)
    {
        playerAudioSource.PlayOneShot(ChooseRandom(playerGrunts));
    }

    public void PlayRacquetHit(AudioSource playerAudioSource)
    {
        playerAudioSource.PlayOneShot(ChooseRandom(racquetHits));
    }

    public void PlayRacquetSwish(AudioSource playerAudioSource)
    {
        playerAudioSource.PlayOneShot(ChooseRandom(racquetSwishes));
    }

    public void PlayBallBounce(Vector3 ballSpeed)
    {
        ballAudioSource.volume = 0.05f * ballSpeed.magnitude;
        ballAudioSource.PlayOneShot(ChooseRandom(ballBounces));
    }

    public void PlayService(AudioSource playerAudioSource)
    {
        playerAudioSource.PlayOneShot(ChooseRandom(playerServices));
    }

    public void PlayShoeSqueak(AudioSource playerAudioSource)
    {
        playerAudioSource.PlayOneShot(ChooseRandom(shoeSqueaks));
    }

    public void PlayNetHit(Vector3 ballVelocity)
    {
        ballAudioSource.volume = 0.1f * ballVelocity.magnitude;
        ballAudioSource.PlayOneShot(ChooseRandom(netHits));
    }

    private static AudioClip ChooseRandom(IReadOnlyList<AudioClip> clips)
    {
        return clips[Random.Range(0, clips.Count)];
    }
}
