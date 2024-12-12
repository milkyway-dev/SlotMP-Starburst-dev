using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    [Header("Popus UI")]
    [SerializeField] private GameObject MainPopup_Object;

    [Header("Win Popup")]
    [SerializeField] private Image Win_Image;
    [SerializeField] private GameObject WinPopup_Object;
    [SerializeField] private TMP_Text Win_Text;
    [SerializeField] private RectTransform WinBgAnimation;
    [SerializeField] private Sprite BigWin_Sprite, HugeWin_Sprite, MegaWin_Sprite, Jackpot_Sprite;
    [SerializeField] private ImageAnimation JackpotImageAnimation;
    private Tween ImageRotationTween;

    [Header("Disconnection Popup")]
    [SerializeField] private Button CloseDisconnect_Button;
    [SerializeField] private GameObject DisconnectPopup_Object;

    [Header("AnotherDevice Popup")]
    [SerializeField] private GameObject ADPopup_Object;

    [Header("LowBalance Popup")]
    [SerializeField] private Button LBExit_Button;
    [SerializeField] private GameObject LBPopup_Object;

    [Header("Audio Objects")]
    [SerializeField] private GameObject Settings_Object;
    [SerializeField] private Button SettingsQuit_Button;
    [SerializeField] private Button Sound_Button;
    [SerializeField] private Button Music_Button;
    [SerializeField] private RectTransform SoundToggle_RT;
    [SerializeField] private RectTransform MusicToggle_RT;

    [Header("Paytable Objects")]
    [SerializeField] private GameObject PaytableMenuObject;
    [SerializeField] private Button Paytable_Button;
    [SerializeField] private Button PaytableClose_Button;
    [SerializeField] private Button PaytableClose_Button2;
    [SerializeField] private Button PaytableLeft_Button;
    [SerializeField] private Button PaytableRight_Button;
    [SerializeField] private TMP_Text FreeSpin_Text;
    [SerializeField] private TMP_Text Jackpot_Text;
    [SerializeField] private TMP_Text Wild_Text;
    [SerializeField] private List<GameObject> GameRulesPages = new();
    private int PageIndex;

    [Header("Game Quit Objects")]
    [SerializeField] private Button Quit_Button;
    [SerializeField] private Button QuitYes_Button;
    [SerializeField] private Button QuitNo_Button;
    [SerializeField] private GameObject QuitMenuObject;

    [Header("Menu Objects")]
    [SerializeField] private Button Menu_Button;
    [SerializeField] private Button Info_Button;
    [SerializeField] private Button Settings_Button;
    [SerializeField] private Button RaycastLayerButton;
    [SerializeField] private RectTransform Info_BttnTransform;
    [SerializeField] private RectTransform Settings_BttnTransform;

    [Header("Paytable Slot Text")]
    [SerializeField] private List<TMP_Text> SymbolsText = new();

    [SerializeField] private Image comboAnimationImage;
    [SerializeField] private ImageAnimation bigWinStartAnimation;

    [SerializeField] private AudioController audioController;

    [SerializeField] private SlotBehaviour slotManager;

    [SerializeField] private SocketIOManager socketManager;

    private bool isMusic = true;
    private bool isSound = true;
    private bool isExit = false;
    private bool isMenu = false;
    internal bool isComboSpritesAnimating;


    [SerializeField] private Image targetImage; // Assign the Image in the Inspector

    private Tween ColorCycleTween;

    private void Start()
    {
        if (RaycastLayerButton) RaycastLayerButton.onClick.RemoveAllListeners();
        if (RaycastLayerButton) RaycastLayerButton.onClick.AddListener(() => CanCloseMenu());

        if (LBExit_Button) LBExit_Button.onClick.RemoveAllListeners();
        if (LBExit_Button) LBExit_Button.onClick.AddListener(delegate { ClosePopup(LBPopup_Object); });

        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.RemoveAllListeners();
        if (CloseDisconnect_Button) CloseDisconnect_Button.onClick.AddListener(CallOnExitFunction);

        if (Sound_Button) Sound_Button.onClick.RemoveAllListeners();
        if (Sound_Button) Sound_Button.onClick.AddListener(delegate
        {
            Debug.Log("Here");
            if (isSound)
            {
                SoundOnOFF(false);
            }
            else
            {
                SoundOnOFF(true);
            }
        });

        if (Music_Button) Music_Button.onClick.RemoveAllListeners();
        if (Music_Button) Music_Button.onClick.AddListener(delegate {

            if (isMusic)
            {
                MusicONOFF(false);
            }
            else
            {
                MusicONOFF(true);
            }
        });

        if (Quit_Button) Quit_Button.onClick.RemoveAllListeners();
        if (Quit_Button) Quit_Button.onClick.AddListener(OpenQuitPanel);

        if (QuitNo_Button) QuitNo_Button.onClick.RemoveAllListeners();
        if (QuitNo_Button) QuitNo_Button.onClick.AddListener(delegate { ClosePopup(QuitMenuObject); });

        if (QuitYes_Button) QuitYes_Button.onClick.RemoveAllListeners();
        if (QuitYes_Button) QuitYes_Button.onClick.AddListener(CallOnExitFunction);

        if (Paytable_Button) Paytable_Button.onClick.RemoveAllListeners();
        if (Paytable_Button) Paytable_Button.onClick.AddListener(OpenPaytablePanel);

        if (PaytableClose_Button) PaytableClose_Button.onClick.RemoveAllListeners();
        if (PaytableClose_Button) PaytableClose_Button.onClick.AddListener(delegate { ClosePopup(PaytableMenuObject); });

        if (PaytableClose_Button2) PaytableClose_Button2.onClick.RemoveAllListeners();
        if (PaytableClose_Button2) PaytableClose_Button2.onClick.AddListener(delegate { ClosePopup(PaytableMenuObject); });

        if (Menu_Button) Menu_Button.onClick.RemoveAllListeners();
        if (Menu_Button) Menu_Button.onClick.AddListener(delegate
        {
            if (!isMenu)
            {
                OpenCloseMenu(true);
            }
            else
            {
                OpenCloseMenu(false);
            }
        });

        if (Settings_Button) Settings_Button.onClick.RemoveAllListeners();
        if (Settings_Button) Settings_Button.onClick.AddListener(OpenSettingsPanel);

        if (SettingsQuit_Button) SettingsQuit_Button.onClick.RemoveAllListeners();
        if (SettingsQuit_Button) SettingsQuit_Button.onClick.AddListener(delegate { ClosePopup(Settings_Object); });

        if (PaytableLeft_Button) PaytableLeft_Button.onClick.RemoveAllListeners();
        if (PaytableLeft_Button) PaytableLeft_Button.onClick.AddListener(()=> ChangePage(false));

        if (PaytableRight_Button) PaytableRight_Button.onClick.RemoveAllListeners();
        if (PaytableRight_Button) PaytableRight_Button.onClick.AddListener(()=> ChangePage(true));
    }

    internal void CanCloseMenu()
    {
        if (isMenu)
        {
            OpenCloseMenu(false);
        }
    }

    private void ChangePage(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (IncDec)
        {
            if(PageIndex < GameRulesPages.Count - 1)
            {
                PageIndex++;
            }
            if(PageIndex == GameRulesPages.Count - 1)
            {
                if (PaytableRight_Button) PaytableRight_Button.interactable = false;
            }
            if(PageIndex > 0)
            {
                if(PaytableLeft_Button) PaytableLeft_Button.interactable = true;
            }
        }
        else
        {
            if(PageIndex > 0)
            {
                PageIndex--;
            }
            if(PageIndex == 0)
            {
                if (PaytableLeft_Button) PaytableLeft_Button.interactable = false;
            }
            if(PageIndex < GameRulesPages.Count - 1)
            {
                if (PaytableRight_Button) PaytableRight_Button.interactable = true;
            }
        }
        foreach(GameObject g in GameRulesPages)
        {
            g.SetActive(false);
        }
        if (GameRulesPages[PageIndex]) GameRulesPages[PageIndex].SetActive(true);
    }

    private void OpenCloseMenu(bool toggle)
    {
        if(audioController) audioController.PlayButtonAudio();
        if (toggle)
        {
            isMenu = true;
            if (Info_Button) Info_Button.gameObject.SetActive(true);
            if (Settings_Button) Settings_Button.gameObject.SetActive(true);

            DOTween.To(() => Info_BttnTransform.anchoredPosition, (val) => Info_BttnTransform.anchoredPosition = val, new Vector2(Info_BttnTransform.anchoredPosition.x - 125, Info_BttnTransform.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });

            DOTween.To(() => Settings_BttnTransform.anchoredPosition, (val) => Settings_BttnTransform.anchoredPosition = val, new Vector2(Settings_BttnTransform.anchoredPosition.x - 250, Settings_BttnTransform.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_BttnTransform);
            });
        }
        else
        {
            isMenu = false;
            DOTween.To(() => Info_BttnTransform.anchoredPosition, (val) => Info_BttnTransform.anchoredPosition = val, new Vector2(Info_BttnTransform.anchoredPosition.x + 125, Info_BttnTransform.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });

            DOTween.To(() => Settings_BttnTransform.anchoredPosition, (val) => Settings_BttnTransform.anchoredPosition = val, new Vector2(Settings_BttnTransform.anchoredPosition.x + 250, Settings_BttnTransform.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Settings_BttnTransform);
            });

            DOVirtual.DelayedCall(0.1f, () =>
            {
                if (Info_Button) Info_Button.gameObject.SetActive(false);
                if (Settings_Button) Settings_Button.gameObject.SetActive(false);
            });
        }
    }

    private void SoundOnOFF(bool state)
    {
        if (state)
        {
            isSound = true;
            audioController.ToggleMute(!state, "sound");
            DOTween.To(() => SoundToggle_RT.anchoredPosition, (val) => SoundToggle_RT.anchoredPosition = val, new Vector2(SoundToggle_RT.anchoredPosition.x + 108, SoundToggle_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });
        }
        else
        {
            isSound = false;
            audioController.ToggleMute(!state, "sound");
            DOTween.To(() => SoundToggle_RT.anchoredPosition, (val) => SoundToggle_RT.anchoredPosition = val, new Vector2(SoundToggle_RT.anchoredPosition.x - 108, SoundToggle_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });
        }
    }

    private void MusicONOFF(bool state)
    {
        if (state)
        {
            isMusic = true;
            audioController.ToggleMute(!state, "music");
            DOTween.To(() => MusicToggle_RT.anchoredPosition, (val) => MusicToggle_RT.anchoredPosition = val, new Vector2(MusicToggle_RT.anchoredPosition.x + 108, MusicToggle_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });
        }
        else
        {
            isMusic = false;
            audioController.ToggleMute(!state, "music");
            DOTween.To(() => MusicToggle_RT.anchoredPosition, (val) => MusicToggle_RT.anchoredPosition = val, new Vector2(MusicToggle_RT.anchoredPosition.x - 108, MusicToggle_RT.anchoredPosition.y), 0.1f).OnUpdate(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(Info_BttnTransform);
            });
        }
    }

    private void OpenSettingsPanel()
    {
        if (audioController) audioController.PlayButtonAudio();
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        if (Settings_Object) Settings_Object.SetActive(true);
        CanCloseMenu();
    }

    private void OpenQuitPanel()
    {
        if (audioController) audioController.PlayButtonAudio();
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
        if (QuitMenuObject) QuitMenuObject.SetActive(true);
        CanCloseMenu();
    }

    private void OpenPaytablePanel()
    {
        if (audioController) audioController.PlayButtonAudio();

        if (MainPopup_Object) MainPopup_Object.SetActive(true);

        PageIndex = 0;

        foreach(GameObject g in GameRulesPages)
        {
            g.SetActive(false);
        }

        GameRulesPages[0].SetActive(true);
        if(PaytableLeft_Button) PaytableLeft_Button.interactable = false;
        if(PaytableRight_Button) PaytableRight_Button.interactable = true;

        if (PaytableMenuObject) PaytableMenuObject.SetActive(true);

        CanCloseMenu();
    }

    internal void LowBalPopup()
    {
        CanCloseMenu();
        OpenPopup(LBPopup_Object);
    }

    internal void DisconnectionPopup(bool isReconnection)
    {
        if (!isExit)
        {
            CanCloseMenu();
            OpenPopup(DisconnectPopup_Object);
        }
    }

    internal void StartWinAnimation(Sprite winSprite, Sprite[] animationSprites)
    {
        ImageAnimation winImageAnimation = Win_Image.GetComponent<ImageAnimation>();
        if(winImageAnimation.currentAnimationState==ImageAnimation.ImageState.PLAYING){
            Win_Image.rectTransform.DOScale(0, 0.2f);
        }
        DOVirtual.DelayedCall(0.5f, ()=>{
            Win_Image.sprite = winSprite;
            winImageAnimation.textureArray.Clear();
            winImageAnimation.textureArray.AddRange(animationSprites);
            winImageAnimation.doLoopAnimation = true;
            winImageAnimation.StartAnimation();
            Win_Image.rectTransform.DOScale(1, 0.2f);
        });
    }

    internal void StopWinAnimation(){
        targetImage.DOFade(0, 0.5f).OnComplete(()=> {ColorCycleTween.Kill();});
        ImageAnimation winImageAnimation = Win_Image.GetComponent<ImageAnimation>();
        Win_Image.rectTransform.DOScale(0, 0.5f).OnComplete(()=> {winImageAnimation.StopAnimation();});
    }

    internal IEnumerator BigWinStartAnim(){
        bigWinStartAnimation.rendererDelegate.DOFade(1, 0.2f);
        bigWinStartAnimation.StartAnimation();
        yield return new WaitUntil(()=> bigWinStartAnimation.textureArray[^10]==bigWinStartAnimation.rendererDelegate.sprite);
        CycleColors();
        yield return new WaitUntil(()=> bigWinStartAnimation.textureArray[^5]==bigWinStartAnimation.rendererDelegate.sprite);
        bigWinStartAnimation.rendererDelegate.DOFade(0, 0.2f);
        yield return new WaitUntil(()=> bigWinStartAnimation.textureArray[^1]==bigWinStartAnimation.rendererDelegate.sprite);
        bigWinStartAnimation.StopAnimation();
    }

    private void CycleColors()
    {
        targetImage.DOFade(1, 0.5f);
        // Tween through the hue range
        ColorCycleTween = DOTween.To(() => 0f, x =>
        {
            // Convert the hue to a color and apply it
            Color newColor = Color.HSVToRGB(x, 1f, 1f);
            newColor.a = 0.4f;
            targetImage.color = newColor;
        }, 1f, 5f)
        .SetEase(Ease.Linear) // Smooth transition
        .SetLoops(-1, LoopType.Restart); // Infinite loop
    }

    internal void ADfunction()
    {
        OpenPopup(ADPopup_Object); 
    }

    internal void InitialiseUIData(string SupportUrl, string AbtImgUrl, string TermsUrl, string PrivacyUrl, Paylines symbolsText)
    {
        StartCoroutine(DownloadImage(AbtImgUrl));
        PopulateSymbolsPayout(symbolsText);
    }

    private void PopulateSymbolsPayout(Paylines paylines)
    {
        for (int i = 0; i < SymbolsText.Count; i++)
        {
            string text = null;
            if (paylines.symbols[i].Multiplier[0][0] != 0)
            {
                text += paylines.symbols[i].Multiplier[0][0].ToString();
            }
            if (paylines.symbols[i].Multiplier[1][0] != 0)
            {
                text += "\n" + paylines.symbols[i].Multiplier[1][0].ToString();
            }
            if (paylines.symbols[i].Multiplier[2][0] != 0)
            {
                text += "\n" + paylines.symbols[i].Multiplier[2][0].ToString();
            }
            if (SymbolsText[i]) SymbolsText[i].text = text;
        }
    }
    internal IEnumerator AnimateSprite(Sprite sprite)
    {
        isComboSpritesAnimating = true;
        comboAnimationImage.sprite = sprite;

        float randomZRotation = UnityEngine.Random.Range(-15f, 15f);
        comboAnimationImage.rectTransform.localEulerAngles = new Vector3(0, 0, randomZRotation);

        // Scale up quickly.
        comboAnimationImage.rectTransform.localScale = Vector3.zero;
        comboAnimationImage.color = new(1, 1, 1, 1);
        comboAnimationImage.rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

        // Wait for half a second.
        yield return new WaitForSeconds(0.5f);

        // Fade out the image.
        comboAnimationImage.DOFade(0, 0.3f).WaitForCompletion();
        yield return comboAnimationImage.rectTransform.DOScale(Vector3.one*1.5f, 0.3f).WaitForCompletion();
        isComboSpritesAnimating = false;
    }

    private void CallOnExitFunction()
    {
        isExit = true;
        audioController.PlayButtonAudio();
        slotManager.CallCloseSocket();
    }

    private void OpenPopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();

        if (Popup) Popup.SetActive(true);
        if (MainPopup_Object) MainPopup_Object.SetActive(true);
    }

    private void ClosePopup(GameObject Popup)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (Popup) Popup.SetActive(false);
        if (!DisconnectPopup_Object.activeSelf) 
        {
            if (MainPopup_Object) MainPopup_Object.SetActive(false);
        }
    }

    private void UrlButtons(string url)
    {
        Application.OpenURL(url);
    }

    private IEnumerator DownloadImage(string url)
    {
        // Create a UnityWebRequest object to download the image
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        // Wait for the download to complete
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            // Apply the sprite to the target image
            //AboutLogo_Image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Error downloading image: " + request.error);
        }
    }
}
