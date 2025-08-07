using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Sound
{
    public string soundName;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    [SerializeField] Sound[] bgmSounds;
    private AudioSource bgmSource;

    void Awake()
    {
        if (FindObjectsOfType<SoundManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // ���� �Ѿ�� ����
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        PlaySceneBGM(SceneManager.GetActiveScene().buildIndex);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneBGM(scene.buildIndex);
    }

    void PlaySceneBGM(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < bgmSounds.Length)
        {
            if (bgmSource.clip != bgmSounds[sceneIndex].clip)
            {
                bgmSource.clip = bgmSounds[sceneIndex].clip;
                bgmSource.Play();
                Debug.Log($"BGM ���: {bgmSounds[sceneIndex].soundName}");
            }
        }
        else
        {
            Debug.LogWarning("�ش� �ε����� BGM�� �������� �ʽ��ϴ�");
        }
    }

    void Update()
    {

    }
}