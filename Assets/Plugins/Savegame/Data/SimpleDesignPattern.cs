using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Pack = 1, Size = 0x2A8)]
public unsafe class SimpleDesignPattern : DesignPattern
{
	private string _Name;
	private DesignPattern.Color[] _Palette;
	private byte[] _Image;
	private byte _Type;
    public Usage _UsageFlag;

    public override int Width => 32;
	public override int Height => 32;
	public override string Name { get => _Name; set => _Name = value; }
	public override DesignPattern.TypeEnum Type { get => (DesignPattern.TypeEnum) _Type; set => _Type = (byte) value; }
	public override DesignPattern.Color[] Palette { get => _Palette; set => _Palette = value; }
	public override byte[] Image { get => _Image; set => _Image = value; }
    public override Usage UsageFlag { get => _UsageFlag; set => _UsageFlag = value; }

    public const int NameOffset = 0x10;
    public const int UsageOffset = 0x70;
    public const int PersonalIDOffset = 0x38;
    public const int PaletteOffset = 0x78;
    public const int ImageOffset = 0xA5;
    public const int TypeOffset = 0x2A5;

    public void Write(BinaryData data, int offset)
	{
		UsageFlag = Usage.Opaque;
		for (var i = 0; i < Image.Length; i++)
		{
			if ((Image[i] & 0xF) == 0xF || (Image[i] & 0x0F) == 0x0F) // transparent pixel found
			{
				UsageFlag = Usage.Transparent;
				break;
			}
		}
		data.WriteString(offset + NameOffset, this._Name, 20);
		data.WriteU16(offset + UsageOffset, (ushort) UsageFlag);
		_PersonalID.Write(data, offset + PersonalIDOffset);
		for (int i = 0; i < _Palette.Length; i++)
			_Palette[i].Write(data, offset + PaletteOffset + 0x03 * i);
		data.WriteBytes(offset + ImageOffset, this._Image);
		data.WriteU8(offset + TypeOffset, this._Type);
	}

	public static SimpleDesignPattern Read(BinaryData data, int offset)
	{
		var ret = new SimpleDesignPattern();
		ret._Name = data.ReadString(offset + NameOffset, 20);
		ret._UsageFlag = (Usage) data.ReadU16(offset + UsageOffset);
		ret._PersonalID = PersonalID.Read(data, offset + PersonalIDOffset);
		ret._Palette = new DesignPattern.Color[15];
		for (int i = 0; i < ret._Palette.Length; i++)
			ret._Palette[i] = DesignPattern.Color.Read(data, offset + PaletteOffset + 0x03 * i);
		ret._Image = data.ReadBytes(offset + ImageOffset, 0x200);
		ret._Type = data.ReadU8(offset + TypeOffset);
		return ret;
	}

	override protected int GetIndex(int x, int y)
	{
		x = x / 2;
		return x + y * (this.Width / 2);
	}
}
