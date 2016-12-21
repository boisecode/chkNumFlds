using System;
using System.IO;
using System.Collections;
using System.Windows;
using Microsoft.Win32;
using System.Text;
using System.Text.RegularExpressions;


namespace CheckNumFields
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
            bool bCheckIt = true;
            string strProblems;
            if (bCheckIt == true)
            {
                strProblems = CheckFile(openFileDialog.FileName);
                MessageBox.Show(strProblems);
            }
        }

        private string CheckFile(string inFname)
        {
            StringBuilder sbResults = new StringBuilder();
            StringBuilder sbDebug = new StringBuilder();
            ArrayList aal_BadLines = new ArrayList();
            string line = null;
            string inFileName = "C:\\tmp\\StatisticTypes.txt";
            string outFileName = "Default";
            bool bDone = false;
            bool bNotFound = false;
            int nRead = 0, nFlds = 0, nPrevFlds = 0;
            int nExpected=0, nLnumErr=0, nBadLines=0;
            char[] delim = { '\t' };
            Regex TXT = new Regex(@"\.txt$", RegexOptions.IgnoreCase);
            Regex CSV = new Regex(@"\.csv$", RegexOptions.IgnoreCase);
        
            inFileName = inFname; 
            if (inFileName.Length>4)
                outFileName = "cnumf_" + inFname.Substring(0, 4) + ".txt"; // will get _rpt infix
            else
                outFileName = "cnumf_"+ inFname+".txt"; // will get _rpt infix

            if (!File.Exists(inFileName)) bNotFound = true;
            if (bNotFound)
            {
                sbResults.AppendLine("not found: "+inFileName);
                bDone = true;
                return sbResults.ToString();
            }
            else
            {
                if (TXT.IsMatch(inFileName))
                {
                    MatchCollection TmatchList = TXT.Matches(inFileName);
                    Match first = TmatchList[0];
                    sbDebug.AppendLine("first match: " + first.ToString());

                    outFileName = Regex.Replace(inFileName, first.ToString(), "_rpt");
                    sbDebug.AppendLine("interim: " + outFileName);
                    Group grp;
                    grp = first.Groups[0];
                }

                if (CSV.IsMatch(inFileName))
                {
                    MatchCollection CmatchList = CSV.Matches(inFileName);
                    Match first = CmatchList[0];
                    outFileName = Regex.Replace(inFileName, first.ToString(), "_rpt");
                }
                outFileName += ".txt";
                sbDebug.AppendLine("outFileName is " + outFileName);
            }
            if (bDone)
                return ("sudden death");
            else 
            {
                TextReader tr = new StreamReader(inFileName);
                TextWriter tw = new StreamWriter(outFileName);
                sbResults.AppendLine("# DateTime: "+DateTime.Now.ToString());
                sbResults.AppendLine("# InputFile: " + inFileName + "   ");

                Regex rgCommas = new Regex("^.*,.*,.*$");  // at least 3 flds
                Regex rgColons = new Regex("^.*:.*:.*$");  // at least 3 flds
                Regex rgSemiColons = new Regex("^.*;.*;.*$");  // at least 3 flds
                while ((line = tr.ReadLine()) != null)
                {
                    nRead++;
                    if (nRead == 1)
                    {
                        if (rgCommas.IsMatch(line))
                        {
                            delim[0] = ',';
                            sbResults.AppendLine("# Delimiter: COMMA (,)");
                        }
                        if (rgColons.IsMatch(line))
                        {
                            delim[0] = ':';
                            sbResults.AppendLine("# Delimiter: COMMA (,)");
                        }
                        if (rgSemiColons.IsMatch(line))
                        {
                            delim[0] = ';';
                            sbResults.AppendLine("# Delimiter: SEMICOLON (;)");
                        }
                    }
                    string[] aaFlds = line.Split(delim);
                    nFlds = aaFlds.GetLength(0);
                    if (nRead == 2)
                    {
                        nExpected = nFlds;
                    }
                    if (nRead != 1 && nFlds != nPrevFlds)
                    {
                        if (!(nRead == nLnumErr + 1 && nFlds == nExpected))
                        {
                            nBadLines++;
                            nLnumErr = nRead;
                            tw.Write("\t"+"line " + nRead.ToString() + "  ");
                            tw.Write("\t" + "expected " + nExpected.ToString() + "  ");
                            tw.WriteLine("\t" + "prev " + nPrevFlds.ToString() + "  curr " + nFlds.ToString());
                            tw.WriteLine(line+"\n");
                        }
                    }
                    nPrevFlds = nFlds;
                }

                tr.Close();
                tw.Close();

                sbResults.Append("nRead: " + nRead.ToString() + " nFlds: " + nFlds.ToString() ); 
                sbResults.AppendLine(" nBadLines: " + nBadLines.ToString() +"\n");
                TextReader trInterim = new StreamReader(outFileName);
                sbResults.AppendLine(trInterim.ReadToEnd() );
                trInterim.Close();
                //-( Delete the file? Or announce it in the MessageBox?
                return sbResults.ToString();
            }
        }
    }
}
