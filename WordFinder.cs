using CommunityToolkit.Common;
using System.Globalization;


namespace WordFinder
{
    public class WordFinder
    {
        private readonly IEnumerable<string> _wordFinderMatrix;
        private readonly char[][] _wordFinderArray;
        private readonly Dictionary<string, int> foundWords = [];
        private readonly CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
        private readonly CompareOptions options = CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols;

        public WordFinder(IEnumerable<string> matrix)
        {  
            _wordFinderMatrix = matrix;
            _wordFinderArray = _wordFinderMatrix.Select(wfm => wfm.ToArray()).ToArray();

        }
        
        public IEnumerable<string> Find(IEnumerable<string> wordstream)
        {
            int[] analyzedColumnsIndexes = [];
            string[] analyzedWords = [];
            foreach (string word in wordstream)
            {
                if (IsWordInvalid(word) || analyzedWords.Contains(word, StringComparer.InvariantCultureIgnoreCase)) continue;
                analyzedWords = [.. analyzedWords, .. new string[] { word }];
                int index = 0;
                foreach (var row in _wordFinderMatrix)
                {
                    index = GetIndexFromString(row, word[0]);
                    if (index != -1)
                    {
                        ProcessRow(row, word);
                        analyzedColumnsIndexes = ProcessColumn(word, row, index, analyzedColumnsIndexes);
                    }
                }
                analyzedColumnsIndexes = [];
            }
            return foundWords.OrderByDescending(x => x.Value).Take(10).Select(x => x.Key);
            
        }
        private void ProcessRow(string row, string word)
        {
            ProcessVector(row, word);
        }
        private int[] ProcessColumn(string word, string row, int index, int[] analyzedColumnsIndexes)
        {
            do
            {
                if (!analyzedColumnsIndexes.Contains(index))
                {
                    analyzedColumnsIndexes = [.. analyzedColumnsIndexes, .. new int[] { index }];
                    var column = string.Join("", ArrayExtensions.GetColumn(_wordFinderArray, index));
                    ProcessVector(column, word);
                }
                index = GetIndexFromString(row, word[0], index + 1);
            }
            while (index != -1);
            return analyzedColumnsIndexes;
        }
 
        private void ProcessVector(string vector, string word)
        {
            if (vector.Length < word.Length) return;
            int index = 0;
            do
            {
                index = GetIndexFromString(vector, word, index);
                if (index != -1)
                {
                    UpdateFoundWords(word);
                    index++;
                }
            }
            while ( index != -1);
        }

        private void UpdateFoundWords(string word)
        {
            if (!foundWords.TryAdd(word, 1)) 
                foundWords[word] += 1;
        }

        private int GetIndexFromString(string sourceStr, string searchTerm, int index = 0)
        {
            return compareInfo.IndexOf(sourceStr, searchTerm, index, options);
        }

        private int GetIndexFromString(string sourceStr, char searchTerm, int index = 0)
        {
            return compareInfo.IndexOf(sourceStr, searchTerm, index, options);
        }

        private bool IsWordInvalid(string word)
        {
            return word.Length < 2 || !word.All(char.IsAsciiLetter);
        }
    }
}
