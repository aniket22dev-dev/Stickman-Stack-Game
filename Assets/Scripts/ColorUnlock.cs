using UnityEngine;
using UnityEngine.UI;

public class ColorUnlock : MonoBehaviour
{
    [System.Serializable]
    public class ColorSlot
    {
        public GameObject lockSprite;    // The lock image object
        public Button colorButton;       // The color button
        public int requiredScore;        // Score needed to unlock
    }

    public ColorSlot[] slots;            // Assign all 6 slots in Inspector

    private const string UNLOCK_KEY = "UnlockedSlots"; // PlayerPrefs key

    void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int savedUnlocks = PlayerPrefs.GetInt(UNLOCK_KEY, 0);

        for (int i = 0; i < slots.Length; i++)
        {
            // Unlock if either highscore qualifies OR it was previously unlocked
            bool shouldUnlock = highScore >= slots[i].requiredScore || IsSlotSaved(savedUnlocks, i);

            if (shouldUnlock)
            {
                UnlockSlot(i);
                savedUnlocks = SaveSlotBit(savedUnlocks, i); // persist it
            }
            else
            {
                LockSlot(i);
            }
        }

        PlayerPrefs.SetInt(UNLOCK_KEY, savedUnlocks);
        PlayerPrefs.Save();
    }

    void UnlockSlot(int i)
    {
        if (slots[i].lockSprite != null)
            Destroy(slots[i].lockSprite);

        if (slots[i].colorButton != null)
            slots[i].colorButton.interactable = true;
    }

    void LockSlot(int i)
    {
        if (slots[i].colorButton != null)
            slots[i].colorButton.interactable = false;
    }

    // Uses a single int as a bitmask to save 6 unlock states
    // e.g. if slots 0 and 2 are unlocked → binary 000101 → int 5
    bool IsSlotSaved(int savedUnlocks, int index)
    {
        return (savedUnlocks & (1 << index)) != 0;
    }

    int SaveSlotBit(int savedUnlocks, int index)
    {
        return savedUnlocks | (1 << index);
    }
}
