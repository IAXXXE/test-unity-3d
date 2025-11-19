using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;
using DG.Tweening;

public class SoulSoil : MonoBehaviour
{
    public Transform eyes;
    public Transform eyeL;
    public Transform eyeR;

    void Update()
    {
        
    }

    [Button]
    public void Startled()
    {
        transform.DOScale(new Vector3(0.8f, 1.3f, 0.8f), 0.2f).SetEase(Ease.OutElastic);
        transform.DOScale(new Vector3(1f ,1f ,1f), 0.3f).SetDelay(0.3f).OnComplete(() => EyesHide());
    }

    [Button]
    public void Glance()
    {
        eyes.DORotate(new Vector3(0, 0, -15), 0.2f);
        eyes.DORotate(new Vector3(0, 10, -15), 0.2f).SetDelay(0.5f);
        eyes.DORotate(new Vector3(0, -15, -15), 0.4f).SetDelay(1.5f);
        eyes.DORotate(new Vector3(0, 0, 0), 0.2f).SetDelay(3f);

    }

    [Button]
    public void EyesHide()
    {
        eyes.DORotate(new Vector3(0, 0, -30), 0.3f);
    }

    [Button]
    public void EyesShow()
    {
        eyes.DORotate(new Vector3(0, 0, 0), 0.3f);
    }

    [Button]
    public void EyesLookAt()
    {
        eyes.DORotate(new Vector3(0, 0, 20), 0.3f);
    }

    [Button]
    public void Squinting()
    {
        eyeL.DOScaleY(0.03f, 0.15f);
        eyeR.DOScaleY(0.03f, 0.15f);
    }

    [Button]
    public void EyesOpen()
    {
        eyeL.DOScaleY(0.12f, 0.15f);
        eyeR.DOScaleY(0.12f, 0.15f);
    }

    [Button]
    public void EyesBlink()
    {
        eyeL.DOScaleY(0.03f, 0.15f);
        eyeR.DOScaleY(0.03f, 0.15f);

        eyeL.DOScaleY(0.12f, 0.15f).SetDelay(0.16f);
        eyeR.DOScaleY(0.12f, 0.15f).SetDelay(0.16f);
    }

}

// 