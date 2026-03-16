using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlantCard : MonoBehaviour
{
    public GameObject plantPrefab;

    private PlantSlots plantSlots;
    private GameObject selected;
    private Image plantIcon;
    private TextMeshProUGUI plantCost;
    public Sprite plantImg;        // sprite kéo từ project

    void Start()
    {
        plantSlots = FindAnyObjectByType<PlantSlots>();

        selected = transform.Find("Selected").gameObject;
        selected.SetActive(false);
        plantIcon = transform.Find("PlantIcon").GetComponent<Image>();
        plantCost = transform.Find("PlantCost").GetComponentInChildren<TextMeshProUGUI>();
        plantIcon.sprite = plantImg;
        plantCost.text = plantPrefab.GetComponent<Plant>().cost.ToString();
    
    }

    void Update()
    {
        if (plantSlots.selectedPlant == plantPrefab)
            selected.SetActive(true);
        else
        {
            selected.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        plantSlots.GetImg(plantImg);
        plantSlots.EnableFollowImage();
        plantSlots.GetSelectedPlant(plantPrefab); 
    }
}