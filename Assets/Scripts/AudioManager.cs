using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 0.7f;
    [Range(0.5f, 1.5f)]
    public float pitch = 1.0f;

    public bool isLoop;

    private AudioSource source;

    public void SetSource(AudioSource _source)
    {
        source = _source;
        source.clip = clip;
        source.loop = isLoop;
    }

    public void Play()
    {
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
    }

    public void Stop()
    {
        source.Stop();
    }
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField]
    Sound[] sounds;

    public Sound currentBg;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.Log("More than one AudioManager in the scene.");
        } else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < sounds.Length; i++)
        {
            GameObject _go = new GameObject("Sound_" + i + "_" + sounds[i].name);
            sounds[i].SetSource(_go.AddComponent<AudioSource>());
            _go.transform.SetParent(this.transform);
        }
        currentBg = PlaySound("greenlandbg");
        
    }

    public Sound PlaySound(string _name)
    {
        for(int i = 0; i < sounds.Length; i++)
        {
            if(sounds[i].name == _name)
            {
                sounds[i].Play();
                return sounds[i];
            }
        }
        Debug.LogError("Audio Manager: can't find sound " + _name);
        return null;
    }

    public Sound changeBg(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                currentBg.Stop();
                sounds[i].Play();
                currentBg = sounds[i];
                return sounds[i];
            }
        }
        Debug.LogError("Audio Manager: can't find sound " + _name);
        return null;
    }

    public Sound stopSound(string _name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == _name)
            {
                sounds[i].Stop();
                return sounds[i];
            }
        }
        Debug.LogError("Audio Manager: can't find sound " + _name);
        return null;
    }
}
