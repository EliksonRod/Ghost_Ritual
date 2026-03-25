using UnityEngine;

[CreateAssetMenu(fileName = "Item Data", menuName = "PickUp")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public GameObject prefab;
}
