using System;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace CommentExtras {
	class Formatter {
		private bool isChangingText;
		private IWpfTextView view;

		public Formatter(IWpfTextView textView) {
			view = textView;
			view.TextBuffer.Changed += new EventHandler<TextContentChangedEventArgs>(TextBuffer_Changed);
			view.TextBuffer.PostChanged += new EventHandler(TextBuffer_PostChanged);
		}

		private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e) {
			if (!isChangingText) {
				isChangingText = true;
				FormatCode(e);
			}
		}

		private void TextBuffer_PostChanged(object sender, EventArgs e) {
			isChangingText = false;
		}

		private void FormatCode(TextContentChangedEventArgs e) {
			if (e.Changes != null) {
				for (int i = 0; i < e.Changes.Count; i++) {
					HandleChange(e.Changes[0]);
				}
			}
		}

		private void HandleChange(ITextChange change) {
			if (!Util.isNewLine(change.NewText)) { return; }

			ITextSnapshot snap = view.Caret.ContainingTextViewLine.Snapshot;
			ITextSnapshotLine curLine = snap.GetLineFromPosition(change.OldPosition);
			string curLineText = curLine.GetText();
			int pos = 0;
		
			while (pos < curLineText.Length && char.IsWhiteSpace(curLineText[pos])) {
				++pos;
			}
		
			// The line is empty
			if (pos == curLineText.Length) { return; }

			string lastLineText = "";

			// Not at the start of the file
			if (curLine.Start.Position - 1 > 0) {
				lastLineText = snap.GetLineFromPosition(curLine.Start - 1).GetText();
			}

			string lastFirstTwo = Util.getFirst(lastLineText.Trim(), 2);
			string curFirstTwo = Util.getFirst(curLineText.Trim(), 2);
			bool infront = change.OldPosition - pos <= curLine.Start;
			char nextChar = '\0';

			// Make sure we are still on the same line
			if (change.OldPosition != snap.Length && change.OldPosition < curLine.End) { 
				nextChar = snap.GetText(change.OldPosition, 1)[0];
			}

			if (!curLineText.Contains("*/")) {
				if (curFirstTwo == "/*") {
					if (!infront) {
						ITextEdit edit = view.TextBuffer.CreateEdit();
						if (char.IsWhiteSpace(nextChar)) {
							edit.Insert(change.NewEnd, curLineText.Substring(0, pos) + " *"); 
						} else {
							edit.Insert(change.NewEnd, curLineText.Substring(0, pos) + " * ");
						}
						edit.Apply();
					}
				} else if (curFirstTwo[0] == '*') {
					if (infront) {
						ITextEdit edit = view.TextBuffer.CreateEdit();
						int offset = change.OldPosition - curLine.Start;
					
						if (offset == pos) {
							edit.Insert(change.OldPosition, "* ");
						} else {
							edit.Insert(change.OldPosition, curLineText.Substring(offset, pos-offset) + "* ");
						}

						if (offset <= pos) {
							edit.Insert(change.NewEnd, curLineText.Substring(0, offset));
						}

						edit.Apply();
					} else {
						ITextEdit edit = view.TextBuffer.CreateEdit();
						if (char.IsWhiteSpace(nextChar)) {
							edit.Insert(change.NewEnd, curLineText.Substring(0, pos) + "* ");
						} else {
							edit.Insert(change.NewEnd, curLineText.Substring(0, pos) + "* ");
						}
						edit.Apply();
					}
				}
			} else {
				if (infront && curFirstTwo == "*/") {
					if (lastFirstTwo[0] == '*') {
						ITextEdit edit = view.TextBuffer.CreateEdit();
						int offset = change.OldPosition - curLine.Start;
					
						if (offset == pos) {
							edit.Insert(change.OldPosition, "* ");
						}else {
							edit.Insert(change.OldPosition, curLineText.Substring(offset, pos-offset) + "* ");
						}
					
						if (offset <= pos) {
							edit.Insert(change.NewEnd, curLineText.Substring(0, offset));
						}

						edit.Apply();
					} else if (lastFirstTwo == "/*") {
						ITextEdit edit = view.TextBuffer.CreateEdit();
						edit.Insert(change.OldPosition, " * ");
						edit.Insert(change.NewEnd, curLineText.Substring(0, pos) + " ");
						edit.Apply();
					}
				}
			}
		}
	}
}
