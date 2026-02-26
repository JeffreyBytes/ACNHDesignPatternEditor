using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsPopup : MonoBehaviour
{
    public Pop CancelPop;
    public Pop CreditsPop;
    public MenuButton _60HzButton;
    public MenuButton _90HzButton;
    public MenuButton _120HzButton;
    public MenuButton _144HzButton;
    public MenuButton _240HzButton;
    public MenuButton NormalButton;
    public MenuButton FastButton;
    public MenuButton FastestButton;
    public MenuButton CreditsButton;
    public MenuButton CloseButton;
    public Credits Credits;

    public CanvasGroup BackgroundCanvasGroup;
    public TMPro.TextMeshProUGUI Text;

    public Pop PopupPop;
    private bool IsOpen = false;
    private float OpenPhase = 0f;

    void OnEnable()
    {
    }

    void Start()
    {
        BackgroundCanvasGroup.alpha = 0f;
        CloseButton.OnClick = () => {
            Hide();
        };
        _60HzButton.OnClick = () =>
        {
            Settings.RefreshsRate = 60;
            _60HzButton.IsSelected = true;
            _90HzButton.IsSelected = false;
            _120HzButton.IsSelected = false;
            _144HzButton.IsSelected = false;
            _240HzButton.IsSelected = false;
        };
        _90HzButton.OnClick = () =>
        {
            Settings.RefreshsRate = 90;
            _60HzButton.IsSelected = false;
            _90HzButton.IsSelected = true;
            _120HzButton.IsSelected = false;
            _144HzButton.IsSelected = false;
            _240HzButton.IsSelected = false;
        };
        _120HzButton.OnClick = () =>
        {
            Settings.RefreshsRate = 120;
            _60HzButton.IsSelected = false;
            _90HzButton.IsSelected = false;
            _120HzButton.IsSelected = true;
            _144HzButton.IsSelected = false;
            _240HzButton.IsSelected = false;
        };
        _144HzButton.OnClick = () =>
        {
            Settings.RefreshsRate = 144;
            _60HzButton.IsSelected = false;
            _90HzButton.IsSelected = false;
            _120HzButton.IsSelected = false;
            _144HzButton.IsSelected = true;
            _240HzButton.IsSelected = false;
        };
        _240HzButton.OnClick = () =>
        {
            Settings.RefreshsRate = 240;
            _60HzButton.IsSelected = false;
            _90HzButton.IsSelected = false;
            _120HzButton.IsSelected = false;
            _144HzButton.IsSelected = false;
            _240HzButton.IsSelected = true;
        };
        NormalButton.OnClick = () =>
        {
            Settings.AnimationSpeed = 0;
            NormalButton.IsSelected = true;
            FastButton.IsSelected = false;
            FastestButton.IsSelected = false;
        };
        FastButton.OnClick = () =>
        {
            Settings.AnimationSpeed = 1;
            NormalButton.IsSelected = false;
            FastButton.IsSelected = true;
            FastestButton.IsSelected = false;
        };
        FastestButton.OnClick = () =>
        {
            Settings.AnimationSpeed = 2;
            NormalButton.IsSelected = false;
            FastButton.IsSelected = false;
            FastestButton.IsSelected = true;
        };
        CreditsButton.OnClick = () =>
        {
            Hide();
            Credits.Show();
        };
    }

    public void Hide()
    {
        if (IsOpen)
        {
            Controller.Instance.PlayPopoutSound();
            IsOpen = false;
            StartCoroutine(Close());
        }
    }


    public void Show()
    {
        if (!IsOpen)
        {
            IsOpen = true;

            _60HzButton.IsSelected = Settings.RefreshsRate == 60;
            _90HzButton.IsSelected = Settings.RefreshsRate == 90;
            _120HzButton.IsSelected = Settings.RefreshsRate == 120;
            _144HzButton.IsSelected = Settings.RefreshsRate == 144;
            _240HzButton.IsSelected = Settings.RefreshsRate == 240;
            NormalButton.IsSelected = Settings.AnimationSpeed == 0;
            FastButton.IsSelected = Settings.AnimationSpeed == 1;
            FastestButton.IsSelected = Settings.AnimationSpeed == 2;

            gameObject.SetActive(true);
            StartCoroutine(Open());
        }
    }

    IEnumerator Open()
    {
        PopupPop.PopUp();
        yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);

        CancelPop.PopUp();
        CreditsPop.PopUp();
    }

    IEnumerator Close()
    {
        CancelPop.PopOut();
        CreditsPop.PopOut();
        yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
        PopupPop.PopOut();
        yield return new WaitForSeconds(0.4f * Settings.AnimationMultiplier);
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOpen && OpenPhase < 1f)
            OpenPhase = Mathf.Min(1f, OpenPhase + Time.deltaTime * 2f);
        if (!IsOpen && OpenPhase > 0f)
            OpenPhase = Mathf.Max(0f, OpenPhase - Time.deltaTime * 2f);
        BackgroundCanvasGroup.alpha = OpenPhase;
    }
}
