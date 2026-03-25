using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Configuration")]
    [SerializeField] private AudioMixer mainMixer;
    
    // Valeurs par défaut (0.75f = environ -2dB à -3dB)
    [Range(0.0001f, 1f)] public float masterVolume = 0.75f;
    [Range(0.0001f, 1f)] public float musicVolume = 0.6f;
    [Range(0.0001f, 1f)] public float voiceVolume = 1.0f;
    [Range(0.0001f, 1f)] public float sfxVolume = 0.8f;

    private void Awake()
    {
        // Gestion du Singleton pour la persistance entre scènes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ApplyAllVolumes();
    }

    public void ApplyAllVolumes()
    {
        SetMixerVolume("MasterVol", masterVolume);
        SetMixerVolume("MusicVol", musicVolume);
        SetMixerVolume("VoiceVol", voiceVolume);
        SetMixerVolume("SFXVol", sfxVolume);
    }

    /// <summary>
    /// Convertit une valeur linéaire (0 à 1) en décibels (-80 à 0)
    /// </summary>
    private void SetMixerVolume(string parameterName, float linearValue)
    {
        // On utilise Log10 car l'oreille humaine n'est pas linéaire
        // 0.0001f est le minimum pour éviter le Log10 de 0 (impossible)
        float dB = Mathf.Log10(Mathf.Max(0.0001f, linearValue)) * 20;
        mainMixer.SetFloat(parameterName, dB);
    }
}