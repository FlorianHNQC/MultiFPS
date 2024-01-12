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
        m_HostButton.onClick.AddListener(() =>
        {
            if (m_NetworkManager.StartHost() == true)
            {
                if(m_NetworkManager.IsServer == true)
                {
                    StartCoroutine(Spawn());
                }
                else
                {
                    Debug.Log("is not the server");
                }
            }
            else {
                Debug.Log("start can't start");
            };
        });
        m_JoinButton.onClick.AddListener(() =>
        {
            m_NetworkManager.StartClient();
            StartCoroutine(SpawnClient());
            //
        });

        IEnumerator Spawn()
        {
            yield return new WaitForSeconds(2f);
            m_CanvasMenu.gameObject.SetActive(false);
            m_CanvasHUD.gameObject.SetActive(true);
            enemy.transform.position = enemySpawnPosition.transform.position;
            GameObject Enemy = Instantiate(enemy);
            Enemy.GetComponent<NetworkObject>().Spawn();
            yield return null;      
        }

        IEnumerator SpawnClient()
        {
            yield return new WaitForSeconds(2f);
            m_CanvasMenu.gameObject.SetActive(false);
            m_CanvasHUD.gameObject.SetActive(true);
            yield return null;
        }
    }
}
