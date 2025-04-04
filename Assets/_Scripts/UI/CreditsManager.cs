using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    public void OnMainMenuPress()
    {
        LoadingScene.Instance.LoadScene(0); // Loading Screen
    }
}
