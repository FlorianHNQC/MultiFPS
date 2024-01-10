using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetWorkManagerUI : MonoBehaviour
{
    [SerializeField] private Button m_HostButton;
    [SerializeField] private Button m_JoinButton;
    [SerializeField] private Canvas m_CanvasMenu;
    [SerializeField] private GameObject m_CanvasHUD;
    [SerializeField] private List<GameObject> m_EnnemyList;

    private void Awake()
    {
        m_HostButton.onClick.AddListener(() =>
        {
            Debug.Log("start server");
            NetworkManager.Singleton.StartHost();
            m_CanvasMenu.gameObject.SetActive(false);
            StartCoroutine(Spawn());
        });
        m_JoinButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            m_CanvasMenu.gameObject.SetActive(false);
            StartCoroutine(Spawn());

        });

        IEnumerator Spawn()
        {
            yield return new WaitForSeconds(0.5f);
            m_CanvasHUD.gameObject.SetActive(true);
            yield return null;
        }
    }
}
