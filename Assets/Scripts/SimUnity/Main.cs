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
        
        //var conf = new Config();
        //conf.package_dir = "C:/Users/TonyStank/Documents/EA Games/The Sims™ 2 Ultimate Collection/Neighborhoods/E001/E001_Neighborhood.package";
        var pack = new DBPFFile(conf.package_dir);
        Debug.Log("Entries in this file("+conf.package_dir+"): " + pack.NumEntries);
        /*
        //var fil = pack.GetItemByID((((ulong)0xFF60DF7E) << 32) + (ulong)0xF9FCFBD1 + (ulong)0x2026960B);
        //var fil = pack.GetItemByID((((ulong)0xFFA059A0) << 32) + (ulong)0x045D17A7 + (ulong)0x2026960B);
        var fil = pack.GetItemByID(new EntryRef((uint)0xFF60DF7E, (uint)0xF9FCFBD1, (uint)0x2026960B));
        
        if (!LoadAudioFromData(fil))
        {
            //System.Windows.Forms.MessageBox.Show("Cannot open mp3 file!");
            return;
        }

        mWaveOutDevice.Play();
        */
        //Resources.UnloadUnusedAssets();

        var modelPack = new DBPFFile(conf.model_dir);
        //var modelFile = modelPack.GetItemByID((((ulong)conf.model_instance) << 32) + (ulong)conf.model_instance2 + (ulong)0xAC4F8687);
        //var modelFile = modelPack.GetItemByID(new EntryRef(conf.model_instance, conf.model_instance2, 0xAC4F8687));
        //var modelFile = modelPack.GetItemByID((((ulong)0xFF0100F4) << 32) + (ulong)0xBAA19F2A + (ulong)0xAC4F8687);
        //File.WriteAllBytes(conf.output, modelFile);
        /*
        var modelRCOL = new RCOLFile(modelFile);
        arfile = modelRCOL;
        gmdcData = modelRCOL.dataBlocks[0] as GMDCDataBlock;*/
        //gmdcData.model.boundingMesh.RecalculateNormals();
        //gmdcData.model.meshes[conf.model_id].RecalculateNormals();
        /*
        GetComponent<SkinnedMeshRenderer>().sharedMesh = gmdcData.model.meshes[conf.model_id];
        GetComponent<SkinnedMeshRenderer>().localBounds = gmdcData.model.meshes[conf.model_id].bounds;*/
    }

    void OnApplicationQuit()
    {
        mWaveOutDevice.Stop();
        mWaveOutDevice.Dispose();
    }
}


