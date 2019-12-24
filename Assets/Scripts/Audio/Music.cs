﻿using System.Collections;
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
    /*
    public static Dictionary<MusicCategory, List<int>> gameMusic = new Dictionary<MusicCategory, List<int>>()
    {
        { MusicCategory.Splash, new List<int>() {Hash.TGIRHash(0xFFA87950, 0x5BA1DCC7, 0x2026960B, 0xFF606C7A) } },
        { MusicCategory.Nhood, new List<int>() {Hash.TGIRHash(0xFFA78F62, 0x09CCDF11, 0x2026960B, 0xFFC4D24B) } }
    };*/

    private WaveOut LoadAudioFromData(XAFile data, WaveOut device)
    {
        try
        {
            if (started)
            {
                device.Stop();
                device.Dispose();
                device = new WaveOut();
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
            /*
            while (mWaveOutDevice.Volume < 1f)
            {
                mWaveOutDevice.Volume += Mathf.Min(1f, Mathf.Max(0f, (mWaveOutDevice.Volume + Time.deltaTime)));
                yield return null;
            }*/
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
        /*
        if (station != currentStation)
        {
            if (started)
            {
                mWaveOutDevice.Stop();
                mWaveOutDevice.Dispose();
            }
            currentStation = station;
            mWaveOutDevice = new WaveOut();
            var song = GetRandomSong();
            mWaveOutDevice = LoadAudioFromData(Environment.GetAudio(Environment.GetAsset(song)), mWaveOutDevice as WaveOut);
            mWaveOutDevice.Play();
            started = true;
        }*/
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