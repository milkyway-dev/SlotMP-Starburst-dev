using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using System.Xml.Serialization;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] myImages;  //images taken initially

    [Header("Slot Images")]
    [SerializeField] private List<SlotImage> images;     //class to store total images
    [SerializeField] private List<SlotImage> Tempimages;     //class to store the result matrix
    [SerializeField] private List<BoxScript> TempBoxScripts;
    [SerializeField] private List<Sprite> Box_Sprites;

    [Header("Slots Transforms")]
    [SerializeField] private Transform[] Slot_Transform;

    private Dictionary<int, string> y_string = new Dictionary<int, string>();

    [Header("Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button TotalBetPlus_Button;
    [SerializeField] private Button TotalBetMinus_Button;
    [SerializeField] private Button LineBetPlus_Button;
    [SerializeField] private Button LineBetMinus_Button;
    [SerializeField] private Button SkipWinAnimation_Button;
    [SerializeField] private Button BonusSkipWinAnimation_Button;

    [Header("Animated Sprites")]
    [SerializeField] private Sprite[] BlueGem_Sprites;
    [SerializeField] private Sprite[] GreenGem_Sprites;
    [SerializeField] private Sprite[] PurpleGem_Sprites;
    [SerializeField] private Sprite[] YellowGem_Sprites;
    [SerializeField] private Sprite[] OrangeGem_Sprites;
    [SerializeField] private Sprite[] Seven_Sprites;
    [SerializeField] private Sprite[] Bar_Sprites;
    [SerializeField] private Sprite[] Rainbow_Sprites;


    [Header("Miscellaneous UI")]
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private TMP_Text LineBet_text;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private TMP_Text BigWin_Text;
    [SerializeField] private TMP_Text BonusWin_Text;
    [SerializeField] private Image[] Fill_Images;

    [Header("Audio Management")]
    [SerializeField] private AudioController audioController;

    [SerializeField] private UIManager uiManager;

    [Header("BonusGame Popup")]
    [SerializeField] private BonusController _bonusManager;

    [Header("Free Spins Board")]
    [SerializeField] private GameObject FSBoard_Object;
    [SerializeField] private TMP_Text FSnum_text;

    int tweenHeight = 0;  //calculate the height at which tweening is done

    [SerializeField] private PayoutCalculation PayCalculator;

    private List<Tweener> alltweens = new List<Tweener>();

    private Tweener WinTween = null;

    [SerializeField] private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField] private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Coroutine BoxAnimRoutine = null;

    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool WinAnimationFin = true;

    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;

    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    protected int Lines = 20;
    [SerializeField] private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;          //number of columns
    private ImageAnimation BaseImageAnimation;
    private bool isBaseAnimationRunning;

    private void Start()
    {
        IsAutoSpin = false;

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            StartSlots();
        });

        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.RemoveAllListeners();
        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(true);
        });

        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.RemoveAllListeners();
        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(false);
        });

        if (LineBetPlus_Button) LineBetPlus_Button.onClick.RemoveAllListeners();
        if (LineBetPlus_Button) LineBetPlus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(true);
        });

        if (LineBetMinus_Button) LineBetMinus_Button.onClick.RemoveAllListeners();
        if (LineBetMinus_Button) LineBetMinus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(false);
        });

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            AutoSpin();
        });

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            StopAutoSpin();
        });

        if (SkipWinAnimation_Button) SkipWinAnimation_Button.onClick.RemoveAllListeners();
        if (SkipWinAnimation_Button) SkipWinAnimation_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            StopGameAnimation();
	    });

        if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.onClick.RemoveAllListeners();
        if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            StopGameAnimation();
        });

        if (FSBoard_Object) FSBoard_Object.SetActive(false);

        tweenHeight = (13 * IconSizeFactor) - 280;
        //Debug.Log("Tween Height: " + tweenHeight);
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {
            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        { 
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            if(BoxAnimRoutine!=null && !WinAnimationFin)
            {
                yield return new WaitUntil(() => WinAnimationFin);
                StopGameAnimation();
            }

            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(.5f);
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        if (AutoSpinStop_Button) AutoSpinStop_Button.interactable = false;
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            AutoSpinStop_Button.interactable = true;
            tweenroutine = null;
            AutoSpinRoutine = null;
            IsAutoSpin = false;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = spins.ToString();
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        int j = spinchances;
        while (i < spinchances)
        {
            j -= 1;
            if (FSnum_text) FSnum_text.text = j.ToString();

            StartSlots(false);

            yield return tweenroutine;
            yield return new WaitForSeconds(.5f);
            i++;
        }
        ToggleButtonGrp(true);
        IsFreeSpin = false;
        StartCoroutine(_bonusManager.BonusGameEndRoutine());
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            SlotStart_Button.interactable = true;
        }
    }

    #region LinesCalculation

    //Destroy Static Lines from button hovers
    internal void DestroyStaticLine()
    {
        PayCalculator.ResetStaticLine();
    }
    internal void GenerateStaticLine(TMP_Text LineID_Text)
    {
        DestroyStaticLine();
        int LineID = 1;
        try
        {
            LineID = int.Parse(LineID_Text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Exception while parsing " + e.Message);
        }
        List<int> y_points = null;
        if(y_string.Count>0){
            y_points = y_string[LineID]?.Split(',')?.Select(Int32.Parse)?.ToList();
            PayCalculator.GeneratePayoutLinesBackend(y_points, y_points.Count, true);
        }
    }
    internal void FetchLines(string LineVal, int count)
    {
        y_string.Add(count + 1, LineVal);
    }
    #endregion

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            BetCounter++;
            if (BetCounter >= SocketManager.initialData.Bets.Count)
            {
                BetCounter = 0; // Loop back to the first bet
            }
        }
        else
        {
            BetCounter--;
            if (BetCounter < 0)
            {
                BetCounter = SocketManager.initialData.Bets.Count - 1; // Loop to the last bet
            }
        }
        SetFillImage();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
    }

    private void SetFillImage(){
        int fillBetCounter = BetCounter + 1;
        float fillAmount = Mathf.Clamp(1f/SocketManager.initialData.Bets.Count * fillBetCounter, 0, 1);
        // Debug.Log(fillAmount);
        foreach(Image i in Fill_Images){
            i.fillAmount= fillAmount;
        }
    } 

    #region InitialFunctions
    internal void ShuffleInitialMatrix()
    {
        for (int i = 0; i < images.Count; i++)
        {
            for (int j = 0; j < images[i].slotImages.Count; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, myImages.Length-1);
                images[i].slotImages[j].sprite = myImages[randomIndex];
                
                if (j >= 8 && j <= 10)
                {
                    var imageAnimation = images[i].slotImages[j].GetComponent<ImageAnimation>();
                    if (imageAnimation != null)
                    {
                        PopulateAnimationSprites(imageAnimation, randomIndex);
                    }
                }
            }
        }

        StartCoroutine(AnimationLoop());
    }

    private IEnumerator AnimationLoop()
    {
        while (true)
        {
            if (!IsSpinning && !IsAutoSpin && !IsFreeSpin && !isBaseAnimationRunning)
            {
                if (BaseImageAnimation == null)
                {
                    int randomColumn = UnityEngine.Random.Range(0, 5);
                    int randomRow = UnityEngine.Random.Range(0, 3);

                    BaseImageAnimation = Tempimages[randomColumn].slotImages[randomRow].GetComponent<ImageAnimation>();
                    
                    if (BaseImageAnimation != null)
                    {
                        StartCoroutine(BaseAnimation());
                    }
                }
            }

            yield return new WaitForSeconds(0.1f); // Add a delay to prevent overloading the game loop
        }
    }

    private IEnumerator BaseAnimation()
    {
        isBaseAnimationRunning = true;

        if (BaseImageAnimation != null)
        {
            BaseImageAnimation.StartAnimation();

            if (BaseImageAnimation.textureArray.Count > 0)
            {
                yield return new WaitUntil(() => BaseImageAnimation.textureArray[^1] == BaseImageAnimation.rendererDelegate.sprite);
            }

            BaseImageAnimation.StopAnimation();
            BaseImageAnimation = null;
        }

        isBaseAnimationRunning = false;
    }

    void StopBaseAnimation()
    {
        if (BaseImageAnimation != null)
        {
            StopCoroutine(BaseAnimation());
            BaseImageAnimation.StopAnimation();
            BaseImageAnimation = null;
            isBaseAnimationRunning = false;
        }
    }

    internal void SetInitialUI()
    {
        BetCounter = 0;
        SetFillImage();
        if (LineBet_text) LineBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalBet_text) TotalBet_text.text = (SocketManager.initialData.Bets[BetCounter] * Lines).ToString();
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter] * Lines;
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        Debug.Log("Called val: " +val);
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 0:
                for(int i=0; i < PurpleGem_Sprites.Length; i++)
                {
                    animScript.textureArray.Add(PurpleGem_Sprites[i]);
                }
                animScript.AnimationSpeed = 9f;
                break;

            case 1:
                for(int i=0;i<BlueGem_Sprites.Length; i++)
                {
                    animScript.textureArray.Add(BlueGem_Sprites[i]);
                }
                animScript.AnimationSpeed = 9f;
                break;
            case 2:
                for(int i=0;i<OrangeGem_Sprites.Length; i++)
                {
                    animScript.textureArray.Add(OrangeGem_Sprites[i]);
                }
                animScript.AnimationSpeed = 9f;
                break;
            case 3:
                for(int i=0;i<GreenGem_Sprites.Length; i++)
                {
                    animScript.textureArray.Add(GreenGem_Sprites[i]);
                }
                animScript.AnimationSpeed = 9f;
                break;
            case 4:
                for(int i=0;i<YellowGem_Sprites.Length; i++)
                {
                    animScript.textureArray.Add(YellowGem_Sprites[i]);
                }
                animScript.AnimationSpeed = 9f;
                break;
            case 5:
                for(int i=0;i<Seven_Sprites.Length; i++)
                {
                    animScript.textureArray.Add(Seven_Sprites[i]);
                }
                animScript.AnimationSpeed = 9f;
                break;
            case 6:
                for(int i=0;i<Bar_Sprites.Length; i++)
                {
                    animScript.textureArray.Add(Bar_Sprites[i]);
                }
                animScript.AnimationSpeed = 9f;
                break;
            case 7:
                for(int i=0;i<Rainbow_Sprites.Length; i++)
                {
                    animScript.textureArray.Add(Rainbow_Sprites[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;
        }
    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }
        //WinningsAnim(false);
        if (SlotStart_Button) SlotStart_Button.interactable = false;

        StopGameAnimation();
        
        //PayCalculator.ResetLines();
        tweenroutine = StartCoroutine(TweenRoutine());

        if (TotalWin_text) TotalWin_text.text = "0.00";
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine(bool bonus = false)
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin) // Check if balance is sufficient to place the bet
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            yield break;
        }

        CheckSpinAudio = true;
        IsSpinning = true;
        ToggleButtonGrp(false);

        for (int i = 0; i < numberOfSlots; i++) // Initialize tweening for slot animations
        {
            InitializeTweening(Slot_Transform[i]);
            // if (!bonus) yield return new WaitForSeconds(0.1f);
        }

        // if (!bonus) // Deduct balance if not a bonus
        // {
        //     BalanceDeduction();
        // }
        BalanceDeduction(); //test


        // SocketManager.AccumulateResult(BetCounter);
        // yield return new WaitUntil(() => SocketManager.isResultdone);

        // for (int j = 0; j < SocketManager.resultData.ResultReel.Count; j++) // Update slot images based on the results
        // {
        //     List<int> resultnum = SocketManager.resultData.FinalResultReel[j]?.Split(',')?.Select(Int32.Parse)?.ToList();
        //     for (int i = 0; i < 5; i++)
        //     {
        //         if (images[i].slotImages[images[i].slotImages.Count - 5 + j]) images[i].slotImages[images[i].slotImages.Count - 5 + j].sprite = myImages[resultnum[i]];
        //         PopulateAnimationSprites(images[i].slotImages[images[i].slotImages.Count - 5 + j].gameObject.GetComponent<ImageAnimation>(), resultnum[i]);
        //     }
        // }

        yield return new WaitForSeconds(2f);
        // yield return new WaitForSeconds(.5f);

        for (int i = 0; i < numberOfSlots; i++) // Stop tweening for each slot
        {
            yield return StopTweening(Slot_Transform[i], i);
        }

        yield return new WaitForSeconds(1f);
        KillAllTweens();

        // CheckPayoutLineBackend(SocketManager.resultData.linesToEmit, SocketManager.resultData.FinalsymbolsToEmit, SocketManager.resultData.jackpot);

        // if (SocketManager.playerdata.currentWining>0) WinningsTextAnimation(bonus); // Trigger winnings animation if applicable

        // CheckPopups = true;

        // if (SocketManager.resultData.jackpot > 0) // Check for jackpot or winnings popups
        // {
        //     uiManager.PopulateWin(4); 
        // }
        // else if(!bonus)
        // {
        //     CheckWinPopups();
        // }
        // else
        // {
        //     CheckPopups = false;
        // }

        // if(SocketManager.playerdata.currentWining <= 0 && SocketManager.resultData.jackpot <= 0 && !SocketManager.resultData.freeSpins.isNewAdded)
        // {
        //     audioController.PlayWLAudio("lose");
        // }

        // currentBalance = SocketManager.playerdata.Balance;
        // yield return new WaitUntil(() => !CheckPopups);

        // if (IsFreeSpin && BoxAnimRoutine != null && !WinAnimationFin) // Waits for winning payline animation to finish when triggered bonus
        // {
        //     yield return new WaitUntil(() => WinAnimationFin);
        //     //yield return new WaitForSeconds(0.5f);
        //     StopGameAnimation();
        // }

        // if (SocketManager.resultData.freeSpins.isNewAdded)
        // {
        //     Debug.Log(IsFreeSpin ? "Bonus In Bonus" : "First Time Bonus");

        //     yield return new WaitForSeconds(1.5f);

        //     if (BoxAnimRoutine != null && !WinAnimationFin)
        //     {
        //         yield return new WaitUntil(() => WinAnimationFin);
        //         StopGameAnimation();
        //     }

        //     yield return new WaitForSeconds(1f);

        //     if (!IsFreeSpin)
        //     {
        //         _bonusManager.StartBonus(SocketManager.resultData.freeSpins.count);
        //     }
        //     else
        //     {
        //         IsFreeSpin = false;
        //         yield return StartCoroutine(_bonusManager.BonusInBonus());
        //     }

        //     if (IsAutoSpin)
        //     {
        //         IsSpinning = false;
        //         StopAutoSpin();
        //     }

        // }

        if (!IsAutoSpin && !IsFreeSpin) // Reset spinning state and toggle buttons
        {
            ToggleButtonGrp(true);
            IsSpinning = false;
        }
        else
        {
            IsSpinning = false;
            yield return new WaitForSeconds(2f);
        }
    }
    #endregion

    internal void CheckWinPopups()
    {
        if (SocketManager.resultData.WinAmout >= currentTotalBet * 5 && SocketManager.resultData.WinAmout < currentTotalBet * 10)
        {
            uiManager.PopulateWin(1);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        {
            uiManager.PopulateWin(2);
        }
        else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15)
        {
            uiManager.PopulateWin(3);
        }
        else
        {
            CheckPopups = false;
        }
    }

    private void WinningsTextAnimation(bool bonus = false)
    {
        double winAmt = 0;
        double currentWin = 0;

        double currentBal = 0;
        double Balance = 0;

        double BonusWinAmt = 0;
        double currentBonusWinnings = 0;

        if (bonus)
        {
            try
            {
                BonusWinAmt = double.Parse(SocketManager.playerdata.currentWining.ToString("f2"));
                currentBonusWinnings = double.Parse(BonusWin_Text.text);
            }
            catch (Exception e)
            {
                Debug.Log("Error while conversion " + e.Message);
            }
        }
        try
        {
            winAmt = double.Parse(SocketManager.playerdata.currentWining.ToString("f2"));
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            currentBal = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            Balance = double.Parse(SocketManager.playerdata.Balance.ToString("f2"));
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            currentWin = double.Parse(TotalWin_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        if (bonus)
        {
            double CurrTotal = BonusWinAmt + currentBonusWinnings;
            DOTween.To(() => currentBonusWinnings, (val) => currentBonusWinnings = val, CurrTotal, 0.8f).OnUpdate(() =>
            {
                if (BonusWin_Text) BonusWin_Text.text = currentBonusWinnings.ToString("f2");
            });

            double start = 0;
            DOTween.To(() => start, (val) => start = val, BonusWinAmt, 0.8f).OnUpdate(() =>
            {
                if (BigWin_Text) BigWin_Text.text = start.ToString("f2");
            });
        }
        else
        {
            DOTween.To(() => currentWin, (val) => currentWin = val, winAmt, 0.8f).OnUpdate(() =>
            {
                if (TotalWin_text) TotalWin_text.text = currentWin.ToString("f2");
                if (BigWin_Text) BigWin_Text.text = currentWin.ToString("f2");
            });

            DOTween.To(() => currentBal, (val) => currentBal = val, Balance, 0.8f).OnUpdate(() =>
            {
                if (Balance_text) Balance_text.text = currentBal.ToString("f2");
            });
        }
    }

    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        List<int> points_anim = null;
        if (LineId.Count > 0 || points_AnimString.Count > 0)
        {
            if (jackpot > 0)
            {
                if (audioController) audioController.PlayWLAudio("megaWin");
                for (int i = 0; i < Tempimages.Count; i++)
                {
                    for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
                    {
                        StartGameAnimation(Tempimages[i].slotImages[k].gameObject, TempBoxScripts[i].boxScripts[k]);
                    }
                }
            }
            else
            {
                if (audioController) audioController.PlayWLAudio("win");
                for (int i = 0; i < points_AnimString.Count; i++)
                {
                    points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

                    for (int k = 0; k < points_anim.Count; k++)
                    {
                        if (points_anim[k] >= 10)
                        {
                            StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject, TempBoxScripts[(points_anim[k] / 10) % 10].boxScripts[points_anim[k] % 10]);
                        }
                        else
                        {
                            StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject, TempBoxScripts[0].boxScripts[points_anim[k]]);
                        }
                    }
                }
            }

            if (!SocketManager.resultData.freeSpins.isNewAdded)
            {
                if (SkipWinAnimation_Button) SkipWinAnimation_Button.gameObject.SetActive(true);
            }

            if (IsFreeSpin && !SocketManager.resultData.freeSpins.isNewAdded)
            {
                if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.gameObject.SetActive(true);
            }
        }
        else
        {
            if (audioController) audioController.StopWLAaudio();
        }

        if (LineId.Count > 0)
        {
            BoxAnimRoutine = StartCoroutine(BoxRoutine(LineId));
        }

        CheckSpinAudio = false;
    }

    private IEnumerator BoxRoutine(List<int> LineIDs)
    {
        yield return null;
        // WinAnimationFin = false;
        // while (true)
        // {
        //     for (int i = 0; i < LineIDs.Count; i++)
        //     {
        //         PayCalculator.GeneratePayoutLinesBackend(LineIDs[i]);
        //         PayCalculator.DontDestroyLines.Add(LineIDs[i]);
        //         for (int s = 0; s < 5; s++)
        //         {
        //             if (TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isAnim)
        //             {
        //                 TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].SetBG(Box_Sprites[LineIDs[i]]);
        //             }
        //         }
        //         if (LineIDs.Count < 2)
        //         {
        //             WinAnimationFin = true;
        //             yield return new WaitForSeconds(2f);
        //             yield break;
        //         }
        //         yield return new WaitForSeconds(2f);
        //         for (int s = 0; s < 5; s++)
        //         {
        //             if (TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].isAnim)
        //             {
        //                 TempBoxScripts[s].boxScripts[SocketManager.LineData[LineIDs[i]][s]].ResetBG();
        //             }
        //         }
        //         PayCalculator.DontDestroyLines.Clear();
        //         PayCalculator.DontDestroyLines.TrimExcess();
        //         PayCalculator.ResetStaticLine();
        //     }
        //     for (int i = 0; i < LineIDs.Count; i++)
        //     {
        //         PayCalculator.GeneratePayoutLinesBackend(LineIDs[i]);
        //         PayCalculator.DontDestroyLines.Add(LineIDs[i]);
        //     }
        //     yield return new WaitForSeconds(3f);
        //     PayCalculator.DontDestroyLines.Clear();
        //     PayCalculator.DontDestroyLines.TrimExcess();
        //     PayCalculator.ResetStaticLine();
        //     WinAnimationFin = true;
        // }
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    void ToggleButtonGrp(bool toggle)
    {
        if (SlotStart_Button) SlotStart_Button.interactable = toggle;
        if (AutoSpin_Button && !IsAutoSpin) AutoSpin_Button.interactable = toggle;
        if (BetCounter != 0)
        {
            if (LineBetMinus_Button) LineBetMinus_Button.interactable = toggle;
            if (TotalBetMinus_Button) TotalBetMinus_Button.interactable = toggle;
        }
        if(BetCounter < SocketManager.initialData.Bets.Count - 1)
        {
            if (LineBetPlus_Button) LineBetPlus_Button.interactable = toggle;
            if (TotalBetPlus_Button) TotalBetPlus_Button.interactable = toggle;
        }
    }

    //Start the icons animation
    private void StartGameAnimation(GameObject animObjects, BoxScripting boxscript)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        if (temp.textureArray.Count > 0)
        {
            temp.StartAnimation();
            TempList.Add(temp);
        }
        boxscript.isAnim = true;
    }

    //Stop the icons animation
    internal void StopGameAnimation()
    {
        if (BoxAnimRoutine != null)
        {
            StopCoroutine(BoxAnimRoutine);
            BoxAnimRoutine = null;
            WinAnimationFin = true;
        }

        // if (TempBoxScripts.Count > 0)
        // {
        //     for (int i = 0; i < TempBoxScripts.Count; i++)
        //     {
        //         foreach (BoxScripting b in TempBoxScripts[i].boxScripts)
        //         {
        //             b.isAnim = false;
        //             b.ResetBG();
        //         }
        //     }
        // }
        
        if (SkipWinAnimation_Button) SkipWinAnimation_Button.gameObject.SetActive(false);
        if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.gameObject.SetActive(false);

        if (TempList.Count > 0)
        {
            for (int i = 0; i < TempList.Count; i++)
            {
                TempList[i].StopAnimation();
            }
            TempList.Clear();
            TempList.TrimExcess();
        }

        // PayCalculator.DontDestroyLines.Clear();
        // PayCalculator.DontDestroyLines.TrimExcess();
        PayCalculator.ResetStaticLine();
    }

    #region TweeningCode
    private void InitializeTweening(Transform slotTransform)
    {
        Tweener tweener = null;
        slotTransform.DOLocalMoveY(-3221f, .7f)
        .SetEase(Ease.InBack)
        .OnComplete(()=> {
            slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, -959f);

            tweener = slotTransform.DOLocalMoveY(-3221f, .7f)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
            alltweens.Add(tweener);
        });
    }

    private IEnumerator StopTweening(Transform slotTransform, int index)
    {
        bool IsRegister = false;
        yield return alltweens[index].OnStepComplete(delegate { IsRegister = true; });
        yield return new WaitUntil(() => IsRegister);

        alltweens[index].Kill();
        slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, -1372f);
        alltweens[index] = slotTransform.DOLocalMoveY(-1691f+323.195f, .7f).SetEase(Ease.OutSine).OnComplete( ()=> {alltweens[index].Kill();} );
        if (audioController) audioController.PlayWLAudio("spinStop");
    }


    private void KillAllTweens()
    {
        for (int i = 0; i < numberOfSlots; i++)
        {
            alltweens[i].Kill();
        }
        alltweens.Clear();

    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}

[Serializable]
public class BoxScript
{
    public List<BoxScripting> boxScripts = new List<BoxScripting>(10);
}