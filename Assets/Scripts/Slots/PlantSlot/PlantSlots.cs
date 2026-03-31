using System;
using UnityEngine;
using UnityEngine.UI;
public class PlantSlots : MonoBehaviour
{
    public GameObject plantslot;
    public GameObject[] slot;
    public GameObject selectedPlant;
    public GameObject selectedPlantCard;
    public Image plantFollowImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int childCount = plantslot.transform.childCount;

        slot = new GameObject[childCount];

        for(int i = 0; i < childCount; i++)
        {
            slot[i] = plantslot.transform.GetChild(i).gameObject;
        }
        plantFollowImage.gameObject.SetActive(false);
    }
    public void GetSelectedPlant(GameObject plant, GameObject plantCard)
    {
        if (plant != null)
        {
            float sunAmount = FindAnyObjectByType<GameManager>().sumAmount;
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
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        plantFollowImage.transform.position = mousePos;
    }
    public void GetImg(Sprite img)
    {
        plantFollowImage.sprite = img;
    }
    public void DisableFollowImage()
    {
        plantFollowImage.gameObject.SetActive(false);
    }
    public void EnableFollowImage()
    {
        plantFollowImage.gameObject.SetActive(true);
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
