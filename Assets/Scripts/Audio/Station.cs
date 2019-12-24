using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station
{
    public string folderName;
    public bool hidden;
    public List<Song> songs = new List<Song>();

    public Station(string folderName, bool hidden)
    {
        this.folderName = folderName;
        this.hidden = hidden;
    }
}

public class Song
{
    public int TGIR;

    public Song(int tgir)
    {
        this.TGIR = tgir;
    }
}
