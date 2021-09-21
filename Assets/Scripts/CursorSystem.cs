using System.Collections.Generic;
using UnityEngine;

public enum CursorState
{
    Targeted, UnTargeted
}

public class CursorSystem : MonoBehaviour
{
    [SerializeField] private Texture2D targetCursorImg;
    [SerializeField] private Texture2D untargetCursorImg;

    public static CursorState currentCursorState = CursorState.Targeted;
    
    private static Dictionary<CursorState, Texture2D> cursorDic = new Dictionary<CursorState, Texture2D>();
    
    public void InitCursor()
    {
        cursorDic.Add(CursorState.Targeted, targetCursorImg);
        cursorDic.Add(CursorState.UnTargeted, untargetCursorImg);
        
        Cursor.visible = true;
        SetCursor();
    }

    public static void CursorStateChange(CursorState state)
    {
        if (state != currentCursorState)
        {
            currentCursorState = state;
            SetCursor();
        }
    }

    private static void SetCursor()
    {
        Cursor.SetCursor(cursorDic[currentCursorState], new Vector2(cursorDic[currentCursorState].width / 2, cursorDic[currentCursorState].height / 2), CursorMode.Auto);
    }
}