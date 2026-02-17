using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    // ══════════════════════════════════════════════
    //  RÉFÉRENCES — ONGLETS

    [Header("=== TAB BUTTONS ===")]
    public Button audioTabButton;
    public Button graphicsTabButton;
    public Button controlsTabButton;

    [Header("=== TAB PANELS ===")]
    public GameObject audioSettings;
    public GameObject graphicsSettings;
    public GameObject controlsSettings;

    [Header("=== TAB COLORS ===")]
    public Color activeTabColor = new Color(0.24f, 0.24f, 0.59f, 1f);
    public Color inactiveTabColor = new Color(0.16f, 0.16f, 0.20f, 1f);

    // ══════════════════════════════════════════════
    //  RÉFÉRENCES — AUDIO

    [Header("=== AUDIO SLIDERS ===")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public TextMeshProUGUI masterValueText;
    public TextMeshProUGUI musicValueText;
    public TextMeshProUGUI sfxValueText;

    // ══════════════════════════════════════════════
    //  RÉFÉRENCES — GRAPHICS

    [Header("=== GRAPHICS ===")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    // ══════════════════════════════════════════════
    //  VARIABLES INTERNES

    private Resolution[] availableResolutions;

    // ══════════════════════════════════════════════
    //  INITIALISATION

    void OnEnable()
    {
        // Appelé chaque fois que le panel Options devient visible
        LoadSettings();
    }

    void Start()
    {
        PopulateResolutions();
        LoadSettings();
        ShowAudioTab(); // Onglet Audio ouvert par défaut

        // Lie les sliders à leurs textes de valeur
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(v =>
                SafeSetText(masterValueText, Mathf.RoundToInt(v) + "%"));

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(v =>
                SafeSetText(musicValueText, Mathf.RoundToInt(v) + "%"));

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(v =>
                SafeSetText(sfxValueText, Mathf.RoundToInt(v) + "%"));
    }

    // ══════════════════════════════════════════════
    //  ONGLETS

    public void ShowAudioTab()
    {
        SafeSetActive(audioSettings, true);
        SafeSetActive(graphicsSettings, false);
        SafeSetActive(controlsSettings, false);
        SetTabColors(audioTabButton, graphicsTabButton, controlsTabButton);
    }

    public void ShowGraphicsTab()
    {
        SafeSetActive(audioSettings, false);
        SafeSetActive(graphicsSettings, true);
        SafeSetActive(controlsSettings, false);
        SetTabColors(graphicsTabButton, audioTabButton, controlsTabButton);
    }

    public void ShowControlsTab()
    {
        SafeSetActive(audioSettings, false);
        SafeSetActive(graphicsSettings, false);
        SafeSetActive(controlsSettings, true);
        SetTabColors(controlsTabButton, audioTabButton, graphicsTabButton);
    }

    void SetTabColors(Button activeBtn, params Button[] inactiveBtns)
    {
        if (activeBtn != null)
            activeBtn.GetComponent<Image>().color = activeTabColor;

        foreach (Button btn in inactiveBtns)
            if (btn != null)
                btn.GetComponent<Image>().color = inactiveTabColor;
    }

    // ══════════════════════════════════════════════
    //  RÉSOLUTIONS

    void PopulateResolutions()
    {
        if (resolutionDropdown == null) return;

        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            options.Add(availableResolutions[i].width + " x "
                        + availableResolutions[i].height);

            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // ══════════════════════════════════════════════
    //  APPLY et BACK

    public void OnApplyButton()
    {
        // --- Audio ---
        if (masterSlider != null)
        {
            AudioListener.volume = masterSlider.value / 100f;
            PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        }
        if (musicSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        if (sfxSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);

        // --- Graphismes ---
        if (resolutionDropdown != null && availableResolutions != null
            && availableResolutions.Length > 0)
        {
            Resolution res = availableResolutions[resolutionDropdown.value];
            bool fs = fullscreenToggle != null && fullscreenToggle.isOn;
            Screen.SetResolution(res.width, res.height, fs);
            PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        }

        if (fullscreenToggle != null)
        {
            Screen.fullScreen = fullscreenToggle.isOn;
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        }

        if (vsyncToggle != null)
        {
            QualitySettings.vSyncCount = vsyncToggle.isOn ? 1 : 0;
            PlayerPrefs.SetInt("VSync", vsyncToggle.isOn ? 1 : 0);
        }

        PlayerPrefs.Save();
        Debug.Log("✅ Settings saved!");
    }

    public void OnBackButton()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.HideOptions();
        else
            gameObject.SetActive(false); // Fallback
    }

    // ══════════════════════════════════════════════
    //  CHARGEMENT DES PARAMÈTRES SAUVEGARDÉS

    void LoadSettings()
    {
        // Audio
        float master = PlayerPrefs.GetFloat("MasterVolume", 80f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 70f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 90f);

        if (masterSlider != null) { masterSlider.value = master; SafeSetText(masterValueText, Mathf.RoundToInt(master) + "%"); }
        if (musicSlider != null) { musicSlider.value = music; SafeSetText(musicValueText, Mathf.RoundToInt(music) + "%"); }
        if (sfxSlider != null) { sfxSlider.value = sfx; SafeSetText(sfxValueText, Mathf.RoundToInt(sfx) + "%"); }

        // Graphismes
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (vsyncToggle != null)
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;

        if (resolutionDropdown != null)
        {
            int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
            resolutionDropdown.value = savedIndex;
            resolutionDropdown.RefreshShownValue();
        }

        AudioListener.volume = master / 100f;
    }

    // ══════════════════════════════════════════════
    //  UTILITAIRES


    void SafeSetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    void SafeSetText(TextMeshProUGUI tmp, string text)
    {
        if (tmp != null) tmp.text = text;
    }
}