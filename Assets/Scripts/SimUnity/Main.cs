using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSO.Files.Formats.DBPF;
using SU2.Files.Formats.RCOL;
using System.IO;
using NAudio;
using NAudio.Wave;
using SU2.Utils;

public class Main : MonoBehaviour
{
    private IWavePlayer mWaveOutDevice;
    private WaveStream mMainOutputStream;
    private WaveChannel32 mVolumeStream;
    public RCOLFile arfile;
    public GMDCDataBlock gmdcData;

    private bool LoadAudioFromData(byte[] data)
    {
        try
        {
            MemoryStream tmpStr = new MemoryStream(data);
            mMainOutputStream = new Mp3FileReader(tmpStr);
            mVolumeStream = new WaveChannel32(mMainOutputStream);

            mWaveOutDevice = new WaveOut();
            mWaveOutDevice.Init(mVolumeStream);

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Error! " + ex.Message);
        }

        return false;
    }
    /*
    private void LoadAudio()
    {
        System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
        ofd.Title = "Open audio file";
        ofd.Filter = "MP3 audio (*.mp3) | *.mp3";
        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            WWW www = new WWW(cLocalPath + ofd.FileName);
            Debug.Log("path = " + cLocalPath + ofd.FileName);
            while (!www.isDone) { };
            if (!string.IsNullOrEmpty(www.error))
            {
                //System.Windows.Forms.MessageBox.Show("Error! Cannot open file: " + ofd.FileName + "; " + www.error);
                return;
            }

            byte[] imageData = www.bytes;

            if (!LoadAudioFromData(imageData))
            {
                //System.Windows.Forms.MessageBox.Show("Cannot open mp3 file!");
                return;
            }

            mWaveOutDevice.Play();

            Resources.UnloadUnusedAssets();
        }
    }*/

    private void UnloadAudio()
    {
        if (mWaveOutDevice != null)
        {
            mWaveOutDevice.Stop();
        }
        if (mMainOutputStream != null)
        {
            // this one really closes the file and ACM conversion
            mVolumeStream.Close();
            mVolumeStream = null;

            // this one does the metering stream
            mMainOutputStream.Close();
            mMainOutputStream = null;
        }
        if (mWaveOutDevice != null)
        {
            mWaveOutDevice.Dispose();
            mWaveOutDevice = null;
        }
    }

    private void Start()
    {
        Debug.Log(Path.Combine(Application.dataPath, "config.json"));
        var conf = JsonUtility.FromJson<Config>(File.ReadAllText(Path.Combine(Application.dataPath,"config.json")));
        
          var pack = new DBPFFile(conf.package_dir);
        Debug.Log("Entries in this file("+conf.package_dir+"): " + pack.NumEntries);
        var modelPack = new DBPFFile(conf.model_dir);
    }

    void OnApplicationQuit()
    {
        mWaveOutDevice.Stop();
        mWaveOutDevice.Dispose();
    }
}


