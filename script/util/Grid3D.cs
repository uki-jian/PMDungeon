using UnityEngine;

public class CGrid3D : MonoBehaviour
{
    Grid m_grid;
    public void Init(Vector3 cellSize, Vector3 cellGap)
    {
        m_grid = gameObject.AddComponent<Grid>();
        m_grid.cellSize = cellSize;
        m_grid.cellGap = cellGap;
        m_grid.cellLayout = GridLayout.CellLayout.Rectangle;
        m_grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;
    }
    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        return m_grid.CellToWorld(cellPos);
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        Vector3 fixedPos = worldPos + 0.5f * m_grid.cellSize; //加cell尺寸的一半来修正
        return m_grid.WorldToCell(fixedPos);
    }
}
