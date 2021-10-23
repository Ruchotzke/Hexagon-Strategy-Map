using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour {

	public Color[] colors;

	public HexGrid hexGrid;

	int activeElevation;
    int activeWaterLevel;

    Color activeColor;

    bool applyColor = false;
    bool applyElevation = false;
    bool applyWaterLevel = false;
    int brushSize;

    /* Dragging */
    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

	public void SelectColor (int index) {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
	}

	public void SetElevation (float elevation) {
		activeElevation = (int)elevation;
	}

	void Awake () {
		SelectColor(-1);
	}

	void Update () {
		if (
			Input.GetMouseButton(0) &&
			!EventSystem.current.IsPointerOverGameObject()
		) {
			HandleInput();
		}
        else
        {
            previousCell = null;
        }
	}

	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
            HexCell current = hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != current)
            {
                ValidateDrag(current);
            }
            else
            {
                isDrag = false;
            }
            EditCells(current);
            previousCell = current;
		}
        else
        {
            previousCell = null;
        }
	}

    void ValidateDrag(HexCell currentCell)
    {
        for (dragDirection = HexDirection.NE; dragDirection <= HexDirection.NW; dragDirection++)
        {
            if (previousCell.GetNeighbor(dragDirection) == currentCell)
            {
                isDrag = true;
                return;
            }
        }
        isDrag = false;
    }

    void EditCell (HexCell cell) {
        if(cell != null)
        {
            if (applyColor) cell.Color = activeColor;
            if (applyElevation) cell.Elevation = activeElevation;
            if (applyWaterLevel) cell.WaterLevel = activeWaterLevel;
            if (riverMode == OptionalToggle.No) cell.RemoveRiver();
            if (roadMode == OptionalToggle.No) cell.RemoveRoads();
            if (isDrag)
            {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell)
                {
                    if (riverMode == OptionalToggle.Yes)
                    {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if (roadMode == OptionalToggle.Yes)
                    {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
	}

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    void EditCells(HexCell center)
    {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        for(int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for(int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    public void ShowUI(bool visible)
    {
        hexGrid.ShowUI(visible);
    }

    enum OptionalToggle{Ignore, Yes, No}

    OptionalToggle riverMode, roadMode;

    public void SetRiverMode(int mode)
    {
        riverMode = (OptionalToggle)mode;
    }

    public void SetRoadMode(int mode)
    {
        roadMode = (OptionalToggle)mode;
    }

    public void SetApplyWaterLevel(bool toggle)
    {
        applyWaterLevel = toggle;
    }

    public void SetWaterLevel(float level)
    {
        activeWaterLevel = (int)level;
    }
}