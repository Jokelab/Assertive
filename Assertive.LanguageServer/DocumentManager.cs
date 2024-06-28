using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Concurrent;
using System.Text;

namespace Assertive.LanguageServer
{

    /// <summary>
    /// Helper class to keep track of document content
    /// </summary>
    internal class DocumentManager
    {
        private readonly ConcurrentDictionary<DocumentUri, StringBuilder> _documents = new();

        public void SetDocument(DocumentUri uri, string content)
        {
            _documents[uri] = new StringBuilder(content);
        }


        public void ApplyChanges(DocumentUri uri, IEnumerable<TextDocumentContentChangeEvent> changes)
        {
            if (!_documents.ContainsKey(uri))
            {
                throw new InvalidOperationException("Document not found.");
            }

            var document = _documents[uri];
            foreach (var change in changes)
            {
                // Assuming full range changes here for simplicity, handle other cases as needed
                if (change.Range != null)
                {
                    var start = change.Range.Start;
                    var end = change.Range.End;

                    // Calculate the start and end positions in the string
                    var startPos = GetPosition(document.ToString(), start);
                    var endPos = GetPosition(document.ToString(), end);

                    // Replace the old text with the new text
                    document.Remove(startPos, endPos - startPos);
                    document.Insert(startPos, change.Text);
                }
                else
                {
                    // Handle the case where the entire content is replaced
                    document.Clear();
                    document.Append(change.Text);
                }
            }
        }

        public string GetDocumentContent(DocumentUri uri)
        {
            return _documents[uri].ToString();
        }

        public bool HasDocument(DocumentUri documentUri)
        {
            return _documents.ContainsKey(documentUri);
        }

        private int GetPosition(string text, Position position)
        {
            var lines = text.Split('\n');
            var pos = 0;
            for (int i = 0; i < position.Line; i++)
            {
                pos += lines[i].Length + 1; // +1 for the newline character
            }
            return pos + position.Character;
        }
    }


}
