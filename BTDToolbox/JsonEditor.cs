﻿using BTDToolbox.Classes;
using BTDToolbox.Extra_Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;

namespace BTDToolbox
{
    public partial class JsonEditor : ThemedForm
    {
        //Project variables
        string livePath = Environment.CurrentDirectory;
        public string Path;
        public string fileName;

        //Find and replace variables
        public int numPhraseFound;
        public int startPosition;
        public int endPosition;
        public int endEditor;
        public bool isCurrentlySearching;
        public string previousSearchPhrase;
        public bool isReplacing;
        public bool isFinding;
        public bool findNextPhrase;
        public static int maxLC = 1;

        //Congif variables
        private ContextMenuStrip selMenu;
        private ContextMenuStrip highlightMenu;
        public static bool jsonError;
        
        public static float jsonEditorFont;
        public string lastJsonFile;
        int CharIndex_UnderMouse = 0;
        bool searchSubtask = false;
        bool tabLine = false;
        string tab;
        public JsonEditor(string Path)
        {
            InitializeComponent();
            Serializer.SaveSettings();
            StartUp();

            initSelContextMenu();
            initHighlightContextMenu();
            this.Path = Path;
            this.FormClosing += exitHandling;
            Editor_TextBox.MouseUp += Editor_TextBox_RightClicked;

            FileInfo info = new FileInfo(Path);
            this.Text = info.Name;            
            this.Find_TextBox.Visible = false;
            this.FindNext_Button.Visible = false;
            this.toolStripSeparator2.Visible = false;
            this.Replace_TextBox.Visible = false;
            this.ReplaceDropDown.Visible = false;
            

            //tabstops
            this.tB_line.TabStop = false;
            this.lintPanel.TabStop = false;
            this.Find_TextBox.AcceptsTab = false;
            this.Editor_TextBox.TabStop = false;
            string formattedText = "";

            string unformattedText = File.ReadAllText(Path);
            
            try
            {
                JToken jt = JToken.Parse(unformattedText);
                formattedText = jt.ToString(Formatting.Indented);
                Editor_TextBox.Text = formattedText;
            }
            catch (Exception)
            {
                Editor_TextBox.Text = unformattedText;
            }
            CheckJSON(this.Editor_TextBox.Text);

            this.FontSize_TextBox.TextChanged += new System.EventHandler(this.FontSize_TextBox_TextChanged);

            JsonProps.increment(this);
            this.Load += EditorLoading;

            HandleTools();
        }
        private void StartUp()
        {
            this.Size = new Size(Serializer.cfg.JSON_Editor_SizeX, Serializer.cfg.JSON_Editor_SizeY);
            this.Location = new Point(Serializer.cfg.JSON_Editor_PosX, Serializer.cfg.JSON_Editor_PosY);

            jsonEditorFont = Serializer.cfg.JSON_Editor_FontSize;
            Font newfont = new Font("Consolas", jsonEditorFont);
            tB_line.Font = newfont;
            Editor_TextBox.Font = newfont;
            FontSize_TextBox.Text = jsonEditorFont.ToString();
        }
        private void HandleTools()
        {
            if (Path.EndsWith("tower"))
                EasyTowerEditor_Button.Visible = true;
            else
                EasyTowerEditor_Button.Visible = false;

            if (Path.EndsWith("bloon"))
                EZBoon_Button.Visible = true;
            else
                EZBoon_Button.Visible = false;
        }

        private void EditorLoading(object sender, EventArgs e)
        {
            tB_line.Font = Editor_TextBox.Font;
            Editor_TextBox.Select();
            AddLineNumbers();

            bool close = false;
            
            if(close)
            {
                this.Close();
            }
        }
        private void Editor_TextBox_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllText(Path, Editor_TextBox.Text);
            //Editor_TextBox.SelectionIndent = 100;
            CheckJSON(this.Editor_TextBox.Text);

            if (Editor_TextBox.Text == "")
            {
                AddLineNumbers();
            }
            if(tabLine)
            {
                Editor_TextBox.SelectedText = tab;
                tabLine = false;
            }
        }
        private void CheckJSON (string text)
        {
            if (JSON_Reader.IsValidJson(text) == true)
            {
                this.lintPanel.BackgroundImage = Properties.Resources.JSON_valid;
                jsonError = false;
            }
            else
            {
                this.lintPanel.BackgroundImage = Properties.Resources.JSON_Invalid;
                jsonError = true;
            }
        }
        private void FontSize_TextBox_TextChanged(object sender, EventArgs e)
        {
            float FontSize = 0;
            float.TryParse(FontSize_TextBox.Text, out FontSize);
            if (FontSize < 3)
                FontSize = 3;
            jsonEditorFont = FontSize;
            Font newfont = new Font("Consolas", jsonEditorFont);
            Editor_TextBox.Font = newfont;
            tB_line.Font = newfont;
        }
        private void exitHandling(object sender, EventArgs e)
        {
            Serializer.SaveSettings();
            JsonProps.decrement(this);
        }
        private void Editor_TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                searchSubtask = false;
                ShowFindMenu();
            }
            if (e.Control && e.KeyCode == Keys.H)
            {
                ShowReplaceMenu();
            }

            if (e.Control && e.KeyCode == Keys.D)
            {
                MessageBox.Show("Insert Ctrl + D hotkey here...");
            }
            if (e.KeyCode == Keys.Enter)
            {
                tab = string.Concat(Enumerable.Repeat(" ", IndentNewLines()));
                tabLine = true;
            }
            if (e.KeyCode == Keys.Back)
            {
                if (Editor_TextBox.SelectedText.Length == 0)
                {
                    RemoveEmptySpaces();
                }
            }
        }
        private void FindText()
        {
            if (Find_TextBox.Text.Length <= 0)
            {
                MessageBox.Show("You didn't enter anything to search for. Please Try Again");
            }
            else
            {
                findNextPhrase = true;
                endEditor = Editor_TextBox.Text.Length;

                startPosition = Editor_TextBox.SelectionStart + 1;

                if (previousSearchPhrase != Find_TextBox.Text)
                {
                    endPosition = 0;
                    numPhraseFound = 0;
                }
                for (int i = 0; i < endEditor; i = startPosition)
                {
                    previousSearchPhrase = this.Find_TextBox.Text;
                    isCurrentlySearching = true;
                    if (i == -1)
                    {
                        isCurrentlySearching = false;
                        break;
                    }
                    if (startPosition >= endEditor)
                    {
                        MessageBox.Show("Reached the end of the file");
                        break;
                    }
                    startPosition = Editor_TextBox.Find(Find_TextBox.Text, startPosition, endEditor, RichTextBoxFinds.None);
                    if (startPosition >= 0)
                    {
                        findNextPhrase = false;
                        numPhraseFound++;
                        //Editor_TextBox.SelectionColor = Color.Blue;       //saving this value for later use
                        endPosition = this.Find_TextBox.Text.Length;
                        startPosition = startPosition + endPosition;
                        break;
                    }

                    if (numPhraseFound == 0)
                    {
                        MessageBox.Show("No Match Found!!!");
                    }
                }
            }
        }
        private void HideFindButton()
        {
            isFinding = false;
            this.Find_TextBox.Visible = false;
            this.FindNext_Button.Visible = false;
        }
        private void HideReplaceButton()
        {
            isReplacing = false;
            this.Replace_TextBox.Visible = false;
            this.ReplaceDropDown.Visible = false;
            this.toolStripSeparator2.Visible = false;
        }
        private void ShowFindMenu()
        {
            isFinding = !isFinding;
            this.Find_TextBox.Visible = !this.Find_TextBox.Visible;
            this.FindNext_Button.Visible = !this.FindNext_Button.Visible;
            if (isReplacing)
            {
                HideFindButton();
                HideReplaceButton();
            }
        }
        private void ShowReplaceMenu()
        {
            isReplacing = !isReplacing;
            searchSubtask = false;
            this.Find_TextBox.Visible = !this.Find_TextBox.Visible;
            this.FindNext_Button.Visible = !this.FindNext_Button.Visible;
            this.toolStripSeparator2.Visible = !this.toolStripSeparator2.Visible;
            this.Replace_TextBox.Visible = !this.Replace_TextBox.Visible;
            this.ReplaceDropDown.Visible = !this.ReplaceDropDown.Visible;
            if (isFinding)
            {
                isFinding = false;
                isReplacing = true;
                this.Find_TextBox.Visible = true;
                this.FindNext_Button.Visible = true;
                this.toolStripSeparator2.Visible = true;
                this.Replace_TextBox.Visible = true;
                this.ReplaceDropDown.Visible = true;
            }
        }
        private void ShowFindMenu_Button_Click_1(object sender, EventArgs e)
        {
            searchSubtask = false;
            ShowFindMenu();
        }
        private void ReplaceButton_Click(object sender, EventArgs e)
        {
            if (Find_TextBox.Text.Length <= 0)
            {
                MessageBox.Show("You didn't enter anything to search for. Please Try Again");
            }
            if (Replace_TextBox.Text.Length <= 0)
            {
                MessageBox.Show("You didn't enter anything to replace with. Please Try Again");
            }
            if (findNextPhrase)
            {
                MessageBox.Show("You need to find something before you can try replacing it...");
            }
            else if (findNextPhrase == false)
            {
                Editor_TextBox.Text = Editor_TextBox.Text.Remove(startPosition - endPosition, Find_TextBox.Text.Length);
                Editor_TextBox.Text = Editor_TextBox.Text.Insert(startPosition - endPosition, Replace_TextBox.Text);
                endPosition = this.Replace_TextBox.Text.Length;
                startPosition = startPosition + endPosition;

                endEditor = Editor_TextBox.Text.Length;

                startPosition = Editor_TextBox.SelectionStart + 1;

                if (previousSearchPhrase != Find_TextBox.Text)
                {
                    endPosition = 0;
                    numPhraseFound = 0;
                }

                for (int i = 0; i < endEditor; i = startPosition)
                {
                    previousSearchPhrase = this.Find_TextBox.Text;
                    isCurrentlySearching = true;
                    if (i == -1)
                    {
                        isCurrentlySearching = false;
                        break;
                    }
                    startPosition = Editor_TextBox.Find(Find_TextBox.Text, startPosition, endEditor, RichTextBoxFinds.None);
                    if (startPosition >= 0)
                    {
                        numPhraseFound++;
                        //Editor_TextBox.SelectionColor = Color.Blue;       //saving this value for later use
                        endPosition = this.Find_TextBox.Text.Length;
                        startPosition = startPosition + endPosition;
                        break;
                    }
                }
            }
        }
        private void ReplaceAllButton_DropDown_Click_1(object sender, EventArgs e)
        {
            if (Find_TextBox.Text.Length <= 0)
            {
                MessageBox.Show("You didn't enter anything to search for. Please Try Again");
            }
            else if (Replace_TextBox.Text.Length <= 0)
            {
                MessageBox.Show("You didn't enter anything to replace with. Please Try Again");
            }
            else
            {
                Editor_TextBox.Text = Editor_TextBox.Text.Replace(Find_TextBox.Text, Replace_TextBox.Text);
            }
        }
        private void FindNext_Button_Click(object sender, EventArgs e)
        {
            if(!searchSubtask)
            {
                FindText();
            }
            else
            {
                SearchForSubtask();
            }
        }
        private void JsonEditor_Load(object sender, EventArgs e)
        {
            Serializer.SaveSettings();
        }
        private void ShowReplaceMenu_Button_Click(object sender, EventArgs e)
        {
            ShowReplaceMenu();
        }

        public int getWidth()
        {
            int w = 25;
            // get total lines of richTextBox1
            int line = Editor_TextBox.Lines.Length;
            if (line <= 99)
            {
                w = 20 + (int)Editor_TextBox.Font.Size;
            }
            else if (line <= 999)
            {
                w = 30 + (int)Editor_TextBox.Font.Size;
            }
            else
            {
                w = 50 + (int)Editor_TextBox.Font.Size;
            }

            return w;
        }
        public void AddLineNumbers()
        {
            Point pt = new Point(0, 0);
            int First_Index = Editor_TextBox.GetCharIndexFromPosition(pt);
            int First_Line = Editor_TextBox.GetLineFromCharIndex(First_Index);
            pt.X = ClientRectangle.Width;
            pt.Y = ClientRectangle.Height;

            int Last_Index = Editor_TextBox.GetCharIndexFromPosition(pt);
            int Last_Line = Editor_TextBox.GetLineFromCharIndex(Last_Index);
            tB_line.SelectionAlignment = HorizontalAlignment.Center;
            tB_line.Text = "";
            tB_line.Width = getWidth();
            for (int i = First_Line; i <= Last_Line + 2; i++)
            {
                tB_line.Text += i + 1 + "\n";   
            }
        }
        private void Editor_TextBox_SelectionChanged(object sender, EventArgs e)
        {
            Point pt = Editor_TextBox.GetPositionFromCharIndex(Editor_TextBox.SelectionStart);
            if (pt.X == 1)
            {
                AddLineNumbers();
            }
        }
        private void Editor_TextBox_VScroll(object sender, EventArgs e)
        {
            tB_line.Text = "";
            AddLineNumbers();
            tB_line.Invalidate();
        }
        private void Editor_TextBox_FontChanged(object sender, EventArgs e)
        {
            tB_line.Font = Editor_TextBox.Font;
            Editor_TextBox.Select();
            AddLineNumbers();
        }
        private void TB_line_MouseDown(object sender, MouseEventArgs e)
        {
            Editor_TextBox.Select();
            tB_line.DeselectAll();
        }
        private void Editor_TextBox_MouseClick(object sender, MouseEventArgs e)
        {
            //SearchForPairs();
        }
        private void SearchForPairs()
        {
            int duplicate = -1;
            string searchDirection = "";
            string searchText = "";

            int index = Editor_TextBox.SelectionStart;
            char[] ch = Editor_TextBox.Text.ToCharArray();
            char selectedText;
            int duplicatesFound = 0;

            endEditor = Editor_TextBox.Text.Length;
            startPosition = Editor_TextBox.SelectionStart;

            if (index - 1 < 0)
                index = 0;
            else if (index - 1 > Editor_TextBox.Text.Length)
                index = Editor_TextBox.Text.Length;
            selectedText = ch[index - 1];

            switch (selectedText)
            {
                case '[':
                    searchDirection = "down";
                    searchText = "]";
                    break;
                case ']':
                    searchDirection = "up";
                    searchText = "[";
                    break;
                case '{':
                    searchDirection = "down";
                    searchText = "}";
                    break;
                case '}':
                    searchDirection = "up";
                    searchText = "{";
                    break;
                case '(':
                    searchDirection = "down";
                    searchText = ")";
                    break;
                case ')':
                    searchDirection = "up";
                    searchText = "(";
                    break;
            }

            for (int i = 0; i < endEditor + 1; i = startPosition)
            {
                duplicate = -1;
                if (startPosition >= endEditor + 1 || i == -1)
                {
                    break;
                }
                else
                {
                    if (searchDirection == "down")
                    {
                        startPosition = Editor_TextBox.Find(searchText, index, endEditor + 1, RichTextBoxFinds.None);
                        if (startPosition >= 0)
                        {
                            //we found the character, lets make sure its the correct match to our pair
                            duplicate = Editor_TextBox.Find(selectedText.ToString(), index, startPosition + 1, RichTextBoxFinds.NoHighlight);
                            if (duplicate != -1)
                            {
                                duplicatesFound++;
                                index = duplicate + 1;
                            }
                            else
                            {
                                if (duplicatesFound != 0)
                                {
                                    index = startPosition + 1;
                                    duplicatesFound--;
                                }
                                else
                                    break;
                            }
                        }
                        else
                            break;
                    }
                    else
                    {
                        startPosition = Editor_TextBox.Find(searchText, 0, index, RichTextBoxFinds.Reverse);
                        if (startPosition >= 0)
                        {
                            //we found the character, lets make sure its the match
                            duplicate = Editor_TextBox.Find(selectedText.ToString(), startPosition, index - 1, RichTextBoxFinds.Reverse | RichTextBoxFinds.NoHighlight);
                            if (duplicate != -1)
                            {
                                duplicatesFound++;
                                index = duplicate - 1;
                            }
                            else
                            {
                                if (duplicatesFound != 0)
                                {
                                    index = startPosition - 1;
                                    duplicatesFound--;
                                }
                                else
                                    break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private void Close_button_Click(object sender, EventArgs e)
        {
            if (JsonEditor.jsonError == true)
            {
                DialogResult dialogResult = MessageBox.Show("ERROR!!! There is a JSON Error in this file!!!\n\nIf you leave the file now it will be corrupted and WILL break your mod. Do you still want to leave?", "ARE YOU SURE!!!!!", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }
        private void initSelContextMenu()
        {
            selMenu = new ContextMenuStrip();
            selMenu.Items.Add("Paste");
            selMenu.Items.Add("Find subtask");
            selMenu.Items.Add("Get current subtask number");
            selMenu.ItemClicked += jsonContextClicked;
        }
        private void initHighlightContextMenu()
        {
            highlightMenu = new ContextMenuStrip();
            highlightMenu.Items.Add("Copy");
            highlightMenu.Items.Add("Paste");
            highlightMenu.Items.Add("Find");
            highlightMenu.Items.Add("Replace");
            highlightMenu.Items.Add("Find subtask");
            highlightMenu.Items.Add("Get current subtask number");
            highlightMenu.ItemClicked += highlightContextClicked;
        }
        private void Editor_TextBox_RightClicked(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CharIndex_UnderMouse = GeneralMethods.CharIndex_UnderMouse(Editor_TextBox, e.X, e.Y);

                if (Editor_TextBox.SelectedText.Length > 0)
                {
                    highlightMenu.Show(Editor_TextBox, e.Location);
                }
                else if (Editor_TextBox.SelectedText.Length == 0)
                {
                    selMenu.Show(Editor_TextBox, e.Location);
                }
            }
        }
        private void jsonContextClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Paste")
            {
                try
                {
                    Editor_TextBox.SelectionStart = CharIndex_UnderMouse;
                    Editor_TextBox.Paste();

                }
                catch (Exception)
                {
                }
            }
            if (e.ClickedItem.Text == "Find subtask")
            {
                try
                {
                    ConsoleHandler.force_append_Notice("Please enter the subtask numbers you are looking for in the \"Find\" text box above.\n>> Example:    0,0,1");
                    searchSubtask = true;
                    ShowFindMenu();

                }
                catch (Exception)
                {
                }
            }
            if (e.ClickedItem.Text == "Get current subtask number")
            {
                if(jsonError == false)
                {
                    try
                    {
                        //func here
                        GetSubtaskNum();
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    ConsoleHandler.append_Force("JSON error detected... You need to fix the JSON error before you can get the subtask");
                }
            }
        }
        private void highlightContextClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Copy")
            {
                try
                {
                    Clipboard.SetText(Editor_TextBox.SelectedText);
                }
                catch (Exception)
                {
                }
            }
            if (e.ClickedItem.Text == "Paste")
            {
                try
                {
                    Editor_TextBox.Paste();
                }
                catch (Exception)
                {
                }
            }
            if (e.ClickedItem.Text == "Find")
            {
                try
                {
                    Find_TextBox.Text = Editor_TextBox.SelectedText;
                    ShowFindMenu();
                    FindText();
                }
                catch (Exception)
                {
                }
            }
            if (e.ClickedItem.Text == "Replace")
            {
                try
                {
                    Find_TextBox.Text = Editor_TextBox.SelectedText;
                    ShowReplaceMenu();
                }
                catch (Exception)
                {
                }
            }
            if (e.ClickedItem.Text == "Find subtask")
            {
                try
                {
                    ConsoleHandler.force_append_Notice("Please enter the subtask numbers you are looking for in the \"Find\" text box above.\n>> Example:    0,0,1");
                    searchSubtask = true;
                    ShowFindMenu();

                }
                catch (Exception)
                {
                }
            }
            if (e.ClickedItem.Text == "Get current subtask number")
            {
                if (jsonError == false)
                {
                    try
                    {
                        //func here
                        GetSubtaskNum();
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    ConsoleHandler.append_Force("JSON error detected... You need to fix the JSON error before you can get the subtask");
                }
            }
        }
        private void GetSubtaskNum()
        {
            string subtaskNum = JSON_Reader.GetSubtaskNum(CharIndex_UnderMouse, Editor_TextBox.Text);
            if (subtaskNum != "" && subtaskNum != " " && subtaskNum != null)
            {
                ConsoleHandler.append_Force_CanRepeat("Subtask:  [" + subtaskNum + " ]");
            }
            else
            {
                ConsoleHandler.append_Force("Unable to detect subtask. Please try clicking somewhere else...");
            }
        }
        private void SearchForSubtask()
        {
            int i = 0;
            bool found = false;
            foreach(char c in Editor_TextBox.Text)
            {
                if (c == ':')
                {
                    string subtaskNum = JSON_Reader.GetSubtaskNum(i, Editor_TextBox.Text);
                    if(subtaskNum.Replace(" ", "").Replace(",","") == Find_TextBox.Text.Replace(" ", "").Replace(",", ""))
                    {
                        found = true;

                        int startHighlighht = Editor_TextBox.Text.LastIndexOf("\"", i - 2);
                        Editor_TextBox.SelectionStart = i;
                        Editor_TextBox.Select(i, - (i - startHighlighht));
                        Editor_TextBox.ScrollToCaret();
                        HideFindButton();
                        searchSubtask = false;
                        break;
                    }
                }
                i++;
            }
            if (!found)
            {
                ConsoleHandler.append_Force_CanRepeat("That subtask was not found");
            }
        }
        private void FindSubtask_Button_Click(object sender, EventArgs e)
        {
            ConsoleHandler.force_append_Notice("Please enter the subtask numbers you are looking for in the \"Find\" text box above.\n>> Example:    0,0,1");
            searchSubtask = true;
            ShowFindMenu();
        }
        private int IndentNewLines()
        {
            int index = Editor_TextBox.GetFirstCharIndexOfCurrentLine();
            string text = Editor_TextBox.Text.Remove(0, index);

            int numSpace = 0;
            foreach(char c in text)
            {
                if (c != ' ')
                    break;
                else
                    numSpace++;
            }
            return numSpace;
        }
        private void RemoveEmptySpaces()
        {
            int numSpaces = IndentNewLines();
            int startIndex = Editor_TextBox.GetFirstCharIndexOfCurrentLine();
            int currentIndex = Editor_TextBox.SelectionStart;

            //if(currentIndex <= (startIndex + numSpaces))
            if(currentIndex <= (startIndex + numSpaces))
            {
                if (numSpaces > 5)
                {
                    Editor_TextBox.SelectionLength = 5;
                    Editor_TextBox.SelectionStart = currentIndex-5;
                    Editor_TextBox.SelectedText = "";
                }
            }
        }
        private void EZBoon_Button_Click(object sender, EventArgs e)
        {
            var ezBloon = new EZBloon_Editor();
            string path = Path;
            ezBloon.path = path;
            ezBloon.Show();
        }
    }
}
