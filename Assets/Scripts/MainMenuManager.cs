// MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için bu satýr ZORUNLUDUR!
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public Slider masterVolumeSlider;
    // Bu fonksiyon "Oyuna Baþla" butonu tarafýndan çaðrýlacak.
    private void Start()
    {
        // Ayarlar panelini baþlangýçta kapalý tut (güvenlik için)
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        // YENÝ: Slider'ýn baþlangýç deðerini ayarla
        if (AudioManager.Instance != null && masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
        }
    }
    public void StartGame()
    {
        // Build Settings'e eklediðimiz "GameScene" sahnesini yükle.
        SceneManager.LoadScene("Scene1");
    }

    // Bu fonksiyon "Oyundan Çýk" butonu tarafýndan çaðrýlacak.
    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýlýyor..."); // Editörde test etmek için mesaj
        Application.Quit(); // Bu komut sadece build alýnmýþ oyunda çalýþýr.
    }
    // MainMenuManager.cs
    

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }
}
