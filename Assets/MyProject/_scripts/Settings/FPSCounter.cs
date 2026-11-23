
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;

    [SerializeField] private float updateInterval = 0.5f;

    private float elapsed;
    private int frames;

    private void Update()
    {
        if (fpsText == null)
            return;

        GameSettingsManager settings =
            GameSettingsManager.Instance;

        bool show =
            settings != null &&
            settings.Current.showFPS;

        if (!show)
        {
            if (fpsText.gameObject.activeSelf)
                fpsText.gameObject.SetActive(false);

            return;
        }

        if (!fpsText.gameObject.activeSelf)
            fpsText.gameObject.SetActive(true);

        elapsed += Time.unscaledDeltaTime;
        frames++;

        if (elapsed >= updateInterval)
        {
            float fps = frames / elapsed;

            fpsText.text =
                Mathf.RoundToInt(fps) + " FPS";

            elapsed = 0f;
            frames = 0;
        }
    }
}
