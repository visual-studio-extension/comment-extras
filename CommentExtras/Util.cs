using System;

namespace CommentExtras {
	static class Util {
		public static string getFirst(string str, int num) {
			int end = Math.Max(0, Math.Min(num, str.Length));
			return str.Substring(0, end).PadRight(num, '\0');
		}

		public static bool isNewLine(string str) {
			// CRLF or CR or LF or NEL or LS or PS
			return str == "\r\n" || str == "\n" || str == "\r" || str == "\u0085" || str == "\u2028" || str == "\u2029";
		}
	}
}
