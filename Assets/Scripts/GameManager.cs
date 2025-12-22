using UnityEngine;
using Vuforia;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameMode { TBA, Kompleksometri }

    [Header("Mode Saat Ini")]
    public GameMode currentMode;

    [Header("Data Tahapan")]
    public TahapanData[] tahapanTBA;
    public TahapanData[] tahapanKompleksometri;

    [Header("State (runtime only)")]
    [Tooltip("Tahap yang sedang dikerjakan saat ini. -1 = belum ada.")]
    public int currentAttemptingTahapIndex = -1;

    private const string LAST_COMPLETED_TAHAP_TBA_KEY = "LastCompletedTahapTBA";
    private const string LAST_COMPLETED_TAHAP_KOMP_KEY = "LastCompletedTahapKomp";

    
    [Header("Info Panel Default Style")]
    public TMP_FontAsset montserratFont;                 
    public float defaultFontSize = 34f;
    public TextAlignmentOptions defaultAlignment = TextAlignmentOptions.TopLeft;
    public bool defaultBold = false;
    public float defaultLineSpacing = 4f;
    public float defaultParagraphSpacing = 10f;
    public Vector4 defaultMargin = new Vector4(32, 28, 32, 28); 

    
    private string CurrentProgressKey
    {
        get
        {
            return currentMode == GameMode.TBA
                ? LAST_COMPLETED_TAHAP_TBA_KEY
                : LAST_COMPLETED_TAHAP_KOMP_KEY;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetARCameraActive(false);
    }

    
    
    
    public void SetARCameraActive(bool isActive)
    {
        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.enabled = isActive;
            Debug.Log($"[GameManager] Vuforia AR Camera: {(isActive ? "ON" : "OFF")}");
        }
        else
        {
            Debug.LogWarning("[GameManager] VuforiaBehaviour instance not found!");
        }
    }

    
    
    
    public int GetLastCompletedTahapIndex()
    {
        
        return PlayerPrefs.GetInt(CurrentProgressKey, -1);
    }

    
    public void SetMode(GameMode mode)
    {
        currentMode = mode;
        currentAttemptingTahapIndex = -1;

        Debug.Log($"[GameManager] SetMode = {currentMode}");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            var panelToOpen = (currentMode == GameMode.TBA)
                ? UIManager.Instance.panelTBA
                : UIManager.Instance.panelTK;

            UIManager.Instance.ShowPanelAndAddToHistory(panelToOpen);
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }

    
    public void StartTahap(int tahapIndex)
    {
        int lastCompleted = GetLastCompletedTahapIndex();

        
        if (tahapIndex > lastCompleted + 1)
        {
            Debug.LogWarning($"[GameManager] Tahap {tahapIndex + 1} masih terkunci. LastCompleted = {lastCompleted}");
            return;
        }

        currentAttemptingTahapIndex = tahapIndex;

        TahapanData data = GetCurrentTahapanData(tahapIndex);
        string namaTahap = data != null ? data.namaTahapan : $"Tahap {tahapIndex + 1}";

        Debug.Log($"[GameManager] MULAI Tahap {tahapIndex + 1} - {namaTahap} (Mode {currentMode})");

        if (UIManager.Instance != null) UIManager.Instance.ForceHideInfoPanel();

        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPanelAndAddToHistory(UIManager.Instance.panelScanAR);
        }

        SetARCameraActive(true);
    }

    
    public void CompleteCurrentTahap()
    {
        if (currentAttemptingTahapIndex < 0)
        {
            Debug.LogWarning("[GameManager] CompleteCurrentTahap dipanggil tapi tidak ada tahap aktif.");
            return;
        }

        
        PlayerPrefs.SetInt(CurrentProgressKey, currentAttemptingTahapIndex);
        PlayerPrefs.Save();

        Debug.Log($"[GameManager] SELESAI Tahap {currentAttemptingTahapIndex + 1} (Mode {currentMode}). " +
                  $"Tahap berikutnya yang akan terbuka: {currentAttemptingTahapIndex + 2}");

        currentAttemptingTahapIndex = -1;
        SetARCameraActive(false);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ForceHideInfoPanel();
            UIManager.Instance.GoBack();
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }

    public void ShowInfoPopup(GameObject infoPanel, string infoText)
    {
        if (infoPanel == null) return;

        var textComponent = infoPanel.GetComponentInChildren<TextMeshProUGUI>(true);
        if (textComponent != null)
        {
            
            if (string.IsNullOrWhiteSpace(infoText))
            {
                int idx = currentAttemptingTahapIndex;
                if (currentMode == GameMode.TBA)
                    infoText = (idx >= 0 && idx < InfoTextBank.TBA.Length) ? InfoTextBank.TBA[idx] : "";
                else
                    infoText = (idx >= 0 && idx < InfoTextBank.TK.Length) ? InfoTextBank.TK[idx] : "";
            }
            textComponent.text = infoText;

            
            if (montserratFont != null) textComponent.font = montserratFont;
            textComponent.fontSize = defaultFontSize;
            textComponent.alignment = defaultAlignment;
            textComponent.fontStyle = defaultBold ? FontStyles.Bold : FontStyles.Normal;
            textComponent.lineSpacing = defaultLineSpacing;
            textComponent.paragraphSpacing = defaultParagraphSpacing;
            textComponent.margin = defaultMargin;
            
            textComponent.overflowMode = TextOverflowModes.Overflow;

            
            var idxNow = currentAttemptingTahapIndex;
            InfoTextBank.InfoStyle? styleMaybe = null;

            if (currentMode == GameMode.TBA && idxNow >= 0 && idxNow < InfoTextBank.TBAStyle.Length)
                styleMaybe = InfoTextBank.TBAStyle[idxNow];
            else if (currentMode == GameMode.Kompleksometri && idxNow >= 0 && idxNow < InfoTextBank.TKStyle.Length)
                styleMaybe = InfoTextBank.TKStyle[idxNow];

            if (styleMaybe.HasValue)
            {
                var st = styleMaybe.Value;
                if (st.font != null) textComponent.font = st.font;
                if (st.fontSize > 0) textComponent.fontSize = st.fontSize;
                textComponent.alignment = st.alignment == 0 ? textComponent.alignment : st.alignment;
                textComponent.fontStyle = st.bold ? FontStyles.Bold : FontStyles.Normal;
                if (st.lineSpacing > 0) textComponent.lineSpacing = st.lineSpacing;
                if (st.paragraphSpacing > 0) textComponent.paragraphSpacing = st.paragraphSpacing;

                
                float L = st.marginLeft > 0 ? st.marginLeft : textComponent.margin.x;
                float T = st.marginTop > 0 ? st.marginTop : textComponent.margin.y;
                float R = st.marginRight > 0 ? st.marginRight : textComponent.margin.z;
                float B = st.marginBottom > 0 ? st.marginBottom : textComponent.margin.w;
                textComponent.margin = new Vector4(L, T, R, B);
            }
        }

        infoPanel.SetActive(true);
        Debug.Log("[GameManager] Info popup ditampilkan (bank).");
    }

    public void HideInfoPopup(GameObject infoPanel)
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);  
            Debug.Log("[GameManager] Info popup disembunyikan.");
        }
    }


    
    public TahapanData GetCurrentTahapanData(int index)
    {
        if (currentMode == GameMode.TBA)
        {
            if (tahapanTBA != null && index >= 0 && index < tahapanTBA.Length)
                return tahapanTBA[index];
        }
        else 
        {
            if (tahapanKompleksometri != null && index >= 0 && index < tahapanKompleksometri.Length)
                return tahapanKompleksometri[index];
        }

        return null;
    }

    
    
    
    public void OnMarkerFound(string markerName)
    {
        
        Debug.Log($"[GameManager] Marker ditemukan: {markerName}. (Validasi tahapan DIMATIKAN sementara untuk debug indexing.)");
    }

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_TBA_KEY);
        PlayerPrefs.DeleteKey(LAST_COMPLETED_TAHAP_KOMP_KEY);
        PlayerPrefs.Save();

        currentAttemptingTahapIndex = -1;
        UIManager.Instance.ForceHideInfoPanel();
        SetARCameraActive(false);

        Debug.Log("[GameManager] ResetAllProgress: semua progres dihapus. Kembali ke Tahap 1 untuk tiap mode.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTahapButtonStates();
        }
    }
}
