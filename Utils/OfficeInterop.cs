using ChoreMgr.Models;
using Word = Microsoft.Office.Interop.Word;
namespace ChoreMgr.Utils
{

    public class OfficeInterop
    {
        Word.Application _appWord = null;
        internal List<Quote> ReadWord()
        {
            DanLogger.Log("Read from Word");
            //var filename = $"weekly{DateTime.Today.ToString("yyyyMM")}.txt";
            if (_appWord == null)
                _appWord = new Word.Application();

            DanLogger.Log("Open file");
            var word = _appWord.Documents.Open(@"F:\OneDrive\Dan\weekly51.docx", true, true);
            var rv = new List<Quote>();
            var quote = new Quote(Guid.NewGuid().ToString());
            foreach (Word.Paragraph p in word.Paragraphs)
            {
                var text = p.Range.Text.Trim();
                //var range = p.Range;
                //var format = p.Range.ParagraphStyle as Word.ParagraphFormat;
                //var style = p.Range.ParagraphStyle as Word.Style;
                if (!quote.Lines.Any() && text.Length < 12)
                {
                    DanLogger.Log("Ignore " + text);
                    continue;
                }
                var after = p.SpaceAfter;
                quote.Lines.Add(text);
                if (after > 0)
                {
                    rv.Add(quote);
                    //ADSearcher.Log(string.Join("|", block));
                    quote = new Quote(Guid.NewGuid().ToString());
                }
            }
            word.Close();
            //File.WriteAllText($"weekly{DateTime.Today.ToString("yyyyMM")}.txt", string.Join($"{Environment.NewLine}-{Environment.NewLine}", blocks));


            //ADSearcher.Log(string.Join("|", block));
            return rv;
        }
    }
}
