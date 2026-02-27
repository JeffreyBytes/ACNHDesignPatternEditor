using DesignServer;
using SimplePaletteQuantizer.Quantizers.XiaolinWu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class PatternSelector : MonoBehaviour
{
	public RectTransform PanelLeft;
	public Transform Patterns;
	public GameObject PatternPrefab;
	public ActionMenu ActionMenu;
	public GameObject PatternExchange;
	public GameObject Save;
	public GameObject Cancel;
	public GameObject MainButtons;
	public GameObject CloneSwapButtons;
	private Pop PatternExchangePop;
	private Pop SavePop;
	private Pop CancelPop;
    private MenuButton PatternExchangeButton;
    private MenuButton SaveButton;
	private MenuButton CancelButton;
	public MenuButton CancelCloneSwapButton;
	public Image DesignsIcon;
	public Image ProDesignsIcon;
	public GameObject DesignsTooltip;
	public GameObject ProDesignsTooltip;
	public EventTrigger DesignsEventTrigger;
	public EventTrigger ProDesignsEventTrigger;
	public Menu CurrentMenu;
	private DesignServer.Pattern UploadedPattern;

	private bool IsOpened = false;
	private float OpenPhase = 0f;
	private float LastOpenPhase = -1f;
	private PatternSelectorPattern Selected = null;
	private PatternSelectorPattern[] PatternObjects = new PatternSelectorPattern[100];
	private bool PreventSwitch = false;
	public enum Menu
	{
		None,
		Designs,
		ProDesigns
	}

	// Start is called before the first frame update
	void OnEnable()
    {
        PatternExchangePop = PatternExchange.GetComponent<Pop>();
        SavePop = Save.GetComponent<Pop>();
        CancelPop = Cancel.GetComponent<Pop>();
        PatternExchangeButton = PatternExchange.transform.GetChild(0).GetComponent<MenuButton>();
        SaveButton = Save.transform.GetChild(0).GetComponent<MenuButton>();
		CancelButton = Cancel.transform.GetChild(0).GetComponent<MenuButton>();
	}

	public void ReleaseLock()
	{
		PreventSwitch = false;
	}

    public void SwitchToDesigns(bool preventSwitch = false)
	{
        PreventSwitch = preventSwitch;
        if (CurrentMenu != Menu.Designs)
		{
			ProDesignsTooltip.SetActive(false);
			DesignsTooltip.SetActive(true);
			ProDesignsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
			DesignsIcon.color = new Color(228f / 255f, 107f / 255f, 137f / 255f);
			ProDesignsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
			DesignsIcon.rectTransform.sizeDelta = new Vector2(75f, 75f);
			CurrentMenu = Menu.Designs;
			CreatePatterns();
		}
	}

	public void SwitchToProDesigns(bool preventSwitch = false)
	{
		PreventSwitch = preventSwitch;
		if (CurrentMenu != Menu.ProDesigns)
		{
			DesignsTooltip.SetActive(false);
			ProDesignsTooltip.SetActive(true);
			DesignsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
			ProDesignsIcon.color = new Color(228f / 255f, 107f / 255f, 137f / 255f);
			DesignsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
			ProDesignsIcon.rectTransform.sizeDelta = new Vector2(75f, 75f);
			CurrentMenu = Menu.ProDesigns;
			CreatePatterns();
		}
	}
	void Start()
	{
		var click = new EventTrigger.Entry();
		click.eventID = EventTriggerType.PointerClick;
		click.callback.AddListener((eventData) => {
			if (!PreventSwitch)
			{
				SwitchToDesigns();
				ActionMenu.Close();
			}
		});
		DesignsEventTrigger.triggers.Add(click);

		click = new EventTrigger.Entry();
		click.eventID = EventTriggerType.PointerClick;
		click.callback.AddListener((eventData) => {
			if (!PreventSwitch)
			{
				SwitchToProDesigns();
				ActionMenu.Close();
			}
		});
		ProDesignsEventTrigger.triggers.Add(click);

		CloneSwapButtons.SetActive(false);
        PatternExchangeButton.OnClick += () =>
        {
            Controller.Instance.SwitchToPatternExchange();
        };
        SaveButton.OnClick += () =>
		{
			Controller.Instance.Save();
		};
		CancelButton.OnClick += () =>
		{
			Controller.Instance.SwitchToMainMenu();
		};
		CancelCloneSwapButton.OnClick = () =>
		{
			Controller.Instance.CurrentOperation.Abort();
		};
	}

	public void Close()
	{
		if (IsOpened)
		{
			PreventSwitch = false;
			IsOpened = false;
			StartCoroutine(DoClose());
		}
	}

	void CreatePatterns()
	{
		Logger.Log(Logger.Level.TRACE, "Removing all pattern objects (count: " + Patterns.childCount + ")");
		for (var i = Patterns.childCount - 1; i >= 0; i--)
			Destroy(Patterns.GetChild(i).gameObject);

		if (this.CurrentMenu == Menu.ProDesigns)
		{
			for (var i = 0; i < Controller.Instance.CurrentSavegame.ProDesignPatterns.Length; i++)
			{
				Logger.Log(Logger.Level.TRACE, "Creating pattern selector button for pro design (" + i + ")");
				var newObj = GameObject.Instantiate(PatternPrefab, Patterns);
				PatternObjects[i] = newObj.GetComponent<PatternSelectorPattern>();
				PatternObjects[i].PatternSelector = this;
				PatternObjects[i].SetPattern(Controller.Instance.CurrentSavegame.ProDesignPatterns[i]);
			}
		}
		else
		{
			for (var i = 0; i < Controller.Instance.CurrentSavegame.SimpleDesignPatterns.Length; i++)
			{
				Logger.Log(Logger.Level.TRACE, "Creating pattern selector button for design (" + i + ")");
				var newObj = GameObject.Instantiate(PatternPrefab, Patterns);
				PatternObjects[i] = newObj.GetComponent<PatternSelectorPattern>();
				PatternObjects[i].PatternSelector = this;
				PatternObjects[i].SetPattern(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[i]);
			}
		}
	}

	public void Open()
	{
		Logger.Log(Logger.Level.DEBUG, "Opening pattern selector...");
		try
		{
			if (CurrentMenu == Menu.None)
			{
				CurrentMenu = Menu.Designs;
				ProDesignsTooltip.SetActive(false);
				DesignsTooltip.SetActive(true);
				ProDesignsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
				DesignsIcon.color = new Color(228f / 255f, 107f / 255f, 137f / 255f);
				ProDesignsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
				DesignsIcon.rectTransform.sizeDelta = new Vector2(75f, 75f);
			}

			Logger.Log(Logger.Level.TRACE, "Current menu: " + this.CurrentMenu.ToString());

			CreatePatterns();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "Exception while parsing design patterns: " + e.ToString());
		}

		if (!IsOpened)
		{
			IsOpened = true;
			StartCoroutine(DoOpen());
		}
	}

	IEnumerator DoClose()
	{
		Controller.Instance.PlayPopoutSound();
		ActionMenu.Close();
        PatternExchangePop.PopOut();
        yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
        CancelPop.PopOut();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		SavePop.PopOut();
	}

	IEnumerator DoOpen()
	{
        SavePop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		CancelPop.PopUp();
        yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
        PatternExchangePop.PopUp();
    }

    void SavePattern(DesignPattern editPattern)
	{
		Controller.Instance.NameInput.SetName(editPattern.Name);
		Controller.Instance.SwitchToNameInput(
			() =>
			{
				editPattern.Name = Controller.Instance.NameInput.GetName();
				UnityEngine.Debug.Log(editPattern);
				if (editPattern is ProDesignPattern)
					Controller.Instance.CurrentSavegame.ProDesignPatterns[editPattern.Index].CopyFrom(editPattern);
				else 
					Controller.Instance.CurrentSavegame.SimpleDesignPatterns[editPattern.Index].CopyFrom(editPattern);
				Controller.Instance.SwitchToPatternMenu();
			},
			() =>
			{
				Controller.Instance.SwitchToPatternMenu();
			}
		);
	}

	void EditPattern(DesignPattern editPattern)
	{
		Controller.Instance.SwitchToPatternEditor(editPattern,
		() =>
		{
			var resultPattern = Controller.Instance.PatternEditor.Save();
			//resultPattern.IsPro = resultPattern.Type != DesignPattern.TypeEnum.SimplePattern;
			resultPattern.Index = editPattern.Index;
			resultPattern.ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
			resultPattern.IsSet = true;
			resultPattern.Name = editPattern.Name;
			SavePattern(resultPattern);
		},
		() =>
		{
			Controller.Instance.SwitchToPatternMenu();
		}
	);
	}
	public void SelectPattern(PatternSelectorPattern pattern)
	{
        if (Controller.Instance.CurrentOperation != null && Controller.Instance.CurrentOperation is IMultiplePatternSelectorOperation multiSelectorOperation)
        {
			multiSelectorOperation.SelectPattern(pattern.Pattern);
        }
        else if (Controller.Instance.CurrentOperation != null && Controller.Instance.CurrentOperation is ISelectSecondPatternOperation patternSelectorOperation)
        {
            patternSelectorOperation.SelectPattern(pattern.Pattern);
        }
        else if (Controller.Instance.CurrentOperation != null && Controller.Instance.CurrentOperation is ISelectPatternOperation patternSelectionOperation)
        {
			patternSelectionOperation.SelectPattern(pattern.Pattern);
        }
        else
		{
			if (Selected != null)
				Selected.Unselect();
			Selected = pattern;
			pattern.Select();
			var actions = new List<(string, System.Action)>()
			{
				("Edit design", (System.Action) (() => {
					ActionMenu.Close();
					
					if (pattern.Pattern is ProDesignPattern)
					{
						var editPattern = new ProDesignPattern();
						editPattern.Index = pattern.Pattern.Index;
						editPattern.CopyFrom(pattern.Pattern);
						if (editPattern.Type == DesignPattern.TypeEnum.EmptyProPattern)
						{
							Controller.Instance.SwitchToClothSelector(
								(type) =>
								{
									editPattern.Type = type;
									EditPattern(editPattern);
								},
								() => {
									Controller.Instance.SwitchToPatternMenu();
								}
							);
						}
						else
						{
							EditPattern(editPattern);
						}
					}
					else
					{
						var editPattern = new SimpleDesignPattern();
						editPattern.Index = pattern.Pattern.Index;
						editPattern.CopyFrom(pattern.Pattern);
						EditPattern(editPattern);
					}
				})),
				("Delete design", (System.Action) (() => {
					ActionMenu.Close();
					Controller.Instance.StartOperation(new DeleteOperation(Selected.Pattern));
				})),
				("Clone design", (System.Action) (() => {
					ActionMenu.Close();
					Controller.Instance.StartOperation(new CloneOperation(Selected.Pattern));
				})),
				("Swap design", (System.Action) (() => {
					ActionMenu.Close();
					Controller.Instance.StartOperation(new SwapOperation(Selected.Pattern));
				})),
				("Import design", (System.Action) (() => {
					ActionMenu.Close();
					Controller.Instance.FormatPopup.Show(
						"<align=\"center\"><#827157>Which format do you want to import?",
						(format) =>
						{
                            if (format == FormatPopup.Format.ACNH)
                            {
#if UNITY_STANDALONE_OSX
								var path = TinyFileDialogs.OpenFileDialog("Import design", "", new List<string>() {}, "Design (*.acnh)", false);
#else 
								var path = TinyFileDialogs.OpenFileDialog("Import design", "", new List<string>() { "*.acnh" }, "Design (*.acnh)", false);
#endif
								if (path != null)
                                {
                                    try
                                    {
                                        if (path.EndsWith(".acnh"))
                                        {
                                            var file = new ACNHFileFormat(System.IO.File.ReadAllBytes(path));
                                            if (file.IsPro != (pattern.Pattern is ProDesignPattern))
                                            {
                                                Controller.Instance.Popup.SetText("Basic and pro designs are not interchangeable. (You selected a " + (pattern.Pattern is ProDesignPattern ? "pro design" : "basic design") + " but wanted to import a "+ (file.IsPro ? "pro design" : "basic design") + ")", false, () => { return true; });
                                            }
                                            else
                                            {
                                                if (file.Type == DesignPattern.TypeEnum.Unsupported)
                                                    Controller.Instance.Popup.SetText("The design you tried to import is unspported by Animal Crossing: New Horizons.", false, () => { return true; });
                                                else
                                                {
                                                    if (pattern.Pattern is ProDesignPattern)
                                                    {
                                                        Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].CopyFrom(file);
                                                        Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
                                                        pattern.SetPattern(Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index]);
                                                    }
                                                    else
                                                    {
                                                        Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].CopyFrom(file);
                                                        Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
                                                        pattern.SetPattern(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index]);
                                                    }
                                                }
                                            }
                                        }
										else
										{
											Controller.Instance.Popup.SetText("Please select a .anch file.", false, () => { return true; });
                                        }
                                    }
                                    catch (System.Exception e)
                                    {
                                        Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
                                    }
                                }
                            }
                            else if (format == FormatPopup.Format.Raw)
                            {
                                var path = TinyFileDialogs.OpenFileDialog("Import design", "", new List<string>() { "*.nhd" }, "Design (*.nhd)", false);
                                if (path != null)
                                {
                                    try
                                    {
                                        if (path.EndsWith(".nhd"))
                                        {
											var bytes = System.IO.File.ReadAllBytes(path);
											DesignPattern newPattern = null;
											if (bytes.Length == 0x2A8) // plain simple pattern
											{
												unsafe
												{
													var binaryData = new BinaryData();
													binaryData.Size = bytes.Length;
                                                    var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
													var ptr = (byte*) handle.AddrOfPinnedObject().ToPointer();
													binaryData.Data = ptr;
													newPattern = SimpleDesignPattern.Read(binaryData, 0);
													handle.Free();
												}
											}
											else if (bytes.Length == 0x8A8) // plain pro pattern
											{
												unsafe
												{
													var binaryData = new BinaryData();
													binaryData.Size = bytes.Length;
                                                    var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
													var ptr = (byte*) handle.AddrOfPinnedObject().ToPointer();
													binaryData.Data = ptr;
													newPattern = ProDesignPattern.Read(binaryData, 0);
													handle.Free();
												}
											}
                                            if ((pattern is SimpleDesignPattern && pattern.Pattern is ProDesignPattern) || (pattern is ProDesignPattern && pattern.Pattern is SimpleDesignPattern))
                                            {
                                                Controller.Instance.Popup.SetText("Basic and pro designs are not interchangeable. (You selected a " + (pattern.Pattern is ProDesignPattern ? "pro design" : "basic design") + " but wanted to import a "+ ((newPattern is ProDesignPattern) ? "pro design" : "basic design") + ")", false, () => { return true; });
                                            }
                                            else
                                            {
                                                if (newPattern.Type == DesignPattern.TypeEnum.Unsupported)
                                                    Controller.Instance.Popup.SetText("The design you tried to import is unspported by Animal Crossing: New Horizons.", false, () => { return true; });
                                                else
                                                {
                                                    if (pattern.Pattern is ProDesignPattern)
                                                    {
                                                        Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].CopyFrom(newPattern);
                                                        Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
                                                        pattern.SetPattern(Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index]);
                                                    }
                                                    else
                                                    {
                                                        Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].CopyFrom(newPattern);
                                                        Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
                                                        pattern.SetPattern(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index]);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (System.Exception e)
                                    {
                                        Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
                                    }
                                }
                            }
                            else if (format == FormatPopup.Format.ACNL)
                            {
#if UNITY_STANDALONE_OSX
								var path = TinyFileDialogs.OpenFileDialog("Import design", "", new List<string>() {}, "Design (*.acnl)", false);
#else 
								var path = TinyFileDialogs.OpenFileDialog("Import design", "", new List<string>() { "*.acnl" }, "Design (*.acnl)", false);
#endif
                                if (path != null)
								{
									try
									{
										if (path.EndsWith(".acnl"))
										{
											var file = new ACNLFileFormat(System.IO.File.ReadAllBytes(path));
													
											if (file.IsPro != pattern.Pattern is ProDesignPattern)
											{
												Controller.Instance.Popup.SetText("Basic and pro designs are not interchangeable. (You selected a " + (pattern.Pattern is ProDesignPattern ? "pro design" : "basic design") + " but wanted to import a "+ (file.IsPro ? "pro design" : "basic design") + ")", false, () => { return true; });
											}
											else
											{
												if (file.Type == DesignPattern.TypeEnum.Unsupported)
													Controller.Instance.Popup.SetText("The design you tried to import is unspported by Animal Crossing: New Horizons.", false, () => { return true; });
												else
												{
													if (pattern.Pattern is ProDesignPattern)
													{
														Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].CopyFrom(file);
														Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
														pattern.SetPattern(Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index]);
													}
													else
													{
														Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].CopyFrom(file);
														Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
														pattern.SetPattern(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index]);
													}
												}
											}
                                        }
                                        else
                                        {
                                            Controller.Instance.Popup.SetText("Please select a .acnl file.", false, () => { return true; });
                                        }
                                    }
									catch (System.Exception e)
									{
										Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
									}
								}
							}
							else if (format == FormatPopup.Format.Image)
							{
								var path = TinyFileDialogs.OpenFileDialog("Import image", "", new List<string>() { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp" }, "Image (*.png,*.jpg,*.jpeg,*.bmp,*.gif,*.webp)", false);
								if (path != null)
								{
									TextureBitmap bmp = null;
									try
									{
										bmp = TextureBitmap.Load(path, false);
												
										if (pattern.Pattern is ProDesignPattern)
										{
											Controller.Instance.SwitchToClothSelector(
												(type) => {
													var result = ACNHDesignExtractor.FindPattern(bmp, type);
													if (result.Item1 != -1 && result.Item2 != -1 && result.Item3 != -1 && result.Item4 != -1)
													{
														Controller.Instance.SwitchToImporter(bmp, result, pattern.Pattern is ProDesignPattern ? (64, 64) : (32, 32), (final) => {
															Controller.Instance.SwitchToNameInput(
																() =>
																{
																	string name = Controller.Instance.NameInput.GetName();
																	Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].FromBitmap(final);
																	Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
																	Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].Type = type;
																	Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].Name = name;
																	pattern.SetPattern(Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index]);
																	bmp.Dispose();
																	final.Dispose();
																	Controller.Instance.SwitchToPatternMenu();
																},
																() =>
																{
																	bmp.Dispose();
																	Controller.Instance.SwitchToPatternMenu();
																}
															);
														}, () => {
															bmp.Dispose();
															Controller.Instance.SwitchToPatternMenu();
														});
													}
													else
													{
														if (bmp.Width > 64 || bmp.Height > 64)
															bmp.Resample(ResamplingFilters.Box, 64, 64);

														bmp.Quantize(new WuColorQuantizer(), 15);
														Controller.Instance.SwitchToNameInput(
															() =>
															{
																string name = Controller.Instance.NameInput.GetName();
																Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].FromBitmap(bmp);
																Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
																Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].Type = type;
																Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].Name = name;
																pattern.SetPattern(Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index]);
																bmp.Dispose();
																Controller.Instance.SwitchToPatternMenu();
															},
															() =>
															{
																bmp.Dispose();
																Controller.Instance.SwitchToPatternMenu();
															}
														);
													}
												},
												() => {
													bmp.Dispose();
													Controller.Instance.SwitchToPatternMenu();
												}
											);
										}
										else
										{
											var result = ACNHDesignExtractor.FindPattern(bmp, DesignPattern.TypeEnum.SimplePattern);
											if (result.Item1 != -1 && result.Item2 != -1 && result.Item3 != -1 && result.Item4 != -1)
											{
												Controller.Instance.SwitchToImporter(bmp, result, pattern.Pattern is ProDesignPattern ? (64, 64) : (32, 32), (final) => {
													Controller.Instance.SwitchToNameInput(
														() =>
														{
															string name = Controller.Instance.NameInput.GetName();
															Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].FromBitmap(final);
															Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
															Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].Type = DesignPattern.TypeEnum.SimplePattern;
															Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].Name = name;
															pattern.SetPattern(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index]);
															bmp.Dispose();
															final.Dispose();
															Controller.Instance.SwitchToPatternMenu();
														},
														() =>
														{
															bmp.Dispose();
															Controller.Instance.SwitchToPatternMenu();
														}
													);
												}, () => {
													bmp.Dispose();
													Controller.Instance.SwitchToPatternMenu();
												});
											}
											else
											{
												if (bmp.Width > 32 || bmp.Height > 32)
													bmp.Resample(ResamplingFilters.Box, 32, 32);
												bmp.Quantize(new WuColorQuantizer(), 15);
												Controller.Instance.SwitchToNameInput(
													() =>
													{
														string name = Controller.Instance.NameInput.GetName();
														Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].FromBitmap(bmp);
														Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
														Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].Type = DesignPattern.TypeEnum.SimplePattern;
														Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].Name = name;
														pattern.SetPattern(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index]);
														bmp.Dispose();
														Controller.Instance.SwitchToPatternMenu();
													},
													() =>
													{
														bmp.Dispose();
														Controller.Instance.SwitchToPatternMenu();
													}
												);
											}
										}
									}
									catch (System.Exception e)
									{
										if (bmp != null)
											bmp.Dispose();
										Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
										Debug.LogException(e);
									}
								}
							}
							else if (format == FormatPopup.Format.QR)
                            {
                                var path = TinyFileDialogs.OpenFileDialog("Import image", "", new List<string>() { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp" }, "QR Code Image (*.png,*.jpg,*.jpeg,*.bmp,*.gif,*.webp)", false);
                                if (path != null)
								{
									TextureBitmap bmp = null;
									try
									{
										bmp = TextureBitmap.Load(path, false);
										var scanner = new BarcodeReader()
										{
											TryInverted = true,
											AutoRotate = true,

											Options = new ZXing.Common.DecodingOptions()
											{
												TryHarder = true,
												PureBarcode = true,
												CharacterSet = "ISO-8859-1",
												PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE }
											}
										};

										var result = GetResult(scanner.DecodeMultiple(bmp));
										if (result == null || result.Type == DesignPattern.TypeEnum.Unsupported)
										{
											bmp.Resample(ResamplingFilters.Lanczos8, bmp.Width * 2, bmp.Height * 2);
											result = GetResult(scanner.DecodeMultiple(bmp));
											if (result == null || result.Type == DesignPattern.TypeEnum.Unsupported)
											{
												for (int i = 250; i > 10; i-=10)
												{
													var check = bmp.Clone();
													check.UltraContrast(i);
													result = GetResult(scanner.DecodeMultiple(check));
													check.Dispose();
													if (result != null && result.Type != DesignPattern.TypeEnum.Unsupported)
														break;
												}
											}
										}
										bmp.Dispose();

										if (result == null)
										{
											Controller.Instance.Popup.SetText("Couldn't find a QR code in the provided image file.", false, () => { return true; });
										}
										else
										{
													
											if (result.IsPro != pattern.Pattern is ProDesignPattern)
											{
												Controller.Instance.Popup.SetText("Basic and pro designs are not interchangeable. (You selected a " + (pattern.Pattern is ProDesignPattern ? "pro design" : "basic design") + " but wanted to import a "+ (result.IsPro ? "pro design" : "basic design") + ")", false, () => { return true; });
											}
											else
											{
												if (result.Type == DesignPattern.TypeEnum.Unsupported)
													Controller.Instance.Popup.SetText("The design you tried to import is unspported by Animal Crossing: New Horizons.", false, () => { return true; });
												else
												{
													if (pattern.Pattern is ProDesignPattern)
													{
														Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].CopyFrom(result);
														Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
														pattern.SetPattern(Controller.Instance.CurrentSavegame.ProDesignPatterns[pattern.Pattern.Index]);
													}
													else
													{
														Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].CopyFrom(result);
														Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index].ChangeOwnership(Controller.Instance.CurrentSavegame.PersonalID);
														pattern.SetPattern(Controller.Instance.CurrentSavegame.SimpleDesignPatterns[pattern.Pattern.Index]);
													}
												}
											}
										}
									}
									catch (System.Exception e)
									{
										if (bmp != null) 
											bmp.Dispose();
										Debug.LogException(e);
									}
								}
							}
						}, 
						() => 
						{
						},
						true,
						true,
						true,
						true, 
						true,
						false
					);
				})),
				("Export design", (System.Action) (() => {
					ActionMenu.Close();
					Controller.Instance.FormatPopup.Show(
						"<align=\"center\"><#827157>In which format do you want to export?",
						(format) =>
						{
							if (format == FormatPopup.Format.Online)
							{
								Controller.Instance.SwitchToUploadPattern(
									(DesignServer.Pattern resultPattern) =>
									{
										Controller.Instance.SwitchToPatternExchange(resultPattern, () => {
											Controller.Instance.SwitchToPatternMenu();
										});
									},
									() =>
									{
										Controller.Instance.SwitchToPatternMenu();
									},
									pattern.Pattern,
									"Please enter your creator name!"
								);
							}
							else if (format == FormatPopup.Format.ACNH)
                            {
                                var path = TinyFileDialogs.SaveFileDialog("Export design", "", new List<string>() { "*.acnh" }, "Design (*.acnh)");
                                if (path != null)
								{
									if (!path.EndsWith(".acnh"))
										path += ".acnh";
									try
									{
										var file = ACNHFileFormat.FromPattern(pattern.Pattern);
										var bytes = file.ToBytes();
										System.IO.File.WriteAllBytes(path, bytes);
									}
									catch (System.Exception e)
									{
										Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
										Debug.LogException(e);
									}
								}
                            }
                            else if (format == FormatPopup.Format.Raw)
                            {
                                var path = TinyFileDialogs.SaveFileDialog("Export design", "", new List<string>() { "*.nhd" }, "Design (*.nhd)");

                                if (path != null)
                                {
									if (!path.EndsWith(".nhd"))
										path += ".nhd";
                                    try
                                    {
                                        byte[] bytes = null;
                                        DesignPattern newPattern;
                                        if (pattern.Pattern is SimpleDesignPattern simpleDesign) // plain simple pattern
										{
                                            unsafe
                                            {
												bytes = new byte[0x2a8];
                                                var binaryData = new BinaryData();
												binaryData.Size = bytes.Length;
                                                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                                                var ptr = (byte*) handle.AddrOfPinnedObject().ToPointer();
                                                binaryData.Data = ptr;
												simpleDesign.Write(binaryData, 0);
                                                handle.Free();
                                            }
                                        }
                                        else if (pattern.Pattern is ProDesignPattern proDesign) // plain pro pattern
										{
                                            unsafe
                                            {
												bytes = new byte[0x8a8];
                                                var binaryData = new BinaryData();
                                                binaryData.Size = bytes.Length;
                                                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                                                var ptr = (byte*) handle.AddrOfPinnedObject().ToPointer();
                                                binaryData.Data = ptr;
												proDesign.Write(binaryData, 0);
                                                handle.Free();
                                            }
                                        }
                                        System.IO.File.WriteAllBytes(path, bytes);
                                    }
                                    catch (System.Exception e)
                                    {
                                        Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
                                        Debug.LogException(e);
                                    }
                                }
                            }
                            else if (format == FormatPopup.Format.ACNL)
							{
								var path = TinyFileDialogs.SaveFileDialog("Export design", "", new List<string>() { "*.acnl" }, "Design (*.acnl)");
								if (path != null)
								{
									if (!path.EndsWith(".acnl"))
										path += ".acnl";
                                    try
									{
										var file = ACNLFileFormat.FromPattern(pattern.Pattern);
										var bytes = file.ToBytes();
										System.IO.File.WriteAllBytes(path, bytes);
									}
									catch (System.Exception e)
									{
										Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
										Debug.LogException(e);
									}
								}
							}
							else if (format == FormatPopup.Format.Image)
							{
								var path = TinyFileDialogs.SaveFileDialog("Export image", "", new List<string>() { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }, "Image (*.png,*.jpg,*.jpeg,*.bmp,*.gif,*.webp)");
								if (path != null)
								{
									var bitmap = pattern.Pattern.GetBitmap();
									bitmap.Save(path);
									bitmap.Dispose();
								}
							}
							else if (format == FormatPopup.Format.QR)
							{
                                var path = TinyFileDialogs.SaveFileDialog("Export image", "", new List<string>() { "*.png" }, "QR Code (*.png)");
								if (path != null && path.Length > 0)
								{
									if (!path.EndsWith(".png"))
										path += ".png";
									try
									{
										var file = ACNLFileFormat.FromPattern(pattern.Pattern);
										var bytes = file.ToBytes();
										ZXing.QrCode.Internal.Encoder.ForceByte = true;
										ZXing.BarcodeWriter writer = new BarcodeWriter();
										writer.Format = BarcodeFormat.QR_CODE;
										if (bytes.Length > 620)
										{
											int parity = UnityEngine.Random.Range(0, 255);
											TextureBitmap[] bitmaps = new TextureBitmap[4];
											for (int i = 0; i < 4; i++)
											{
												var qr = new QrCodeEncodingOptions()
												{
													ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
													Height = 400,
													Width = 400
												};
												qr.Hints.Add(EncodeHintType.STRUCTURED_APPEND, new int[] { i, 3, parity });
												qr.Hints[EncodeHintType.WIDTH] = 400;
												qr.Hints[EncodeHintType.HEIGHT] = 400;
												writer.Options = qr;
												byte[] part = new byte[540];
												System.Array.Copy(bytes, 540 * i, part, 0, 540);

												bitmaps[i] = writer.Write(System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(part));
												bitmaps[i].FlipY();
												bitmaps[i].CreateBackgroundTexture();
												bitmaps[i].Apply();
											}
											var b = Controller.Instance.QRCode.Render(pattern.Pattern, bitmaps[0], bitmaps[1], bitmaps[2], bitmaps[3]);
											System.IO.File.WriteAllBytes(path, b);
											bitmaps[0].Dispose();
											bitmaps[1].Dispose();
											bitmaps[2].Dispose();
											bitmaps[3].Dispose();
										}
										else
										{
											var qr = new QrCodeEncodingOptions()
											{
												ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
												Height = 700,
												Width = 700
											};
											writer.Options = qr;
											TextureBitmap bitmap = writer.Write(System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(bytes));
											bitmap.FlipY();
											bitmap.CreateBackgroundTexture();
											bitmap.Apply();

											System.IO.File.WriteAllBytes(path, Controller.Instance.QRCode.Render(pattern.Pattern, bitmap));
											bitmap.Dispose();
										}
									}
									catch (System.Exception e)
									{
										Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
										Debug.LogException(e);
									}
								}
							}
						},
						() =>
						{
						},
						true,
						(pattern.Pattern.Type == DesignPattern.TypeEnum.Hat3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.HornHat3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.LongSleeveDress3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.LongSleeveShirt3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.ShortSleeveDress3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.ShortSleeveShirt3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.NoSleeveDress3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.NoSleeveShirt3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.SimplePattern),
						(pattern.Pattern.Type == DesignPattern.TypeEnum.Hat3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.HornHat3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.LongSleeveDress3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.LongSleeveShirt3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.ShortSleeveDress3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.ShortSleeveShirt3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.NoSleeveDress3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.NoSleeveShirt3DS ||
						 pattern.Pattern.Type == DesignPattern.TypeEnum.SimplePattern),
						true,
						true,
						true
					);
				})),
                ("Delete multiple", (System.Action) (() => {
                    var op = new DeleteMultipleOperation();
					op.SelectPattern(Selected.Pattern);
                    Controller.Instance.StartOperation(op);
                })),
            };
			ActionMenu.ShowActions(
				actions.ToArray()
			);
		}
	}

	private ACNLFileFormat GetResult(Result[] result)
	{
		if (result != null && result.Length == 1)
		{
			var byteData = (List<byte[]>) result[0].ResultMetadata[ResultMetadataType.BYTE_SEGMENTS];

			if (byteData.Count > 0)
			{
				var bytes = byteData[0];
				int wantedLength = 0x870; 
				var patternType = bytes[0x69];
				if (patternType == 0x06 || patternType == 0x07 || patternType == 0x09)
					wantedLength = 0x26C;

				Debug.Log(patternType + " | " + bytes.Length + "/" + wantedLength);
				if (bytes.Length == wantedLength)
				{
					var format = new ACNLFileFormat(bytes);
					return format;
				}
			}
		}
		return null;
	}
	private bool OperationRunning = false;

	public void UnhighlightAll()
	{
        for (int i = 0; i < PatternObjects.Length; i++)
        {
			PatternObjects[i].Unhighlight();
        }
    }

    // Update is called once per frame
    void Update()
    {
		/*if (UploadedPattern != null)
		{
			Controller.Instance.Popup.Close();
			Controller.Instance.SwitchToPatternExchange(UploadedPattern, () => { });
			UploadedPattern = null;
		}*/
		try
		{
			if (Controller.Instance.CurrentOperation != null)
			{
				OperationRunning = true;
                if (Controller.Instance.CurrentOperation is ISelectPatternOperation selectOperation)
                {
                    MainButtons.SetActive(false);
                    CloneSwapButtons.SetActive(true);
                }
                else if (Controller.Instance.CurrentOperation is ISelectSecondPatternOperation patternOperation)
				{
					ActionMenu.Close();
					var pattern = patternOperation.GetPattern();
					for (int i = 0; i < PatternObjects.Length; i++)
					{
						if (PatternObjects[i].Pattern == pattern)
							PatternObjects[i].Highlight();
						else
							PatternObjects[i].Unhighlight();
					}
					MainButtons.SetActive(false);
					CloneSwapButtons.SetActive(true);
				}
				if (Controller.Instance.CurrentOperation is IMultiplePatternSelectorOperation multiSelect)
				{
					var patterns = multiSelect.GetPatterns();
					for (int i = 0; i < PatternObjects.Length; i++)
					{
						if (patterns.Contains(PatternObjects[i].Pattern))
							PatternObjects[i].Highlight();
						else
							PatternObjects[i].Unhighlight();
					}
					MainButtons.SetActive(false);
					CloneSwapButtons.SetActive(false);
				}
			}
			else
			{
				if (OperationRunning)
				{
					MainButtons.SetActive(true);
					CloneSwapButtons.SetActive(false);
					for (int i = 0; i < PatternObjects.Length; i++)
					{
						PatternObjects[i].SetPattern(PatternObjects[i].Pattern);
						PatternObjects[i].Unhighlight();
						PatternObjects[i].Unselect();
					}
					OperationRunning = false;
				}
			}
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError(e);
		}
		if (IsOpened && OpenPhase < 1f)
			OpenPhase = Mathf.Min(1f, OpenPhase + Time.deltaTime * 3f);
		if (!IsOpened && OpenPhase > 0f)
			OpenPhase = Mathf.Max(0f, OpenPhase - Time.deltaTime * 3f);
		if (OpenPhase != LastOpenPhase)
		{
			LastOpenPhase = OpenPhase;
			float x = EasingFunction.EaseOutBack(-750f, 50f, OpenPhase);
			PanelLeft.anchoredPosition = new Vector2(x, PanelLeft.anchoredPosition.y);
		}
	}
}
