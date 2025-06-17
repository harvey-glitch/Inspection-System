using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Item Settings")]
    public string Name, Description;
}
