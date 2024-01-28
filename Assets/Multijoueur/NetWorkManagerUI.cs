using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Data;
using System;

public class NetWorkManagerUI : MonoBehaviour
{
    [SerializeField] private Button m_HostButton;
    [SerializeField] private Button m_JoinButton;
    [SerializeField] private NetworkManager m_NetworkManager;
    [SerializeField] private Canvas m_CanvasMenu;
    [SerializeField] private Camera m_CameraMenu;
    [SerializeField] private GameObject m_CanvasHUD;
    [SerializeField] private List<GameObject> m_EnnemyList;
    [SerializeField] private GameObject enemy;
    [SerializeField] private GameObject enemySpawnPosition;


    private void Awake()
    {
        //Debug.Log("networkUI");
        StartCoroutine(Spawn());
        
    }

    IEnumerator Spawn()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            //Debug.Log("networkUI2");
            yield return new WaitForSeconds(3f);
            enemy.transform.position = enemySpawnPosition.transform.position;
            GameObject Enemy = Instantiate(enemy);
            Enemy.GetComponent<NetworkObject>().Spawn();
            yield return null;
        }
           
    }

}
