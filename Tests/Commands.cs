﻿using System;
using System.Collections.Generic;
using XtermSharp;
using Xunit;

namespace XtermSharp.Tests {
	static class Commands {
		/// <summary>
		/// Move cursor to point
		/// </summary>
		public static void CUP (int col, int row)
		{

		}

		public static (int col, int row) GetCursorPosition (TerminalCommands commander, IResponseReader reader)
		{
			commander.DeviceStatus (new int [] { CsiCommandCodes.DeviceStatus }, "");
			var result = reader.ReadCSI ("R");
			return (result [1], result [0]);
		}

		public static int [] ReadCSI (this IResponseReader reader, string final, string prefix = null)
		{
			var code = reader.ReadNext ();
			switch (code) {
			case 27:
				reader.ReadOrDie ((byte)'[');
				break;
			case 0x9b:
				break;
			default:
				throw new Exception ($"Expected CSI, found {code}");
			}

			var pars = new List<int> ();
			var currentParam = "";

			var ch = reader.ReadNextChar ();
			if (!Char.IsDigit (ch) && ch != ';') {
				if (prefix != null && ch == prefix [0]) {
					ch = reader.ReadNextChar ();
				} else {
					throw new Exception ($"unexpected character `{ch}`");
				}
			}

			while (true) {
				if (ch == ';') {
					pars.Add (int.Parse (currentParam));
					currentParam = "";
				} else if (ch >= '0' && ch <= '9') {
					currentParam += ch;
				} else {
					// Read all the final characters, asserting they match.
					while (true) {
						Assert.Equal (ch, final [0]);
						final = final.Substring (1);
						if (final.Length > 0) {
							ch = reader.ReadNextChar ();
						} else {
							break;
						}
					}

					if (currentParam == "") {
						pars.Add (0);
					} else {
						pars.Add (int.Parse (currentParam));
					}

					break;
				}

				ch = reader.ReadNextChar ();
			}

			return pars.ToArray ();
		}

		static char ReadNextChar (this IResponseReader reader)
		{
			return (char)reader.ReadNext ();
		}

		static void ReadOrDie (this IResponseReader reader, byte expected)
		{
			var ch = reader.ReadNext ();
			if (ch != expected) {
				throw new Exception ($"expected {expected} but read {ch}");
			}
		}

		//	def GetCursorPosition ():
		//esccmd.DSR (esccmd.DSRCPR, suppressSideChannel = True)
		//params = escio.ReadCSI ("R")
		//return Point (int (params[1]), int (params[0]))

		//def CUP (point= None, row= None, col= None):
		//""" Move cursor to |point| """
		//if point is None and row is None and col is None:
		//  escio.WriteCSI(params=[], final="H")
		//elif point is None:
		//  if row is None:
		//    row = ""
		//  if col is None:
		//    escio.WriteCSI(params=[row], final="H")
		//  else:
		//    escio.WriteCSI(params=[row, col], final="H")
		//else:
		//  escio.WriteCSI(params=[point.y (), point.x ()], final="H")

	}
}
