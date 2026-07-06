/*
 * Este script define uma biblioteca de efeitos sonoros que armazena grupos de clipes de áudio.
 * Permite a recuperação de clipes de áudio com base no nome do grupo, retornando um clipe aleatório do grupo.
 */

using UnityEngine;

// #my_code
[System.Serializable]
public struct SoundEffect
{
    public string groupID;
    public AudioClip[] clips;
}

public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects;

    public AudioClip GetClipFromName(string name)
    {
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.groupID == name)
            {
                if (soundEffect.clips == null || soundEffect.clips.Length == 0) return null;
                // Retorna um clipe aleatório para evitar repetição 
                return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
            }
        }
        return null;
    }
}