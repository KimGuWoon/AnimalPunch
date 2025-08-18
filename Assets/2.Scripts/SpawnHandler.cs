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
            // 이미 들고 온 플레이어를 새 스폰 포인트 위치/회전으로 이동
            existing.transform.SetPositionAndRotation(playerSpawnPoint.position, playerSpawnPoint.rotation);

            // 헤드 앵커 다시 알려주기(씬 바뀌어도 UI가 확실히 따라오게)
            Transform headAnchor = FindDeepChild(existing.transform, "HeadAnchor");
            if (headAnchor != null)
                GameEvents.SetPlayerAnchor(headAnchor);

            var pc = existing.GetComponent<PlayerController>();
            if (pc != null) pc.ResetHealthToFull();

            return;
        }

        // 없으면 최초 생성 + DDOL
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
        // 보통 AI는 DDOL 안 하니까 새 씬에서 없으면 생성
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
            // 플레이어는 화면 좌측에 있으니 → 오른쪽(+X) 바라보게
            FaceToSide(player.transform, lookRight: true);
        }

        if (ai != null)
        {
            // AI는 화면 우측에 있으니 → 왼쪽(-X) 바라보게
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
