using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Moonlight.IntellisenseDynamic
{
    internal class IntellisenseDynamics
    {
        #region Members
        private bool compiled = false;
        private Regex chainesRegexp = null;
        private Regex chainesFonctionRegexp = null;
        private Regex tableauRegexp = null;
        private Regex tableauFonctionRegexp = null;
        private Regex simpleVarRegexp = null;
        private Regex simpleVarFonctionRegexp = null;
        private Regex voidFonctionRegexp = null;
        #endregion

        #region Constructor
        public IntellisenseDynamics()
        {
        }
        #endregion

        #region Methods
        public void DoIntellisense_CurrentLine(CodeTextBox codeTextbox, TreeView m_IntellisenseTree)
        {
            #region Compile regexs if necessary
            if (!compiled)
            {
                Update(codeTextbox);
            }
            #endregion

            string line = RichTextboxHelper.GetCurrentLine(codeTextbox);
            int lineStart = RichTextboxHelper.GetCurrentLineStartIndex(codeTextbox);

            ProcessLine(codeTextbox, line, lineStart, m_IntellisenseTree);
        }
        public void DoIntellisense_Selection(CodeTextBox codeTextbox, int selectionStart, int selectionLength)
        {
            #region Compile regexs if necessary
            if (!compiled)
            {
                Update(codeTextbox);
            }
            #endregion

            ProcessSelection(codeTextbox, selectionStart, selectionLength);
        }
        public void DoIntellisense_AllLines(CodeTextBox codeTextbox, TreeView m_IntellisenseTree)
        {
            #region Compile regexs if necessary
            if (!compiled)
            {
                Update(codeTextbox);
            }
            #endregion

            ProcessAllLines(codeTextbox, m_IntellisenseTree);

        }
        /// <summary>
        /// Compiles the necessary regexps
        /// </summary>
        /// <param name="syntaxSettings"></param>
        public void Update(CodeTextBox codeTextbox)
        {
            //bool|byte|sbyte|char|decimal|double|float|int|uint|long|ulong|object|short|ushort

            chainesRegexp = new Regex(@"string (.*?);|string (.*?) ", RegexOptions.Compiled | RegexOptions.Multiline);
            chainesFonctionRegexp = new Regex(@"string (.*?)\(", RegexOptions.Compiled | RegexOptions.Multiline);
            tableauRegexp = new Regex(@"\[\](.*?);|\[\](.*?) ", RegexOptions.Compiled | RegexOptions.Multiline);
            tableauFonctionRegexp = new Regex(@"\[\] (.*?)\(", RegexOptions.Compiled | RegexOptions.Multiline);
            simpleVarRegexp = new Regex(@"bool(.*?);|byte (.*?);|sbyte (.*?);|char (.*?);|decimal (.*?);|double (.*?);|float (.*?);|int (.*?);|uint (.*?);|long (.*?);|ulong (.*?);|object (.*?);|short (.*?);|ushort (.*?);|bool (.*?) |byte (.*?) |sbyte (.*?) |char (.*?) |decimal (.*?) |double (.*?) |float (.*?) |int (.*?) |uint (.*?) |long (.*?) |ulong (.*?) |object (.*?) |short (.*?) |ushort (.*?) ", RegexOptions.Compiled | RegexOptions.Multiline);
            simpleVarFonctionRegexp = new Regex(@"bool (.*?)\(|byte (.*?)\(|sbyte (.*?)\(|char (.*?)\(|decimal (.*?)\(|double (.*?)\(|float (.*?)\(|int (.*?)\(|uint (.*?)\(|long (.*?)\(|ulong (.*?)\(|object (.*?)\(|short (.*?)\(|ushort (.*?)\(", RegexOptions.Compiled | RegexOptions.Multiline);
            voidFonctionRegexp = new Regex(@"void (.*?)\(", RegexOptions.Compiled | RegexOptions.Multiline);

            //Set compiled flag to true
            compiled = true;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Processes a regex.
        /// </summary>
        /// <param name="richTextbox"></param>
        /// <param name="line"></param>
        /// <param name="lineStart"></param>
        /// <param name="regexp"></param>
        /// <param name="color"></param>
        private void ProcessRegex(CodeTextBox codeTextbox, string line, int lineStart, Regex regexp, string nodeType, TreeView m_IntellisenseTree)
        {
            if (regexp == null)
            {
                // for uninitialized typename regexp
                return;
            }

            Match regMatch;

            for (regMatch = regexp.Match(line); regMatch.Success; regMatch = regMatch.NextMatch())
            {
                // Process the words
                string result = regMatch.Groups[1].Value;

                TreeNode newNode = new TreeNode(result);
                foreach (TreeNode nodes in m_IntellisenseTree.Nodes.Find(nodeType, false)[0].Nodes)
                {
                    newNode.Nodes.Add(nodes);
                }
                newNode.Name = result;
                newNode.Tag = "class";
                m_IntellisenseTree.Nodes.Add(newNode);
            }
        }
        /// <summary>
        /// Processes syntax highlightning for a line.
        /// </summary>
        /// <param name="richTextbox"></param>
        /// <param name="syntaxSettings"></param>
        /// <param name="line"></param>
        /// <param name="lineStart"></param>
        private void ProcessLine(CodeTextBox codeTextbox, string line, int lineStart, TreeView m_IntellisenseTree)
        {

            // Save the position and make the whole line black
            int nPosition = codeTextbox.SelectionStart;
            codeTextbox.SelectionStart = lineStart;
            codeTextbox.SelectionLength = line.Length;


            // Process the simpleVar
            ProcessRegex(codeTextbox, line, lineStart, simpleVarRegexp, "simpleVar", m_IntellisenseTree);

            // Process the string
            ProcessRegex(codeTextbox, line, lineStart, chainesRegexp, "chaine", m_IntellisenseTree);

            codeTextbox.SelectionStart = nPosition;
            codeTextbox.SelectionLength = 0;
        }
        private void ProcessSelection(CodeTextBox codeTextbox, int selectionStart, int selectionLength)
        {
            // Save the position
            int nPosition = selectionStart;

            codeTextbox.SelectionStart = selectionStart;
            codeTextbox.SelectionLength = selectionLength;
            string text = codeTextbox.SelectedText;


            codeTextbox.SelectionStart = nPosition;
            codeTextbox.SelectionLength = 0;
        }
        public void ProcessAllLines(CodeTextBox codeTextbox, TreeView m_IntellisenseTree)
        {
            // Save the position
            int nPosition = codeTextbox.SelectionStart;
            codeTextbox.SelectionStart = 0;
            codeTextbox.SelectionLength = codeTextbox.Text.Length;

            // Process the keywords
            ProcessRegex(codeTextbox, codeTextbox.Text, 0, simpleVarRegexp, "simpleVar", m_IntellisenseTree);

            // Process the string
            ProcessRegex(codeTextbox, codeTextbox.Text, 0, chainesRegexp, "chaine", m_IntellisenseTree);

            codeTextbox.SelectionStart = nPosition;
            codeTextbox.SelectionLength = 0;
        }
        #endregion
    }
}
