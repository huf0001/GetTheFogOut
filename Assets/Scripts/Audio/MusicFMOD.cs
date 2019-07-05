using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Stages
{
    Menu,
    Stage1,
    Stage2,
    Stage3,
    ReturnMenu,
    LoseGame,
    WinGame
}

public class MusicFMOD : MonoBehaviour
{
    private GameObject musicGameObject;
    private static MusicFMOD _instance;

    [SerializeField] public Stages musicStages = new Stages();

    public static MusicFMOD instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MusicFMOD>();
                DontDestroyOnLoad(_instance.musicGameObject);
            }

            return _instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
            {
                Destroy(this.musicGameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
