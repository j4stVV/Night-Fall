using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver, 
        LevelUp
    }

    public GameState currentState;
    public GameState previousState;

    [Header("Damage Text Settings")]
    public Canvas damageTextCanvas;
    public float textFontSize = 20;
    public TMP_FontAsset textFont;
    public Camera referenceCamera;

    [Header("Screens")]
    public GameObject pauseScreen;
    public GameObject resultScreen;
    public GameObject levelUpScreen;
    int stackedLevelUp = 0;

    [Header("Current Stat Display")]
    public TextMeshProUGUI currentHealthDisplay;
    public TextMeshProUGUI currentRecoveryDisplay;
    public TextMeshProUGUI currentMoveSpeedDisplay;
    public TextMeshProUGUI currentMightDisplay;
    public TextMeshProUGUI currentProjectileSpeedDisplay;
    public TextMeshProUGUI currentMagnetDisplay;

    [Header("Result Screen Displays")]
    public Image chosenCharacterImage;
    public TextMeshProUGUI chosenCharacterName;
    public TextMeshProUGUI levelReachedDisplay;
    public TextMeshProUGUI timeSurvivedDisplay;
    public List<Image> chosenWeaponsUI = new List<Image>(6);
    public List<Image> chosenPassiveItemsUI = new List<Image>(6);

    [Header("Stopwatch")]
    public float timeLimit; //The time limit in seconds;
    float stopwatchTime;
    public TextMeshProUGUI stopwatchDisplay;

    public GameObject playerObject;

    public bool isGameOver { get { return currentState == GameState.GameOver; } }

    public bool choosingUpgrade { get { return currentState == GameState.LevelUp; }}

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("EXTRA" + this + "DELETED");
            Destroy(gameObject);
        }
        DisableScreens();    
    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Gameplay:
                CheckForPauseAndResume();
                UpdateStopwatch();
                break;

            case GameState.Paused:
                CheckForPauseAndResume();
                break;

            case GameState.GameOver:
                break;
            case GameState.LevelUp:
                break;
            default:
                break;
        }    
    }

    IEnumerator GenerateFloatingTextCoroutine(string text, Transform target, float duration = 1f, float speed = 50f)
    {
        //Generating the floating text
        GameObject textObj = new GameObject("Damage Floating Text");
        RectTransform rect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI tmPro = textObj.AddComponent<TextMeshProUGUI>();
        tmPro.text = text;
        tmPro.horizontalAlignment = HorizontalAlignmentOptions.Center;
        tmPro.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmPro.fontSize = textFontSize;
        if (textFont) tmPro.font = textFont;
        rect.position = referenceCamera.WorldToScreenPoint(target.position);

        //Makes sure this is destroyed after the duration finishes
        Destroy(textObj, duration);

        textObj.transform.SetParent(Instance.damageTextCanvas.transform);
        textObj.transform.SetSiblingIndex(0);

        WaitForEndOfFrame w = new WaitForEndOfFrame();
        float t = 0f;
        float yOffset = 0f;
        Vector3 lastKnowPosition = target.position;
        while (t < duration)
        {
            if (!rect) break;

            tmPro.color = new Color(tmPro.color.r, tmPro.color.g, tmPro.color.b, 1 - t / duration);

            if (target)
            {
                lastKnowPosition = target.position;
            }

            yOffset += speed * Time.deltaTime;
            rect.position = referenceCamera.WorldToScreenPoint(lastKnowPosition + new Vector3(0, yOffset));

            yield return w;
            t += Time.deltaTime;
        }

    }
    public static void GenerateFloatingText(string text, Transform targer, float duration = 1f, float speed = 1f)
    {
        //If the canvas is not set, end the function so we don't generate any floating text
        if (!Instance.damageTextCanvas) return;

        if (!Instance.referenceCamera) Instance.referenceCamera = Camera.main;

        Instance.StartCoroutine(Instance.GenerateFloatingTextCoroutine(text, targer, duration, speed));
    }
    public void ChangeState(GameState newState)
    {
        previousState = currentState;
        currentState = newState;
    }

    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            ChangeState(GameState.Paused);
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
            Debug.Log("Game Paused");
        }
        
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            Time.timeScale = 1;
            pauseScreen.SetActive(false);
            Debug.Log("Game is resumed");
        } 
    }

    void CheckForPauseAndResume()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
            else PauseGame();
        }
    }

    void DisableScreens()
    {
        pauseScreen.SetActive(false);       
        resultScreen.SetActive(false);
        levelUpScreen.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        timeSurvivedDisplay.text = stopwatchDisplay.text;
        stopwatchDisplay.enabled = false;
        ChangeState(GameState.GameOver);
        DisplayResult();
    }

    void DisplayResult()
    {
        resultScreen.SetActive(true);
    }

    public void AssignChosenCharacterUI(CharacterData chosenCharacterData)
    {
        chosenCharacterImage.sprite = chosenCharacterData.Icon;
        chosenCharacterName.text = chosenCharacterData.Name;
    }

    public void AssignLevelReachedUI(int levelReachedData)
    {
        levelReachedDisplay.text = levelReachedData.ToString();
    }

    public void AssignChosenWeaponsAndPassiveItemsUI(List<PlayerInventory.Slot> chosenWeaponsData, List<PlayerInventory.Slot> chosenPassiveItemsData)
    {
        if (chosenWeaponsData.Count != chosenWeaponsUI.Count || chosenPassiveItemsData.Count != chosenPassiveItemsUI.Count)
        {
            Debug.Log("Chosen weapons and passive items data lists have different lengths");
            return;
        }

        //Assign chosen weapons data to chosen weapons UI
        for (int i = 0; i < chosenWeaponsUI.Count; i++)
        {
            if (chosenWeaponsData[i].image.sprite)
            {
                chosenWeaponsUI[i].enabled = true;
                chosenWeaponsUI[i].sprite = chosenWeaponsData[i].image.sprite;
            }
            else
            {
                chosenWeaponsUI[i].enabled = false;
            }
        }
        //Assign chosen passive items data to chosen passive items UI
        for (int i = 0; i < chosenPassiveItemsUI.Count; i++)
        {
            if (chosenPassiveItemsData[i].image.sprite)
            {
                chosenPassiveItemsUI[i].enabled = true;
                chosenPassiveItemsUI[i].sprite = chosenPassiveItemsData[i].image.sprite;
            }
            else
            {
                chosenPassiveItemsUI[i].enabled = false;
            }
        }
    }

    void UpdateStopwatch()
    {
        stopwatchTime += Time.deltaTime;

        UpdateStopwatchDisplay();

        if (stopwatchTime >= timeLimit)
        {
            playerObject.SendMessage("Death");
        }
    }

    void UpdateStopwatchDisplay()
    {
        int minutes = Mathf.FloorToInt(stopwatchTime / 60);
        int seconds = Mathf.FloorToInt(stopwatchTime % 60);

        stopwatchDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    public void StartLevelUp()
    {
        ChangeState(GameState.LevelUp);

        if (levelUpScreen.activeSelf) stackedLevelUp++;
        else
        {
            Time.timeScale = 0f;
            levelUpScreen.SetActive(true);
            playerObject.SendMessage("RemoveAndApplyUpgrades");
        }
    }

    public void EndLevelUp()
    {
        Time.timeScale = 1f;
        levelUpScreen.SetActive(false);
        ChangeState(GameState.Gameplay);

        if (stackedLevelUp > 0)
        {
            stackedLevelUp--;
            StartLevelUp();
        }
    }
}