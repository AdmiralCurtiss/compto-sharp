using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaPluginBase;

namespace compto {
	public class ComptoHyoutaToolsDecompressor : HyoutaPluginBase.IDecompressor {
		private static uint ReadUInt( System.IO.Stream s, bool littleEndian ) {
			int b1 = s.ReadByte();
			int b2 = s.ReadByte();
			int b3 = s.ReadByte();
			int b4 = s.ReadByte();
			if ( littleEndian ) {
				return (uint)( b4 << 24 | b3 << 16 | b2 << 8 | b1 );
			} else {
				return (uint)( b1 << 24 | b2 << 16 | b3 << 8 | b4 );
			}
		}

		public CanDecompressAnswer CanDecompress( DuplicatableStream stream ) {
			long pos = stream.Position;
			try {
				int b0 = stream.ReadByte();
				if ( b0 == 0 || b0 == 1 || b0 == 3 ) {
					uint compressed = ReadUInt( stream, true );
					uint uncompressed = ReadUInt( stream, true );

					if ( b0 == 0 ) {
						return ( compressed == uncompressed && compressed + 9 == stream.Length ) ? CanDecompressAnswer.Yes : CanDecompressAnswer.No;
					} else {
						return ( compressed + 9 == stream.Length ) ? CanDecompressAnswer.Yes : CanDecompressAnswer.No;
					}
				}

				return CanDecompressAnswer.No;
			} finally {
				stream.Position = pos;
			}
		}

		public DuplicatableStream Decompress( DuplicatableStream input ) {
			using ( MemoryStream ms = new MemoryStream() ) {
				if ( complib.DecodeStream( input, ms, 0, 0, true ) == 0 ) {
					ms.Position = 0;
					byte[] data = new byte[ms.Length];
					ms.Read( data, 0, (int)ms.Length );
					return new HyoutaUtils.Streams.DuplicatableByteArrayStream( data );
				}
				return null;
			}
		}

		public string GetId() {
			return "compto";
		}
	}
}
