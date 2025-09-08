using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class Soundboard : NetworkBehaviour
{
    public List<AudioClip> audioClips;

    void Start()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "Soundboard");

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Soundboard folder not found: " + folderPath);
            return;
        }

        Debug.Log("Looking for audio in: " + folderPath);

        // Load all audio files in the folder
        string[] audioFiles = Directory.GetFiles(folderPath);

        foreach (string file in audioFiles)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".ogg" || ext == ".wav" || ext == ".mp3")
            {
                Debug.Log("Found audio: " + file);
                StartCoroutine(AddAudioToList(file));
                break; // just play the first one for now
            }
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                PlaySoundAtIndexServerRpc(i);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PlaySoundAtIndexServerRpc(int index)
    {
        if (audioClips == null || audioClips.Count == 0)
        {
            Debug.LogWarning("No audio clips loaded.");
            return;
        }

        if (index < 0 || index >= audioClips.Count)
        {
            Debug.LogWarning("Invalid index: " + index);
            return;
        }

        if (audioClips[index] == null)
        {
            Debug.LogWarning("Audio clip at index " + index + " is null.");
            return;
        }

        PlayAudioAtPointServerRpc(index, NetworkManager.Singleton.LocalClientId);

    }

    [ServerRpc(RequireOwnership = false)]
    void PlayAudioAtPointServerRpc(int index, ulong clientID)
    {
        Transform playerObject = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.transform;
        GameObject audioObject = new GameObject();
        audioObject.transform.position = playerObject.position;
        audioObject.transform.parent = playerObject;
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = audioClips[index];
        source.spatialBlend = 1f;
        source.Play();
        Destroy(audioObject, source.clip.length);
    }

    private IEnumerator AddAudioToList(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, GetAudioType(path)))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error loading audio: " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (audioClips.Count < 10)
            {
                audioClips.Add(clip);
            }
            else
            {
                Debug.Log("Only 10 audio clips can be loaded");
            }

        }
    }

    private AudioType GetAudioType(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        switch (ext)
        {
            case ".ogg": return AudioType.OGGVORBIS;
            case ".wav": return AudioType.WAV;
            case ".mp3": return AudioType.MPEG;
            default: return AudioType.UNKNOWN;
        }
    }
}
