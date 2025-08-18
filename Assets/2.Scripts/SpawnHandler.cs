using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHandler : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    public Transform playerSpawnPoint;
    public GameObject aiPrefab;
    public Transform aiSpawnPoint;

    private void Awake()
    {
        SpawnPlayer();
        SpawnAI();
        FaceEachOther();
    }

    void SpawnPlayer()
    {
        GameObject existing = GameObject.FindGameObjectWithTag("Player");

        if (existing != null)
        {
            // �̹� ��� �� �÷��̾ �� ���� ����Ʈ ��ġ/ȸ������ �̵�
            existing.transform.SetPositionAndRotation(playerSpawnPoint.position, playerSpawnPoint.rotation);

            // ��� ��Ŀ �ٽ� �˷��ֱ�(�� �ٲ� UI�� Ȯ���� �������)
            Transform headAnchor = FindDeepChild(existing.transform, "HeadAnchor");
            if (headAnchor != null)
                GameEvents.SetPlayerAnchor(headAnchor);

            var pc = existing.GetComponent<PlayerController>();
            if (pc != null) pc.ResetHealthToFull();

            return;
        }

        // ������ ���� ���� + DDOL
        GameObject playerObj = Instantiate(playerPrefabs[UserData.charIndex], playerSpawnPoint.position, playerSpawnPoint.rotation);
        DontDestroyOnLoad(playerObj);
        playerObj.tag = "Player";
        playerObj.SetActive(true);

        Transform newHeadAnchor = FindDeepChild(playerObj.transform, "HeadAnchor");
        if (newHeadAnchor != null)
            GameEvents.SetPlayerAnchor(newHeadAnchor);
    }

    void SpawnAI()
    {
        // ���� AI�� DDOL �� �ϴϱ� �� ������ ������ ����
        if (GameObject.FindGameObjectWithTag("AI") != null)
            return;

        GameObject aiObj = Instantiate(aiPrefab, aiSpawnPoint.position, aiSpawnPoint.rotation);
        aiObj.tag = "AI";
        aiObj.SetActive(true);

        Transform headAnchor = FindDeepChild(aiObj.transform, "HeadAnchor");
        if (headAnchor != null)
            GameEvents.SetAIAnchor(headAnchor);
    }

    void FaceEachOther()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject ai = GameObject.FindGameObjectWithTag("AI");

        if (player != null)
        {
            // �÷��̾�� ȭ�� ������ ������ �� ������(+X) �ٶ󺸰�
            FaceToSide(player.transform, lookRight: true);
        }

        if (ai != null)
        {
            // AI�� ȭ�� ������ ������ �� ����(-X) �ٶ󺸰�
            FaceToSide(ai.transform, lookRight: false);
        }
    }

    void FaceToSide(Transform t, bool lookRight)
    {
        Vector3 dir = lookRight ? Vector3.right : Vector3.left;
        t.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
