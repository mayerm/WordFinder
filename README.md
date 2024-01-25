# Word Finder
This project can be used to solve **m** x **n** word finders (also known as "Letter Soups"), but its **core functionality** is to ***find the top 10 repeated words from the most repeated to the least one*** inside a word finder from a defined set of words. 
## How to use it?
You only need to create an instance of `WordFinder` and pass the word finder with an `IEnumerable<string>` type as an argument, and then execute the `Find(IEnumerable<string> words)` method from said instance with the words you want to check if they are inside of the word finder.
### Here's an example: 
    IEnumerable<string> wfMatrix = ["ADG","DEF","GHI"]
    IEnumerable<string> words = ["DE", "ZH", "ADG"];
    WordFinder wordFinder = new WordFinder(wfMatrix);
    IEnumerable<string> results = wordFinder.Find(words);
    
The expected results are `["ADG", "de"]` because `"ADG"` appears 2 times; `"de"` appears only once and there is no word `"ZH"` inside the current word finder.

## Limitations:
* The current implementation only checks words from left to right horizontally and from top to bottom vertically, though reversed row/column checking can be easily implemented.
* It only takes strings that contains 2 characters at least by design.
* It ignores any duplicate words to avoid unnecessary loops
* It ignores symbols, as this implementation only takes into consideration letters from the latin alphabet.
* It ignores case, so it will consider plane, Plane, PLANE and its variants as the same word.

## Dependencies: 
This project uses **[CommunityToolkit.Common](https://www.nuget.org/packages/CommunityToolkit.Common)** for getting the column in an efficient way.
It also contains an **xUnit** project and a **BenchmarkDotNet** dependency, but are used for testing and checking performance respectively, and are not needed for the correct operation of the program.

## How it works?
The `WordFinder` class grabs the introduced word finder array and keeps it for that instance, as well as having filtering options for the rows and columns and a `Dictionary<string, int> foundWords` to count the amount of times a word is found inside the word finder.

### Private variables:
**Summary**: 
* `_wordFinderMatrix` contains the matrix that represents the word finder.
* `foundWords` is going to contain all the words defined in the wordstream variable that are found inside the matrix 
* `compareInfo` is used together with `options` for getting the `IndexOf()` a substring inside a string, but with defined comparison options (which is what it allows to ignore case and symbols).
* `_wordFinderArray` is used to avoid converting the `_wordFinderMatrix` into a char[][] each time a column is needed.

**Implementation**:


    private readonly IEnumerable<string> _wordFinderMatrix;
    private readonly char[][] _wordFinderArray;
    private readonly Dictionary<string, int> foundWords = [];
    private readonly CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
    private readonly CompareOptions options = CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols;

### `Find()` method:

We create the `analyzedWords` and `analyzedColumnsIndexes` arrays to *avoid checking previously analyzed rows/columns*, in order to improve performance, and then we *start looping through the* `wordstream`:

    public IEnumerable<string> Find(IEnumerable<string> wordstream)
    {
        int[] analyzedColumnsIndexes = [];
        string[] analyzedWords = [];
        foreach (string word in wordstream)
        {
Then we discard invalid (such as numbers, symbols, etc) or previously analyzed words by _skipping to the next iteration from_ `wordstream`. 
If it's a valid word and not already inside the `analyzedWords` array, then we **add it** to said array and we _start checking the matrix_ by creating an int variable containing the default value of `index` of 0, and then _looping through the rows_ from the matrix:

		    if (IsWordInvalid(word) ||
			     analyzedWords.Contains(
				     word,
				     StringComparer.InvariantCultureIgnoreCase)) 
				continue;
		    analyzedWords = [.. analyzedWords, .. new string[] { word }];
		    int index = 0;
		    foreach (var row in _wordFinderMatrix)
		    {
Here we check *if the first letter of the **word** is contained inside the **row***. If it doesn't (that is, the `index` *equals -1*), we *continue to the next row*.
Otherwise, we know we can find a possible word either in the row or the column, so we use the returned `index` to call `ProcessRow()`, which loops everytime an `index` *is found in a row containing the passed word*, and said *word is stored inside* `foundWords`.
Then, `ProcessColumn()` is called to check the column from the index obtained from the row, and works in a similar fashion as `ProcessRow()`, but it returns an array containing the indexes from the *previously analyzed columns for the current word*, and it updates every time a new column is analyzed, in order to *avoid checking the same column multiple times*.
After looping through the matrix for the current word, the `analyzedColumnsIndexes` array is emptied so we can reuse it for a new word.

			    index = GetIndexFromString(row, word[0]);
			    if (index != -1)
			    {
				    ProcessRow(row, word);
			        analyzedColumnsIndexes = ProcessColumn(word, row, index, analyzedColumnsIndexes);
			    }
	        }
	            analyzedColumnsIndexes = [];
	    }
      
Finally, we grab the `foundWords` dictionary; we order it by most repeated word; then take the first 10 elements and then select them to return it as an `IEnumerable<string>`
  
        return foundWords.OrderByDescending(x => x.Value).Take(10).Select(x => x.Key);
    }
## Private methods

### `bool IsWordInvalid(string word)`:
**Summary**: It checks if it's a word, since the minimum length of a word is 2 characters (words like **'do'** or **'if'**), and if the word contains only alphabetic characters. 

**Implementation**:

    private bool IsWordInvalid(string word)
    {
        return word.Length < 2 || !word.All(char.IsAsciiLetter);
    }

### `int GetIndexFromString(string sourceStr, string searchTerm, int index=0)`: 

**Summary**: It gets the `index` of the `searchTerm` string inside the `sourceStr`, starting from a certain `index`, and with the `IgnoreSymbols`, `IgnoreCase` and `InvariantCulture` options; or returns -1 if the `index` is not found.
If no index is passed as argument, then index gets the default value to 0, meaning *it starts looking inside* `sourceStr` *from the beginning*.
Used to get words from entire rows instead of looping each char.

**Implementation**:

    private int GetIndexFromString(string sourceStr, string searchTerm, int index = 0)
    {
        return compareInfo.IndexOf(sourceStr, searchTerm, index, options);
    }

### `int GetIndexFromString(string sourceStr, string searchTerm, int index=0)`: 

**Summary**: Overload of `GetIndexFromString(string sourceStr, string searchTerm, int index=0)`, but it takes a char instead of a string as searchTerm.
Used to check if the first character of a word is present in an array for row/column processing; also to avoid unnecesary conversions.

**Implementation**:

    private int GetIndexFromString(string sourceStr, char searchTerm, int index = 0)
    {
        return compareInfo.IndexOf(sourceStr, searchTerm, index, options);
    }

### `void ProcessVector(string vector, string word)`: 

**Summary**: It searches through the string `vector` (which is a row or a column from the matrix) to check if it contains the string `word`. 
If the `word` has a bigger `length` than the `vector`, then the `word` is *discarded* by exiting the method since *the vector cannot contain a string larger than itself*.
Then it enters a `do-while` loop to check for each time an `index` is found through `GetIndexFromString()`. 
If `index` returns -1, then the loop ends. Otherwise, then the `word` exists inside the `vector` and the `UpdateFoundWords()` method is called to keep track of the found words. Then we add +1 to the `index` to keep analyzing the `vector` starting from the new `index` value.
**Implementation:**

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
### `void ProcessRow(string vector, string word)`: 

**Summary**: It calls ProcessVector(). This method does not bring any functionality; it's here to separate responsabilities, and bring maintainability and readability.

**Implementation**:

        private void ProcessRow(string row, string word)
        {
            ProcessVector(row, word);
        }
        
### `int[] ProcessColumn(string word, string row, int index, int[] analyzedColumnsIndexes)`: 

**Summary**: Through a do-while, it checks if current `index` of the `row` is contained inside the `analyzedColumnsIndexes` array. If it isn't, then its added to the array, and it gets the entire column of the matrix from said index using `ArrayExtensions.GetColumn(string[][] matrix, index);`.
Since it requires a 2D array, a conversion is needed for the rows and columns, which is the reason the `_wordFinderArray` exists.
Then it calls `ProcessVector()` and it sends column and word as arguments for processing.
Whether the index is found or not in the `analyzedColumnsIndexes` array, it tries to find the next non -1 index to continue the loop.
After the loop ends, it returns the `analyzedColumnsIndexes` array in order to keep track of already analyzed columns to prevent unnecessary iterations.
        
        private int[] ProcessColumn(string word, string row, int index, int[] analyzedColumnsIndexes)
        {
            do
            {
                if (!analyzedColumnsIndexes.Contains(index))
                {
                    analyzedColumnsIndexes = [.. analyzedColumnsIndexes, .. new int[] { index }];
                    var column = string.Join("", ArrayExtensions.GetColumn(_wordFinderMatrix.Select(wfm => wfm.ToArray()).ToArray(), index));
                    ProcessVector(column, word);
                }
                index = GetIndexFromString(row, word[0], index + 1);
            }
            while (index != -1);
            return analyzedColumnsIndexes;
        }

### `void UpdateFoundWords(string word)`: 

**Summary**: It takes the string `word` and tries to add it to the `foundWords` dictionary with the `Value` of 1, since it appeared one time so far. 
If adding it to the dictionary fails, it means it exists, so we use the `word` as `Key` and add +1 to its `Value`.

**Implementation**:

    private void UpdateFoundWords(string word)
    {
        if (!foundWords.TryAdd(word, 1)) 
            foundWords[word] += 1;
    }

## Benchmark

A single benchmark has been taken into consideration where the amount of words are over 100 and the word finder is a 64x64 matrix.
Several methods and code has been used, and with the implementation of `ArrayExtensions` and code redundancy reduction and refactoring, the final runtime ended up being of around 150 ns.

## Feedback

If you have any feedback on how to improve this project, suggested tests or anything you'd like to share, leave a comment. I'd really appreciate it.




