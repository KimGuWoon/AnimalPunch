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
    }

    void SpawnPlayer()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
            return;

        GameObject playerObj = Instantiate(playerPrefabs[UserData.charIndex], playerSpawnPoint.position, playerSpawnPoint.rotation);
        DontDestroyOnLoad(playerObj);
        playerObj.tag = "Player";
        playerObj.SetActive(true);

        Transform headAnchor = FindDeepChild(playerObj.transform, "HeadAnchor");
        if (headAnchor != null)
        {
            GameEvents.SetPlayerAnchor(headAnchor);
        }
    }

    void SpawnAI()
    {
        if (GameObject.FindGameObjectWithTag("AI") != null)
            return;

        GameObject aiObj = Instantiate(aiPrefab, aiSpawnPoint.position, aiSpawnPoint.rotation);
        aiObj.tag = "AI";
        aiObj.SetActive(true);

        Transform headAnchor = FindDeepChild(aiObj.transform, "HeadAnchor");
        if (headAnchor != null)
        {
            GameEvents.SetAIAnchor(headAnchor);
        }
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
