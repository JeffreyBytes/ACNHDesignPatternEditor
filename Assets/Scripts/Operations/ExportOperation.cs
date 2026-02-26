using System.Collections.Generic;

public class ExportOperation : IOperation
{
	private DesignPattern Pattern;
	private bool _IsFinished = false;

	public ExportOperation(DesignPattern pattern)
	{
		this.Pattern = pattern;
	}

	public void Abort()
	{
		_IsFinished = true;
	}

	public DesignPattern GetPattern()
	{
		return this.Pattern;
	}

	public bool IsFinished()
	{
		return _IsFinished;
	}

	public void Start()
	{
		var colors = Pattern.GetPixels();
		var bitmap = new TextureBitmap(Pattern.Width, Pattern.Height);
		for (var y = 0; y < Pattern.Width; y++)
		{
			for (var x = 0; x < Pattern.Height; x++)
			{
				bitmap.SetPixel(x, y, new TextureBitmap.Color((byte) (colors[x + y * Pattern.Width].a * 255f), (byte) (colors[x + y * Pattern.Width].r * 255f), (byte) (colors[x + y * Pattern.Width].g * 255f), (byte) (colors[x + y * Pattern.Width].b * 255f)));
			}
		}
        var path = TinyFileDialogs.SaveFileDialog("Export image", "", new List<string>() { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }, "Image");
		if (path != null)
		{
			bitmap.Save(path);
			_IsFinished = true;
		}
	}
}