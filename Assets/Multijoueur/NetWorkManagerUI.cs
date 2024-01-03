using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetWorkManagerUI : MonoBehaviour
{
    [SerializeField] private Button m_HostButton;
    [SerializeField] private Button m_JoinButton;
    [SerializeField] private Canvas m_Canvas;

    private void Awake()
    {
        m_HostButton.onClick.AddListener(() =>
        {
            Debug.Log("start server");
            NetworkManager.Singleton.StartHost();
            m_Canvas.gameObject.SetActive(false);
        });
        m_JoinButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            m_Canvas.gameObject.SetActive(false);
        });
    }
}
