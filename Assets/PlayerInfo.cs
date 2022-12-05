using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public int characterID;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);   
    }
}
