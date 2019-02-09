/*
 *  Compressor/Decompressor utility for Tales of Games
 *  Copyright (C) 2005-2007 soywiz - http://www.tales-tra.com/
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using System;

namespace compto {
	public static class compto {
		public static string VERSION = "2.01a";
		// Modificadores
		private static int modifier = 0, raw = 0, silent = 0, once = 0;
		private static bool bigEndian = false;

		private static void show_header_once() {
			if (once != 0 || silent != 0) return;
			Console.WriteLine("Compressor/Decompressor utility for 'Tales of...' Games - version " + VERSION);
			Console.WriteLine("Copyright (C) 2006-2009 soywiz - http://www.tales-tra.com/");
			Console.WriteLine();
			once = 1;
		}

		private static void show_help() {
			Console.WriteLine("<Modifiers>");
			Console.WriteLine("  -s silent mode");
			Console.WriteLine("  -r use raw files");
			Console.WriteLine("  -e use big endian instead of little endian");
			Console.WriteLine();
			Console.WriteLine("<Commands>");
			Console.WriteLine("  -b <file.out> buffer dump");
			Console.WriteLine("  -c[<V>] compress <file.in> <file.out>");
			Console.WriteLine("  -d[<V>] uncompress <file.in> <file.out>");
			Console.WriteLine("  -t[<V>] tests uncompress/compress/uncompress <file.in>");
			Console.WriteLine("  -p[<V>] make profile of compression <file.in>");
			Console.WriteLine("  \t<V> -> (1 - LZSS | 3 - LZSS+RLE | 5 - LZX)");
		}

		public const int ACTION_NONE    = -1;
		public const int ACTION_ENCODE  =  0;
		public const int ACTION_DECODE  =  1;
		public const int ACTION_TEST    =  2;
		public const int ACTION_PROFILE =  3;
		public const int ACTION_BDUMP   =  4;
		public const int ACTION_HELP    =  5;

		public static int Main(string[] args) {
			int argc = args.Length + 1;
			int retval = 0;
			string[] dparams = new string[2];
			int paramc = 0, @params = -1;

			string temp = null;
			int done = 0;
			string source = null;
			int n = 0;

			// Acción a realizar
			int action = ACTION_NONE;

			if (argc <= 1) {
				action = ACTION_HELP;
				{ @params = 0; paramc = 0; }
			}

			for (n = 1; n <= argc; n++) {
				string arg;
				arg = (n < argc) ? args[n - 1] : "";

				//printf("%s\n", arg);

				// Muestra la ayuda y sale
				if (arg == "-?" || arg == "-h" || arg == "--help") {
					//show_help(); return -1;
					action = ACTION_HELP;
					{ @params = 0; paramc = 0; }
				}

				// Modificadores
				{
					// Modo raw (sin cabeceras de compresión)
					if (arg == "-r" || arg == "-raw") { raw = 1; continue; }

					// Modo silencioso
					if (arg == "-s") { silent = 1; /*fclose(stdout);*/ continue; }

					if (arg == "-e") { bigEndian = true; continue; }
				}

				// Acciones
				{
					if (arg.Length >= 2 && arg[0] == '-') {
						int cnt = 1;

						switch (arg[1]) {
							// Codificad
							case 'c': action = ACTION_ENCODE ; { @params = 2; paramc = 0; } break;
							// Decodificia
							case 'd': action = ACTION_DECODE ; { @params = 2; paramc = 0; } break;
							// Comprueba
							case 't': action = ACTION_TEST   ; { @params = 1; paramc = 0; } break;
							// Crea un perfil de compresión
							case 'p': action = ACTION_PROFILE; { @params = 1; paramc = 0; } break;
							// Dumpea el text_buffer inicial
							case 'b': action = ACTION_BDUMP  ; { @params = 1; paramc = 0; } break;
							default:  cnt = 0; break;
						}

						if (cnt != 0) {
							done = 0;
							modifier = (arg.Length >= 3) ? int.Parse(arg.Substring(2)) : 3;
							continue;
						}
					}
				}

				if ((n < argc) && (paramc < @params)) {
					dparams[paramc++] = arg;
				}

				if (paramc >= @params) {
					show_header_once();

					done = 1;
					switch (action) {
						case ACTION_ENCODE:
							if (dparams[0] != dparams[1]) {
								retval |= complib.EncodeFile(dparams[0], dparams[1], raw, modifier, !bigEndian);
							} else {
								Console.WriteLine("Can't use same file for input and output");
								retval |= -1;
							}
						break;
						case ACTION_DECODE:
							if (dparams[0] != dparams[1]) {
								retval |= complib.DecodeFile(dparams[0], dparams[1], raw, modifier, !bigEndian);
							} else {
								Console.WriteLine("Can't use same file for input and output");
								retval |= -1;
							}
						break;
						case ACTION_PROFILE:
							{
								temp = arg + ".profile";
								complib.ProfileStart(temp);
									retval |= complib.DecodeFile(dparams[0], null, raw, modifier, !bigEndian);
								complib.ProfileEnd();
							}
						break;
						case ACTION_BDUMP:
							complib.DumpTextBuffer(dparams[0]);
						break;
						case ACTION_TEST:
							retval |= complib.CheckCompression(dparams[0], modifier);
						break;
						case ACTION_HELP:
							show_help();
							return -1;
						default:
							if (n == argc) {
								if (paramc == @params || @params == 0) return retval;
								if (@params == -1) { show_help(); return -1; }
								Console.WriteLine("Expected {0} params, but {1} given", @params, paramc);
								return -1;
							}
							Console.WriteLine("Unknown parameter '{0}'", arg);
							return -1;
					}

					paramc = @params = 0;
					action = ACTION_NONE;
				}
			}

			show_header_once();

			Console.WriteLine("Expected {0} params, but {1} given", @params, paramc);
			return -1;
		}
	}
}
