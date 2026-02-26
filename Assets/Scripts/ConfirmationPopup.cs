using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationPopup : MonoBehaviour
{
	public TMPro.TextMeshProUGUI Text;
	public Pop YesPop;
	public Pop NoPop;
	public MenuButton YesButton;
	public MenuButton NoButton;
	public CanvasGroup BackgroundCanvasGroup;

	public Pop PopupPop;
	private System.Action Callback;
	private System.Action CancelCallback;

	private bool IsOpen = false;
	private float OpenPhase = 0f;

	void OnEnable()
	{
	}

	void Start()
	{
		gameObject.SetActive(false);
		BackgroundCanvasGroup.alpha = 0f;
		NoButton.OnClick += Hide;
		YesButton.OnClick += () =>
		{
			Callback?.Invoke();
			Hide();
		};
	}
	
	public void Hide()
	{
		if (IsOpen)
		{
			Controller.Instance.PlayPopoutSound();
			IsOpen = false;
			StartCoroutine(Close());
			CancelCallback?.Invoke();
		}
	}


	public void Show(string text, System.Action callback, System.Action cancel)
	{
		if (!IsOpen)
		{
			IsOpen = true;
			Text.text = text;
			CancelCallback = cancel;
			Callback = callback;
			gameObject.SetActive(true);
			StartCoroutine(Open());
		}
	}

	IEnumerator Open()
	{
		PopupPop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		YesPop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		NoPop.PopUp();
	}

	IEnumerator Close()
	{
		NoPop.PopOut();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		YesPop.PopOut();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		PopupPop.PopOut();
		yield return new WaitForSeconds(0.4f * Settings.AnimationMultiplier);
		gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update()
    {
		if (IsOpen && OpenPhase < 1f)
			OpenPhase = Mathf.Min(1f, OpenPhase + Time.deltaTime * 2f * (1f / Settings.AnimationMultiplier));
		if (!IsOpen && OpenPhase > 0f)
			OpenPhase = Mathf.Max(0f, OpenPhase - Time.deltaTime * 2f * (1f / Settings.AnimationMultiplier));
		BackgroundCanvasGroup.alpha = OpenPhase;
	}
}
