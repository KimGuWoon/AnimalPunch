using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle), typeof(Image))]
public class SelectToggle : MonoBehaviour
{
    [Header("선택 시 기록될 캐릭터 인덱스")]
    public int CharacterIndex = 0;

    [Header("색상을 적용할 Graphic 목록 (Image, RawImage 등)")]
    [SerializeField] private Graphic[] targets; // P1_Image(Image), 필요시 RawImage도 추가

    [Header("시각 상태 색상")]
    [SerializeField] private Color onColor = new Color(1f, 0.45f, 0.45f, 1f); // 선택 고정색
    [SerializeField] private Color offColor = new Color(0.772f, 0.769f, 0.514f, 1f);                     // 기본색

    [Header("하나만 선택되게 할 때 연결(선택)")]
    [SerializeField] private ToggleGroup group;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();

        // Hover/Pressed가 간섭하지 않게 전환/네비게이션 끔
        toggle.transition = Selectable.Transition.None;
        var nav = toggle.navigation;
        nav.mode = Navigation.Mode.None;
        toggle.navigation = nav;

        if (group != null) toggle.group = group;

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnEnable()
    {
        // 패널이 꺼졌다 켜져도 즉시 반영
        ApplyVisual(toggle.isOn);
        if (toggle.isOn) UserData.charIndex = CharacterIndex;
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn) UserData.charIndex = CharacterIndex;
        ApplyVisual(isOn);
    }

    private void ApplyVisual(bool isOn)
    {
        if (targets == null) return;
        var c = isOn ? onColor : offColor;
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null) targets[i].color = c;
        }
    }
}
