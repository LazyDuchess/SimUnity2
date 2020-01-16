using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SU2.Utils;
using NAudio;
using NAudio.Wave;
using System.IO;
using FSO.Files.XA;

public enum MusicCategory { Splash = 0, Nhood = 1};
public class Music : MonoBehaviour
{
    private IWavePlayer mWaveOutDevice;
    private WaveStream mMainOutputStream;
    private WaveChannel32 mVolumeStream;
    public int currentStation = (int)MusicCategory.Splash;
    bool started = false;
    public static List<Station> soundtrack = new List<Station>();
    public void OnSongEnd()
    {
        if (started)
        {
            mWaveOutDevice.Stop();
            mWaveOutDevice.Dispose();
        }
        mWaveOutDevice = new WaveOut();
        var song = GetRandomSong();
        mWaveOutDevice = LoadAudioFromData(Environment.GetAudio(Environment.GetAsset(song)), mWaveOutDevice as WaveOut);
        mWaveOutDevice.Play();
        started = true;
    }
    private WaveOut LoadAudioFromData(XAFile data, WaveOut device)
    {
        try
        {
            if (started)
            {
                device.Stop();
                //device.Dispose();
               // device = new WaveOut();
            }
            MemoryStream tmpStr = null;
            //MemoryStream 

            if (data.XA)
            {
                tmpStr = new MemoryStream(data.DecompressedData);
                mMainOutputStream = new WaveFileReader(tmpStr);
            }
            else
            {
                tmpStr = new MemoryStream(data.inputBytes);
                mMainOutputStream = new Mp3FileReader(tmpStr);
            }
            mVolumeStream = new WaveChannel32(mMainOutputStream);
            device.Init(mVolumeStream);
            started = true;
            return device;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Error! " + ex.Message);
        }

        return null;
    }
    public void Play()
    {
        var song = GetRandomSong();
        mWaveOutDevice = new WaveOut();
        mWaveOutDevice = LoadAudioFromData(Environment.GetAudio(Environment.GetAsset(song)),mWaveOutDevice as WaveOut);
        mWaveOutDevice.Play();
    }
    public IEnumerator SwitchStationCoroutine()
    {
        if (!started)
        {
            mWaveOutDevice = new WaveOut();
            var song = GetRandomSong();
            mWaveOutDevice = LoadAudioFromData(Environment.GetAudio(Environment.GetAsset(song)), mWaveOutDevice as WaveOut);
            mWaveOutDevice.Play();
            started = true;
            yield break;
        }
        else
        {
            while(mWaveOutDevice.Volume > 0f)
            {
                mWaveOutDevice.Volume = Mathf.Min(1f,Mathf.Max(0f,(mWaveOutDevice.Volume - Time.deltaTime*0.5f)));
                yield return null;
            }
            mWaveOutDevice.Stop();
            mWaveOutDevice.Dispose();
            mWaveOutDevice = new WaveOut();
            
            var song = GetRandomSong();
            mWaveOutDevice = LoadAudioFromData(Environment.GetAudio(Environment.GetAsset(song)), mWaveOutDevice as WaveOut);
            mWaveOutDevice.Play();
            started = true;
            mWaveOutDevice.Volume = 1f;
            yield break;
        }
    }
    public void SetStation(MusicCategory station)
    {
        SetStation((int)station);
    }
    public void SetStation(int station)
    {
        if (station != currentStation)
        {
            currentStation = station;
            StartCoroutine(SwitchStationCoroutine());
        }
    }
    private void Update()
    {
        if (started)
        {
            if (mWaveOutDevice.PlaybackState != PlaybackState.Playing)
                OnSongEnd();
        }
    }
    void OnApplicationQuit()
    {
        if (started)
        {
            mWaveOutDevice.Stop();
            mWaveOutDevice.Dispose();
        }
    }
    int GetRandomSong()
    {
        var cat = soundtrack[currentStation].songs;
        return cat[Random.Range(0, cat.Count)].TGIR;
    }
}
