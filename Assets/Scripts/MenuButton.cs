using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class MenuButton : MonoBehaviour
{
	private EventTrigger Events;
	private RectTransform MyRectTransform;
	private bool IsMouseOver = false;
	private float HoverPhase = 0f;
	private RectTransform Strips;
	private GameObject Hover;
	private Image Background;
	private float StripsPhase = 0f;
	public delegate void ClickDelegate();
	public ClickDelegate OnClick;
	private Color BaseColor;
	private Color HoverColor;
	private bool _IsSelected;
	public bool IsSelected
	{
		get
		{
			return _IsSelected;
		}
		set
		{
            _IsSelected = value;
			if (Background != null)
			{
				if (_IsSelected)
					Background.color = new Color(56f / 256f, 183f / 256f, 159f / 256f);
				else
					Background.color = new Color(31f / 256f, 217f / 256f, 181f / 256f);
			}

        }
	}
	// Start is called before the first frame update
	void Start()
	{
		MyRectTransform = GetComponent<RectTransform>();
		Events = GetComponent<EventTrigger>();
		Hover = transform.Find("Hover").gameObject;
		Strips = Hover.transform.Find("Strips").GetComponent<RectTransform>();
		if (Background == null)
			Background = transform.Find("Background").GetComponent<Image>();
		Hover.SetActive(false);

		var mouseOver = new EventTrigger.Entry();
		mouseOver.eventID = EventTriggerType.PointerEnter;
		mouseOver.callback.AddListener((eventData) => {
			Controller.Instance.PlayHoverSound();
			IsMouseOver = true;
		});
		Events.triggers.Add(mouseOver);

		var mouseOut = new EventTrigger.Entry();
		mouseOut.eventID = EventTriggerType.PointerExit;
		mouseOut.callback.AddListener((eventData) => {
			IsMouseOver = false;
		});
		Events.triggers.Add(mouseOut);

		var click = new EventTrigger.Entry();
		click.eventID = EventTriggerType.PointerClick;
		click.callback.AddListener((eventData) => {
			this.Click();
		});
		Events.triggers.Add(click);
		BaseColor = Background.color;
		float h, s, v;
		Color.RGBToHSV(BaseColor, out h, out s, out v);
		v += 0.1f;
		HoverColor = Color.HSVToRGB(h, s, v);
        Background.color = _IsSelected ? new Color(56f / 256f, 183f / 256f, 159f / 256f) : BaseColor;
    }

	private void Click()
	{
		Controller.Instance.PlayClickSound();
		OnClick?.Invoke();
	}

    // Update is called once per frame
    void Update()
    {
		if (IsMouseOver && HoverPhase < 1f)
			HoverPhase = Mathf.Min(1f, HoverPhase + Time.deltaTime * 3f);
		if (!IsMouseOver && HoverPhase > 0f)
			HoverPhase = Mathf.Max(0f, HoverPhase - Time.deltaTime * 3f);
		var scaleAdd = EasingFunction.EaseInOutBack(0f, 0.1f, HoverPhase);
		MyRectTransform.localScale = new Vector3(1f + scaleAdd, 1f + scaleAdd, 1f);

		StripsPhase += Time.deltaTime * 2f;
		if (StripsPhase > 1f)
			StripsPhase -= 1f;
		Strips.anchoredPosition = new Vector2(-60 + 60 * StripsPhase, 0f);
		if (IsMouseOver)
		{
			Background.color = _IsSelected ? new Color(56f / 256f, 183f / 256f, 159f / 256f) : HoverColor; // new Color(90f / 255f, 240f / 255f, 212f / 255f);
			Hover.SetActive(true);
		}
		else
		{
			Background.color = _IsSelected ? new Color(56f / 256f, 183f / 256f, 159f / 256f) : BaseColor;// new Color(31f / 255f, 217f / 255f, 181f / 255f);
			Hover.SetActive(false);
		}

    }
}
