using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class Soundboard : NetworkBehaviour
{
    public List<AudioClip> audioClips;
    public GameObject audioPrefab;

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
        Debug.Log(audioFiles.Length);
        foreach (string file in audioFiles)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".ogg" || ext == ".wav" || ext == ".mp3")
            {
                Debug.Log("Found audio: " + file);
                StartCoroutine(AddAudioToList(file));
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
                int index = (i == 0) ? audioClips.Count - 1 : i - 1;
                PlaySoundAtIndex(index);
            }
        }
    }
    void PlaySoundAtIndex(int index)
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
        GameObject audioObject = Instantiate(audioPrefab, playerObject.position, Quaternion.identity);
        audioObject.GetComponent<NetworkObject>().Spawn();
        audioObject.transform.SetParent(playerObject);
        ulong audioObjectID = audioObject.GetComponent<NetworkObject>().NetworkObjectId;
        PlayAudioAtClientRpc(index, audioObjectID);
        Destroy(audioObject, audioClips[index].length);
    }

    [ClientRpc]
    void PlayAudioAtClientRpc(int index, ulong id)
    {
        AudioSource source = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var audioObject))
        {
            source = audioObject.GetComponent<AudioSource>();
        }

        source.clip = audioClips[index];
        source.Play();
        
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
