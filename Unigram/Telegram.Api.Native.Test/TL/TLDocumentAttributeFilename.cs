// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLDocumentAttributeFilename : TLDocumentAttributeBase 
	{
		public String FileName { get; set; }

		public TLDocumentAttributeFilename() { }
		public TLDocumentAttributeFilename(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.DocumentAttributeFilename; } }

		public override void Read(TLBinaryReader from)
		{
			FileName = from.ReadString();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteString(FileName ?? string.Empty);
		}
	}
}