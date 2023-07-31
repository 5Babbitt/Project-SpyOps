using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleButton : MonoBehaviour
{
    public Button button;
    [SerializeField] private GameObject emptytext, activeRole;
    [SerializeField] private TMP_Text usernameText;

    void Awake()
    {
        button = this.GetComponent<Button>();
    }

    public void SetActiveButton(string name)
    {
        emptytext.SetActive(false);
        activeRole.SetActive(true);
        usernameText.text = name;
        button.interactable = false;
    }
}
