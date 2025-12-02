using System;
using UnityEngine;
using UnityEngine.UI;

public class EndingChoiceUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Button alertVillagersButton;
    [SerializeField] private Button leaveAloneButton;

    private Action onAlertVillagers;
    private Action onLeaveAlone;

    void Awake()
    {
        if (alertVillagersButton)
        {
            alertVillagersButton.onClick.AddListener(() =>
            {
                onAlertVillagers?.Invoke();
                Hide();
            });
        }

        if (leaveAloneButton)
        {
            leaveAloneButton.onClick.AddListener(() =>
            {
                onLeaveAlone?.Invoke();
                Hide();
            });
        }

        Hide();
    }

    public void Show(Action alertVillagers, Action leaveAlone)
    {
        onAlertVillagers = alertVillagers;
        onLeaveAlone = leaveAlone;

        if (rootPanel)
        {
            rootPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (rootPanel)
        {
            rootPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
