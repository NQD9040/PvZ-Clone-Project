using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class PlantSlots : MonoBehaviour
{
    public GameObject plantslot;
    public GameObject[] slot;
    public GameObject selectedPlant;
    public GameObject selectedPlantCard;
    public Image plantFollowImage;
    private LevelData levelData;
    private GameManager gameManager;
    private Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        // Wait for the pre-game chooser to confirm before creating any plant cards.
        while (LevelManager.Instance == null ||
               !PlantSelectionSession.IsConfirmedFor(LevelManager.Instance.currentLevel))
            yield return null;

        gameManager = FindAnyObjectByType<GameManager>();
        mainCamera = Camera.main;
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager is missing; plant cards cannot be loaded.", this);
            yield break;
        }
        levelData = LevelManager.Instance.LoadLevel(LevelManager.Instance.currentLevel);
        if (levelData == null || levelData.plantCards == null)
        {
            Debug.LogError("Plant card data is missing for the current level.", this);
            yield break;
        }
        // Compatibility path for scenes where the pregame controller is disabled or absent.
        if (transform.childCount == 0)
        {
            foreach (GameObject plantCard in PlantSelectionSession.GetSelectedCards(levelData.levelNumber))
                if (plantCard != null) Instantiate(plantCard, transform, false);
        }
        // PlantSelectionController has already moved the chosen card instances into this grid.
        RefreshSlotReferences();
        if (plantFollowImage != null) plantFollowImage.gameObject.SetActive(false);
    }

    public void RefreshSlotReferences()
    {
        int childCount = transform.childCount;
        slot = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
            slot[i] = transform.GetChild(i).gameObject;
    }

    public void GetSelectedPlant(GameObject plant, GameObject plantCard)
    {
        if (plant != null)
        {
            if (gameManager == null) return;
            float sunAmount = gameManager.sunAmount;
            float plantCost = plant.GetComponent<Plant>().GetCost();
            if (plantCost > sunAmount)
            {
                Debug.Log("Not enough sun to select " + plant.name);
                SoundManager.instance.PlaySound(SoundManager.instance.canNotPick);
                DisableFollowImage();
                return;
            }
            selectedPlant = plant;
            selectedPlantCard = plantCard;
            SoundManager.instance.PlaySound(SoundManager.instance.plantPick);
            Debug.Log("Selected " + plant.name);
        }
    }
    void Update()
    {
        if (mainCamera == null || plantFollowImage == null || !plantFollowImage.gameObject.activeSelf) return;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        plantFollowImage.transform.position = mousePos;
    }
    public void GetImg(Sprite img)
    {
        if (plantFollowImage != null) plantFollowImage.sprite = img;
    }
    public void DisableFollowImage()
    {
        if (plantFollowImage != null) plantFollowImage.gameObject.SetActive(false);
    }
    public void EnableFollowImage()
    {
        if (plantFollowImage != null) plantFollowImage.gameObject.SetActive(true);
    }
    public float GetSelectedPlantCost()
    {
        if (selectedPlant != null)
        {
            Plant plantScript = selectedPlant.GetComponent<Plant>();
            if (plantScript != null)
            {
                return plantScript.GetCost();
            }
        }
        return 0;
    }
}
