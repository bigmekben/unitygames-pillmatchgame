using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource moveSound;
    public AudioSource landedSound;
    public AudioSource matchSound;
    public AudioSource settleSound;
    public AudioSource rotateSound;

    public void PlayMove()
    {
        moveSound.Play();
    }
    public void PlayLanded()
    {
        landedSound.Play();
    }

    public void PlayMatch()
    {
        matchSound.Play();
    }

    public void PlaySettle()
    {
        settleSound.Play();
    }

    public void PlayRotate()
    {
        rotateSound.Play();
    }
}
