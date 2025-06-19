using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private Transform follow;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        follow = GameManager.Instance.player.transform;
        SetFollow();
    }

    // Update is called once per frame
    void Update()
    {
        if (follow != null)
            return;

        follow = GameManager.Instance.player.transform;
        SetFollow();
    }

    private void SetFollow()
    {
        if (follow == null)
            return;
        virtualCamera.Follow = follow;
    }
}
