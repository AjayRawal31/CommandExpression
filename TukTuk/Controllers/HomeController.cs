using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TukTuk.Models;
using System.Text.RegularExpressions;

namespace TukTuk.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Deleting the words in list  from paragraph
        /// </summary>
        /// <param name="command"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public List<string> Delete(string command, List<string> list)
        {
            int d = command.IndexOf("d");
            string commandLine = command.Substring(d+1).Trim();
           
            int line =Convert.ToInt32(commandLine);
            if (list.Count < line)
                throw new Exception("line doesn't exits for Delete");

            command = command.Substring(d + 3);
            list.RemoveAt(line - 1);
            return list;

        }

        public List<string> Insert(string command, List<string> list)
        {
            string text;
            int i = command.IndexOf("i");
            string oldstringMultiple = command.Substring(i + 1);
            string[] oldmultiple = oldstringMultiple.Split('"');
            
            int line = Convert.ToInt32(oldmultiple[0]);
            text = oldmultiple[1];
            text = line == 1 ? text + "\n" : "\n" + text;

            if (list.Count < line)
                throw new Exception("line doesn't exits for Insert");

            list.Insert(line - 1, text);
            return list;
        }

        public List<string> SearchReplace(string command, List<string> list)
        {
            string oldText;
            string newText;
            string[] commandValues = { };
            string flag = string.Empty;

            int sr = command.IndexOf("sr");

            string srCommand = command.Substring(sr);
            if (srCommand.Contains("'"))
            {
                commandValues = srCommand.Split('\'');
                flag = commandValues[4];

            }
            else if (srCommand.Contains("\""))
            {
                commandValues = srCommand.Split('\"');
                if (!(commandValues[1].Contains(" ")) || !(commandValues[3].Contains(" ")))
                    throw new Exception("for single characters or words use single quotation");
                flag = commandValues[4];
            }

            oldText = commandValues[1];
            newText = commandValues[3];

            if ((flag == "" && commandValues.Length == 5) || flag.Contains("rev"))
            {
                for (int i = 0; i <list.Count; i++)
                    list[i] = list[i].Replace(oldText, newText);
            }
            else
            {
                int index = Convert.ToInt32(commandValues[4]);
                if (list.Count < index)
                    throw new Exception("line doesn't exits for Search & Replace");

                list[index - 1] = list[index - 1].Replace(oldText, newText);
            }
            return list;
        }

        public List<string> Reverse(string command, List<string> list)
        {
            int rev = command.IndexOf("rev");
            string reverseCommand = command.Substring(rev+3).Trim();
        
            if (reverseCommand!= "")
            {
                int line = Convert.ToInt32(reverseCommand);

                if (list.Count < line)
                    throw new Exception("line doesn't exits for Reverse");

                char[] rLine = list[line - 1].ToCharArray();

                if (line == 1)
                    Array.Reverse(rLine);
                else
                    Array.Reverse(rLine, 1, rLine.Length - 1);

                list[line - 1] = new string(rLine);
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    char[] rline =list[i].ToCharArray();
                    Array.Reverse(rline);
                    list[i] = new string(rline);
                }

            }
            //   command = command.Substring(rev + 3);
            return list;

        }

        public void ExecuteCommand(string commands, List<string> list)
        {
            string regexDelete = @"^[d]\s+\d$";                 //d 10
            string regexInsert= @"^[i]\s+\d\s+"".*""$";         //i 10 "Hello World"
            string regexSR4 = @"^(sr)\s+"".*""\s+"".*""\s+\d$";         //sr "Hello world" "Welcome to the world" 10
            string regexSR3 = @"^(sr)\s+"".*""\s+"".*""$";          //sr "Hello world" "Welcome to the world"
            string regexSR2 = @"^(sr)\s+'.*'\s+'.*'\s+\d$";     //sr 'Hi' 'Hello' 10
            string regexSR1 = @"^(sr)\s+'.*'\s+'.*'$";          // sr 'i' 'e'
            string regexRev1 = @"^(rev)\s+\d$";                 //rev 10
            string regexRev2 = @"^rev$";                        //rev

            Regex delete = new Regex(regexDelete);
            Regex insert = new Regex(regexInsert);
            Regex SearchReplace1 = new Regex(regexSR1);
            Regex SearchReplace2 = new Regex(regexSR2);
            Regex SearchReplace3 = new Regex(regexSR3);
            Regex SearchReplace4 = new Regex(regexSR4);
            Regex reverse1 = new Regex(regexRev1);
            Regex reverse2 = new Regex(regexRev2);

            commands = commands.Trim();

            if (delete.IsMatch(commands))
                Session["result"] = Delete(commands, list);

            else if (insert.IsMatch(commands))
                Session["result"] = Insert(commands, list);

            else if (SearchReplace1.IsMatch(commands) || SearchReplace2.IsMatch(commands) || SearchReplace3.IsMatch(commands) || SearchReplace4.IsMatch(commands))
                Session["result"] = SearchReplace(commands, list);

            else if (reverse1.IsMatch(commands) || reverse2.IsMatch(commands))
                Session["result"] = Reverse(commands, list);
 
            else
                throw new Exception("Please enter commands in correct format");
          
        }

        [HttpPost]
        public ActionResult Index(PasteModel TextContent)
        {
            try
            {
                if (TextContent.Paste == null || TextContent.Paste == string.Empty)
                    throw new Exception("Textarea can't be empty");

                else if (TextContent.Command == null || TextContent.Command == string.Empty)
                    throw new Exception("Command field can't be empty");
                
                string[] lineWiseCommands = { };
                string textLines = TextContent.Paste;

                List<string> list = textLines.Split('\r').ToList();

                string command = TextContent.Command.Trim();
                command = Regex.Replace(command, @"[\r\n]+", "\r\n");
               
                if (command.Length == 0)
                    throw new Exception("Please enter command");
            
                if (command.Contains("\r\n"))
                {
                    string[] separator = { "\r\n" };
                    lineWiseCommands = command.Split(separator, StringSplitOptions.None);
                    int count = 0;
                    while (count < lineWiseCommands.Length)
                    {
                        ExecuteCommand(lineWiseCommands[count], list);
                        count++;
                    }

                }//for single command
                else
                    ExecuteCommand(command, list);
               
            }
            catch (Exception ex)
            {
                ViewBag.error = ex.Message;
            }

            return View();

        }       
    }
}