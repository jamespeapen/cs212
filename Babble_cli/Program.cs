using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using static Hashtable;

class Babble
{
    string input;
    String[] words;
    int wordCount = 400;
    int order = 0;
    Dictionary<string, ArrayList> hashtable;
    void getFile(string path) 
    {   
        input = System.IO.File.ReadAllText(path);
        words = Regex.Split(input, @"\s+");
    }

    int getWords()
    {
        return words.Length;
    }

    int getUniqueWords()
    {   
        return Hashtable.getUniqueWordCount();
    }

    void getHashtable(int order) 
    {
        hashtable = Hashtable.makeHashTable(hashtable, words, order);
    }

    void dump()
    {
        foreach (KeyValuePair<string, ArrayList> entry in hashtable)
        {
            Console.Write("{0} --> ", entry.Key);
            for (int i = 0; i < entry.Value.Count; i++)
            {
                Console.Write("{0} ", entry.Value[i]);
            }
            Console.WriteLine();
        }
    }


    void speak(Dictionary<string, ArrayList> hashtable)
    {   
        Random r = new Random();

        KeyValuePair<string, ArrayList> first_entry = hashtable.First();    //get the first entry in the hashtable
        int rand_index = r.Next(0, first_entry.Value.Count);                //get a random index 
        string key = Convert.ToString(first_entry.Key);                     //get the key at the first index
        string next_word =  Convert.ToString(first_entry.Value[rand_index]);//get a random word from the arraylist for that key
        string first_line = key + next_word;
        Console.Write(first_line + " ");
       
        String[] split_string = first_line.Split(" ");
        String[] new_key;
        string current_line;
        if (order == 0)
        {
            foreach (string word in words)
            {
                Console.Write(word + " ");
            }
        }

        else if (order == 1) 
        {
            next_word += " ";
            for (int i = 0; i < wordCount; i++)
            {
                rand_index = r.Next(hashtable[next_word].Count);
                next_word = Convert.ToString(hashtable[next_word][rand_index]) + " ";
                Console.Write(next_word);
            }
        }

        else 
        {
            for (int i = 0; i < wordCount; i++) 
            {   
                new_key = split_string[1..(split_string.Length)];
                key = string.Join(" ", new_key) + " ";
                rand_index = r.Next(0, hashtable[key].Count);

                next_word = Convert.ToString(hashtable[key][rand_index]);
                current_line = key + next_word;
                Console.Write(next_word + " ");
                split_string  = current_line.Split(" ");
            }
        }
    }
    static void Main(string[] args) 
    {
        Babble babbler = new Babble();

        Console.WriteLine("Enter path to file: ");
        string path = Console.ReadLine();
        
        babbler.getFile(path);
        Console.WriteLine("Got file");
        Console.WriteLine("Number of words: " + babbler.getWords());
        Console.WriteLine("Enter order of analysis: ");
        babbler.order = Convert.ToInt32(Console.ReadLine());
        babbler.getHashtable(babbler.order);
        Console.WriteLine("Number of unique sequences: " + babbler.getUniqueWords());   //get unique sequences
        Console.ReadLine();
        // babbler.dump();
        babbler.speak(babbler.hashtable);
        Console.WriteLine();
        Console.WriteLine("Done!");
    }

}
