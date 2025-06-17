using UnityEngine;

public abstract class Items : MonoBehaviour
{
    [Header("Item Data")]
    [Tooltip("Reference to the item data scriptable object")]
    public ItemData ItemData;

    public abstract ItemData GetItemData();
}
