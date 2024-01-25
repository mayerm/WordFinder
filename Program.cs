/*using BenchmarkDotNet.Running;
using WordFinder;*/

IEnumerable<IEnumerable<string>> smallSoup = 
    [
        ["H", "I", "P", "P", "O"],
        ["H", "I", "C", "E", "W"],
        ["H", "C", "A", "T", "L"],
        ["T", "Y", "T", "O", "X"],
        ["D", "O", "G", "Y", "O"]
    ];
IEnumerable<string> shortWords = ["hippo", "cat", "dog", "owl", "pet", "toy"];

var wf = new WordFinder.WordFinder(smallSoup.Select(x => string.Join("",x)).AsEnumerable());
var result = wf.Find(shortWords);

Console.WriteLine(string.Join(Environment.NewLine, result));
//Benchmark
//var benchmark = BenchmarkRunner.Run<WordFinderBenchmark>();


