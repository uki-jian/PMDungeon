using UnityEngine;

public abstract class CEntity : MonoBehaviour
{
    public abstract void OnSelected();
    public abstract Vector3Int Pos { get; set; }
}
