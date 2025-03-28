using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTypePanelController : PanelController
{
    public void OnClickSinglePlayButton()
    {
        //HGameManager.Instance.StartGame(HYConstants.GameType.SinglePlay);
    }

    public void OnClickDualPlayButton()
    {
        HGameManager.Instance.StartGame(HYConstants.GameType.DualPlay);
    }

    public void OnClickMultiPlayButton()
    {
        HGameManager.Instance.StartGame(HYConstants.GameType.MultiPlay);
    }
    
    public void OnClickCloseButton()
    {
        Hide();
    }
}
