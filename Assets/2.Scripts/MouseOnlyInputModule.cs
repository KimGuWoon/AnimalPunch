using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOnlyInputModule : StandaloneInputModule
{

    public override void Process()
    {
        bool usedEvent = SendUpdateEventToSelectedObject();
        // base.Process()는 키보드/마우스 모두 처리하므로
        // 마우스 이벤트만 따로 실행
        ProcessMouseEvent();
    }  
}
