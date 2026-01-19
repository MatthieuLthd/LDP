using UnityEngine;

public class AudioReactor : MonoBehaviour
{
    public Material targetMaterial;
    // Assurez-vous que c'est bien le nom dans votre shader (_AudioStrength)
    public string parameterName = "_AudioStrength"; 

    [Header("Réglages Audio")]
    [Range(1, 500)] public float sensitivity = 100.0f;

    [Header("Réglages Douceur")]
    // Plus ce chiffre est BAS, plus le mouvement est lent et fluide (amorti)
    // Plus il est HAUT, plus il est réactif et nerveux.
    [Range(1f, 30f)] public float smoothSpeed = 10.0f; 

    // Variable pour stocker la valeur de l'image précédente
    private float currentValue = 0f;

    void Update()
    {
        // 1. Récupération de l'audio brut
        float[] data = new float[256];
        AudioListener.GetOutputData(data, 0);

        float volumeSum = 0;
        foreach (float s in data)
        {
            volumeSum += Mathf.Abs(s);
        }
        float averageVolume = volumeSum / 256.0f;
        
        // 2. Calcul de la valeur cible (ce qu'on VOUDRAIT atteindre)
        float targetValue = averageVolume * sensitivity;

        // --- LE LISSAGE (LERP) ---
        // On ne saute pas à targetValue. On glisse doucement de currentValue vers targetValue.
        // Time.deltaTime assure que la vitesse est la même peu importe les FPS.
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * smoothSpeed);


        // 3. Envoi de la valeur LISSÉE au shader
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat(parameterName, currentValue);
        }
    }
}