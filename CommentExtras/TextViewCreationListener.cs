using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

[ContentType("code")]
[Export(typeof(IWpfTextViewCreationListener))]
[TextViewRole(PredefinedTextViewRoles.Editable)]
internal sealed class TextViewCreationListener : IWpfTextViewCreationListener {
	public void TextViewCreated(IWpfTextView textView) {
		new CommentExtras.Formatter(textView);
	}
}
