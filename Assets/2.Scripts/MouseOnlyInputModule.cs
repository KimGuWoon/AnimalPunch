using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOnlyInputModule : StandaloneInputModule
{

    public override void Process()
    {
        bool usedEvent = SendUpdateEventToSelectedObject();
        // base.Process()�� Ű����/���콺 ��� ó���ϹǷ�
        // ���콺 �̺�Ʈ�� ���� ����
        ProcessMouseEvent();
    }  
}
