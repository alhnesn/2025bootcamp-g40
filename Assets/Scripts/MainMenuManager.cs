// MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Sahne y�netimi i�in bu sat�r ZORUNLUDUR!
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public Slider masterVolumeSlider;
    // Bu fonksiyon "Oyuna Ba�la" butonu taraf�ndan �a�r�lacak.
    private void Start()
    {
        // Ayarlar panelini ba�lang��ta kapal� tut (g�venlik i�in)
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        // YEN�: Slider'�n ba�lang�� de�erini ayarla
        if (AudioManager.Instance != null && masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
        }
    }
    public void StartGame()
    {
        // Build Settings'e ekledi�imiz "GameScene" sahnesini y�kle.
        SceneManager.LoadScene("Main");
    }

    // Bu fonksiyon "Oyundan ��k" butonu taraf�ndan �a�r�lacak.
    public void QuitGame()
    {
        Debug.Log("Oyundan ��k�l�yor..."); // Edit�rde test etmek i�in mesaj
        Application.Quit(); // Bu komut sadece build al�nm�� oyunda �al���r.
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
