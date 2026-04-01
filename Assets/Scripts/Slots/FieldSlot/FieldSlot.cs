using UnityEngine;

public class FieldSlot : MonoBehaviour
{
    private GameObject highlight;
    private PlantSlots plantSlots;
    private bool occupied = false;

    private GameManager gameManager;
    private Shovel shovel;
    private Plant activePlant;
    void Start()
    {
        highlight = transform.Find("Highlight").gameObject;
        highlight.SetActive(false);
        if (highlight == null)
        {
            Debug.LogError("Highlight object not found in FieldSlot " + gameObject.name);
        }
        plantSlots = FindAnyObjectByType<PlantSlots>();
        gameManager = FindAnyObjectByType<GameManager>();
        shovel = FindAnyObjectByType<Shovel>();
    }

    void OnMouseEnter()
    {
        if (InputManager.Instance.isBlocked)
            return;
        if (!occupied)
        {
            if (highlight != null)
            {
                highlight.SetActive(true);
            }
        }
    }

    void OnMouseExit()
    {
        if (highlight != null)
        {
            highlight.SetActive(false);
        }
    }

    void Update()
    {
        HandleClick();
    }

    void HandleClick()
    {
        if (InputManager.Instance.isBlocked)
            return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject)
            {
                if (shovel != null && shovel.IsShovelActive())
                {
                    TryRemovePlant();
                }
                else
                {
                    TryPlant();
                }
                break;
            }
        }
    }
    void TryRemovePlant()
    {
        if (!occupied) return;

        Transform plant = transform.GetChild(0);

        if (plant != null)
        {
            activePlant.Die(false);
            activePlant = null;

            SoundManager.instance.PlaySound(SoundManager.instance.removePlant);
            shovel.DeactivateShovel();
        }
    }
    void TryPlant()
    {
        plantSlots.DisableFollowImage();

        if (plantSlots.selectedPlant == null)
            return;

        if (!occupied)
        {
            float plantCost = plantSlots.GetSelectedPlantCost();

            GameObject plant = Instantiate(
                plantSlots.selectedPlant,
                transform.position,
                Quaternion.identity,
                transform
            );

            plant.transform.localPosition = Vector3.zero;
            Plant plantScript = plant.GetComponent<Plant>();
            activePlant = plantScript;
            plantScript.SetFieldSlot(this);
            occupied = true;
            plantSlots.selectedPlant = null;

            gameManager.UpdateSunAmount(-plantCost);

            highlight.SetActive(false);
            plantSlots.selectedPlantCard.GetComponent<PlantCard>().StartCooldown();
            SoundManager.instance.PlaySound(SoundManager.instance.plantPlace);
        }
        else
        {
            Debug.Log("This slot is already occupied.");
            plantSlots.selectedPlant = null;
        }
        InputManager.Instance.isPlanting = false;
    }

    public void SetOccupied(bool status)
    {
        occupied = status;
    }
}