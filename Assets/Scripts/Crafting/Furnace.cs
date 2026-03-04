using Sortify;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

//Coal, Ui
public class Furnace : Building, IInteractable
{
    [Header("Slots")]
    [SerializeField] private BaseSlot fuel;
    [SerializeField] private BaseSlot input;
    [SerializeField] private BaseSlot output;
    [SerializeField] private InventoryHolder holder;

    [Header("User Interface")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Slider progressSlider;

    [Header("Data")]
    [SerializeField] private Smeltables smeltablesData;

    [Header("Audio")]
    [SerializeField] private AudioSource furnaceSource;
    [SerializeField] private AudioClip smeltClip;
    
    [Header("Smelting Progress")]
    [SerializeField] private float smeltingTime;
    [SerializeField, ReadOnly] private float smeltingProgress = 0;
    [SerializeField, ReadOnly] private float smeltingFuel = 0;

    public override void OnPlaced()
    {
        //Get data
        if (smeltablesData == null)
            smeltablesData = Resources.Load<Smeltables>("Smeltables");

        //Ui sync
        PlayerInventory.Instance.OnInventoryOpen += CloseUi;

        //Setup slots
        fuel.heldBy = holder;
        input.heldBy = holder;
        output.heldBy = holder;
        fuel.UpdateSlot();
        input.UpdateSlot();
        output.UpdateSlot();

        //Slider setup
        progressSlider.maxValue = smeltingTime;
    }

    private void Update()
    {
        if(!isPlaced) 
            return;
            
        progressSlider.value = Mathf.Lerp(progressSlider.value, smeltingProgress, Time.deltaTime * 10);

        if (input.HeldItem == null)
        {
            smeltingProgress = 0;
            return;
        }

        if(smeltingFuel <= 0)
        {
            if(fuel.HeldItem != null || fuel.HeldItem != null && fuel.HeldQuantity > 0)
                foreach (var fuelItem in smeltablesData.fuel)
                {
                    if(fuelItem.item == fuel.HeldItem?.data)
                    {
                        fuel.HeldQuantity--;
                        smeltingFuel += fuelItem.efficiency;
                        fuel.UpdateSlot();
                    }
                }

            if(smeltingFuel <= 0)
            {
                return;
            }
        }

        // Only progress if input is actually smeltable
        Item foundItem = null;
        foreach (var item in smeltablesData.smeltables)
        {
            if (item.input == input.HeldItem.data)
            {
                foundItem = item.output.prefab.GetComponent<Item>();
                break;
            }
        }

        if (foundItem == null)
        {
            smeltingProgress = 0;
            return;
        }

        smeltingProgress += Time.deltaTime;

        if (smeltingProgress >= smeltingTime)
        {
            smeltingProgress = 0;
            progressSlider.value = 0;

            if (output.HeldItem == null)
            {
                Item instantiatedItem = Instantiate(foundItem.gameObject).GetComponent<Item>();
                instantiatedItem.transform.position = new Vector3(0, 1000, 0);
                instantiatedItem.UpdateHoldingItem(true);
                output.HeldItem = instantiatedItem;
                output.HeldQuantity++;
                input.HeldQuantity--;
                smeltingFuel--;

                furnaceSource.PlayOneShot(smeltClip, 1);
            }

            else if (output.HeldItem.data == foundItem.data)
            {
                output.HeldQuantity++;
                input.HeldQuantity--;
                smeltingFuel--;

                furnaceSource.PlayOneShot(smeltClip, 1);
            }

            output.UpdateSlot();
            input.UpdateSlot();
        }
    }

    #region Ui

    public void Interact(GameObject sender)
    {
        OpenUi();
    }

    public void CloseUi(bool state)
    {
        if (state) return;

        PlayerInventory.Instance?.ToggleBagNoEvent(false);

        if(canvas != null)
            canvas.SetActive(false);
    }

    public void OpenUi()
    {
        PlayerInventory.Instance?.ToggleBagNoEvent(true);

        if(canvas != null)
            canvas.SetActive(true);
    }

    public override void OnDeath()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryOpen -= CloseUi;
    }

    #endregion
}