using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle), typeof(Image))]
public class SelectToggle : MonoBehaviour
{
    [Header("���� �� ��ϵ� ĳ���� �ε���")]
    public int CharacterIndex = 0;

    [Header("������ ������ Graphic ��� (Image, RawImage ��)")]
    [SerializeField] private Graphic[] targets; // P1_Image(Image), �ʿ�� RawImage�� �߰�

    [Header("�ð� ���� ����")]
    [SerializeField] private Color onColor = new Color(1f, 0.45f, 0.45f, 1f); // ���� ������
    [SerializeField] private Color offColor = new Color(0.772f, 0.769f, 0.514f, 1f);                     // �⺻��

    [Header("�ϳ��� ���õǰ� �� �� ����(����)")]
    [SerializeField] private ToggleGroup group;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();

        // Hover/Pressed�� �������� �ʰ� ��ȯ/�׺���̼� ��
        toggle.transition = Selectable.Transition.None;
        var nav = toggle.navigation;
        nav.mode = Navigation.Mode.None;
        toggle.navigation = nav;

        if (group != null) toggle.group = group;

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnEnable()
    {
        // �г��� ������ ������ ��� �ݿ�
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
