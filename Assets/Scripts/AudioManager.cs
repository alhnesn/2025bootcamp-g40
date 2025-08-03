// AudioManager.cs
using UnityEngine;
using UnityEngine.Audio; // AudioMixer için bu satýr ZORUNLUDUR!

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer mainMixer;

    private void Awake()
    {
        // Singleton ve DontDestroyOnLoad paterni
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Bu obje sahneler arasýnda geçiþte yok olmaz.
    }

    public void SetMasterVolume(float volume)
    {
        // Slider'dan gelen 0.0001-1 arasýndaki deðeri, -80 ile 0 arasýndaki desibel'e çevirir.
        // Bu, insan kulaðýnýn sesi algýlama þekline daha uygundur (logaritmik).
        mainMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
    public float GetMasterVolume()
    {
        // Mixer'dan mevcut desibel deðerini oku
        if (mainMixer.GetFloat("MasterVolume", out float value))
        {
            // Desibel deðerini, slider'ýn kullandýðý 0-1 aralýðýna geri çevir
            // Bu, SetMasterVolume'daki formülün tam tersidir.
            return Mathf.Pow(10, value / 20);
        }
        // Eðer bir deðer okunamazsa, varsayýlan olarak tam ses döndür.
        return 1f;
    }
}