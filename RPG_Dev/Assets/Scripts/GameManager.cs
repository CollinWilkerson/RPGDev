using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Players")]
    public string playerPrefabLocation;
    public Transform[] spawnPoints;
    public float respawnTime;

    private int playersInGame;
    //singelton
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void ImInGame()
    {
        playersInGame++;

        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, 
            spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
    }
}
