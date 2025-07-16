using InkTrack.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Word = Microsoft.Office.Interop.Word;

namespace InkTrack.Helpers
{
    public class WordHelper
    {

        FileInfo fileInfo;
        string filePath;

        public WordHelper(string filePath)
        {
            this.filePath = filePath; 

            if (File.Exists(filePath))
            {
                fileInfo = new FileInfo(filePath);
            }
            else
            {
                throw new FileNotFoundException($"Файл \"{filePath}\" не найден в директории программы, переустановите программу для восстановления.");
            }
        }

        public void GenerateFileWord(string FULL_NAME, string CARTRIDGE_NUMBER, string DEVICE_NAME, string ROOM_NAME, string SUGGESTION)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>()
            {
                {"<FULL_NAME>", FULL_NAME },
                {"<CARTRIDGE_NUMBER>", CARTRIDGE_NUMBER },
                {"<DEVICE_NAME>", DEVICE_NAME },
                {"<ROOM_NAME>", ROOM_NAME },
                {"<SUGGESTION>", SUGGESTION },
                {"<DATE>", DateTime.Now.ToString("dd MMMM yyyyг.") }
            };

            Process(dictionary);
        }
        bool Process(Dictionary<string, string> items)
        {
            string pathToDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Word.Application app = null;
            try
            {
                app = new Word.Application();
                Object file = fileInfo.FullName;

                Object missing = Type.Missing;

                app.Documents.Open(file);

                foreach ( var item in items )
                {
                    Word.Find find = app.Selection.Find;
                    find.Text = item.Key;
                    find.Replacement.Text = item.Value;

                    Object wrap = Word.WdFindWrap.wdFindContinue;
                    Object replace = Word.WdReplace.wdReplaceAll;

                    find.Execute(
                        FindText: Type.Missing,
                        MatchCase: false,
                        MatchWholeWord: false,
                        MatchWildcards: false,
                        MatchSoundsLike: missing,
                        MatchAllWordForms: false,
                        Forward: true,
                        Wrap: wrap,
                        Format: false,
                        ReplaceWith: missing,
                        Replace: replace);
                }

                Object newFileName = Path.Combine(pathToDesktop, $"Заявка на заправку картриджа от {DateTime.Now:dd.MM.yyyy}.docx");
                app.ActiveDocument.SaveAs2(newFileName);
                app.ActiveDocument.Close();
                app.Quit();
                System.Diagnostics.Process.Start(new ProcessStartInfo(Path.Combine(pathToDesktop, $"Заявка на заправку картриджа от {DateTime.Now:dd.MM.yyyy}.docx")) { UseShellExecute = true });

                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Error", "Ошибка при генерации документа", e);
            }
            finally
            {
                app.Quit();
            }
            return false;
        }
    }
}
