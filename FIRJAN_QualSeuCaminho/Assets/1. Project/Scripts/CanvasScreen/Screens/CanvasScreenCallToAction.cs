using UnityEngine;
using UnityEngine.UI;

public class CanvasScreenCallToAction : CanvasScreen
{
    public Toggle toggle_ptbr;
    public Toggle toggle_english;
    private void Awake()
    {
        // Ensure toggles don't trigger listeners during initial setup
        if (LocalizationManager.instance != null)
        {
            var lang = PlayerPrefs.GetString("lang", LocalizationManager.instance.defaultLang);
            if (toggle_ptbr != null) toggle_ptbr.SetIsOnWithoutNotify(lang == "pt");
            if (toggle_english != null) toggle_english.SetIsOnWithoutNotify(lang == "en");
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (toggle_ptbr != null) toggle_ptbr.onValueChanged.AddListener(OnTogglePtBrChanged);
        if (toggle_english != null) toggle_english.onValueChanged.AddListener(OnToggleEnglishChanged);
        ForcePortugueseLanguage();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (toggle_ptbr != null) toggle_ptbr.onValueChanged.RemoveListener(OnTogglePtBrChanged);
        if (toggle_english != null) toggle_english.onValueChanged.RemoveListener(OnToggleEnglishChanged);
    }

    private void OnTogglePtBrChanged(bool isOn)
    {
        if (!isOn) return;
        if (LocalizationManager.instance != null)
        {
            LocalizationManager.instance.SetLanguage("pt");
            // ensure the other toggle reflects the change without notifying
            if (toggle_english != null) toggle_english.SetIsOnWithoutNotify(false);
        }
    }

    private void OnToggleEnglishChanged(bool isOn)
    {
        if (!isOn) return;
        if (LocalizationManager.instance != null)
        {
            LocalizationManager.instance.SetLanguage("en");
            if (toggle_ptbr != null) toggle_ptbr.SetIsOnWithoutNotify(false);
        }
    }
    public override void TurnOn()
    {
        if (SessionPlacar.Instance != null)
        {
            SessionPlacar.Instance.ResetInstance();
        }
        ForcePortugueseLanguage();
        base.TurnOn();
    }

    private void ForcePortugueseLanguage()
    {
        if (LocalizationManager.instance == null) return;

        LocalizationManager.instance.SetLanguage("pt");
        PlayerPrefs.SetString("lang", "pt");
        PlayerPrefs.Save();

        if (toggle_ptbr != null) toggle_ptbr.SetIsOnWithoutNotify(true);
        if (toggle_english != null) toggle_english.SetIsOnWithoutNotify(false);
    }
}
