﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Bingo
{
    class Program
    {
        private static RelationshipGraph rg;

        // Read RelationshipGraph whose filename is passed in as a parameter.
        // Build a RelationshipGraph in RelationshipGraph rg
        private static void ReadRelationshipGraph(string filename)
        {
            rg = new RelationshipGraph();                           // create a new RelationshipGraph object

            string name = "";                                       // name of person currently being read
            int numPeople = 0;
            string[] values;
            Console.Write("Reading file " + filename + "\n");
            try
            {
                string input = System.IO.File.ReadAllText(filename);// read file
                input = input.Replace("\r", ";");                   // get rid of nasty carriage returns 
                input = input.Replace("\n", ";");                   // get rid of nasty new lines
                string[] inputItems = Regex.Split(input, @";\s*");  // parse out the relationships (separated by ;)
                foreach (string item in inputItems)
                {
                    if (item.Length > 2)                            // don't bother with empty relationships
                    {
                        values = Regex.Split(item, @"\s*:\s*");     // parse out relationship:name
                        if (values[0] == "name")                    // name:[personname] indicates start of new person
                        {
                            name = values[1];                       // remember name for future relationships
                            rg.AddNode(name);                       // create the node
                            numPeople++;
                        }
                        else
                        {
                            rg.AddEdge(name, values[1], values[0]); // add relationship (name1, name2, relationship)

                            // handle symmetric relationships -- add the other way
                            if (values[0] == "hasSpouse" || values[0] == "hasFriend")
                                rg.AddEdge(values[1], name, values[0]);

                            // for parent relationships add child as well
                            else if (values[0] == "hasParent")
                                rg.AddEdge(values[1], name, "hasChild");
                            else if (values[0] == "hasChild")
                                rg.AddEdge(values[1], name, "hasParent");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("Unable to read file {0}: {1}\n", filename, e.ToString());
            }
            Console.WriteLine(numPeople + " people read");
        }

        // Show the relationships a person is involved in
        private static void ShowPerson(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
                Console.Write(n.ToString());
            else
                Console.WriteLine("{0} not found", name);
        }

        //Bingo to show a relationship between two people
        private static void Bingo(string person_1, string person_2)
        {
            GraphNode person1 = rg.GetNode(person_1);
            person1.Label = "visited";                       //set as visited so cycles are not allowed
            GraphNode person2 = rg.GetNode(person_2);
            Dictionary<GraphNode, GraphNode> dict = new Dictionary<GraphNode, GraphNode>();     //dict to store each nodes predecessor
            Dictionary<GraphNode, GraphEdge> dict2 = new Dictionary<GraphNode, GraphEdge>();
            Queue<GraphEdge> queue = new Queue<GraphEdge>();                                    //queue for BFS
            List<GraphEdge> first = person1.GetEdges();

            //check for immediate bingo
            foreach (GraphEdge e in first)
            {
                if (rg.GetNode(e.To()) == person2)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }
                queue.Enqueue(e);
                rg.GetNode(e.To()).Label = "visited";
                dict.Add(rg.GetNode(e.To()), person1);
                dict2.Add(rg.GetNode(e.To()), e);
            }

            bool broken = false;                                        //check if person2 is found and inner loop is broken
            while (queue.Count > 0)
            {
                GraphEdge next_edge = queue.Dequeue();
                GraphNode next_person = rg.GetNode(next_edge.To());
                foreach (GraphEdge e in next_person.GetEdges())
                {
                    if (rg.GetNode(e.To()) == person2)
                    {
                        dict.Add(person2, next_person);
                        dict2.Add(person2, e);
                        broken = true;
                        break;
                    }
                    else
                    {
                        if (rg.GetNode(e.To()).Label != "visited")
                        {
                            queue.Enqueue(e);
                            dict.Add(rg.GetNode(e.To()), next_person);
                            dict2.Add(rg.GetNode(e.To()), e);
                            rg.GetNode(e.To()).Label = "visited";
                        }
                    }
                }
                if (broken)
                    break;
            }

            //traceback to person1
            GraphNode parent = dict[person2];
            GraphNode child = person2;

            //get non-symmetric relationships
            if (child.GetRelationship(parent.Name) == null)
                Console.WriteLine(parent.GetRelationship(child.Name));
            else
                Console.WriteLine(child.GetRelationship(parent.Name).ToString());

            while (parent != person1)
            {
                child = parent;
                parent = dict[parent];

                //get non-symmetric relationships
                if (child.GetRelationship(parent.Name) == null)
                    Console.WriteLine(parent.GetRelationship(child.Name));
                else
                    Console.WriteLine(child.GetRelationship(parent.Name).ToString());
            }

            reset_label();
        }

        // Show a person's friends
        private static void ShowFriends(string name)
        {
            GraphNode n = rg.GetNode(name);
            if (n != null)
            {
                Console.Write("{0}'s friends: ", name);
                List<GraphEdge> friendEdges = n.GetEdges("hasFriend");
                foreach (GraphEdge e in friendEdges)
                {
                    Console.Write("{0} ", e.To());
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("{0} not found", name);
        }

        //Display all orphans in the dataset by checking each nodes edges for parent relationship
        private static void ShowOrphans()
        {
            bool no_orphans = true;
            foreach (GraphNode n in rg.nodes)
            {
                if (n.GetEdges("hasParent").Count == 0)
                {
                    Console.Write(n.Name + " ");
                    no_orphans = false;
                }

            }
            if (no_orphans)
                Console.WriteLine("No orphans found");
        }

        //show siblings
        private static void ShowSiblings(string name)
        {
            GraphNode child = rg.GetNode(name);
            List<GraphNode> parents = rg.GetParentNodes(name);
            List<GraphNode> siblings = new List<GraphNode>();

            //check each parent if there is more than one
            foreach (GraphNode parent in parents)
            {
                foreach (GraphNode sibling in rg.GetChildNodes(parent.Name))
                {
                    if (sibling != child)           //exclude queried child
                        siblings.Add(sibling);
                }
            }

            //no siblings
            if (siblings.Count < 1)
            {
                Console.WriteLine(name + " has no siblings");
                return;
            }

            Console.WriteLine("Siblings of " + child.Name + ": ");
            foreach (GraphNode sibling in siblings)
                Console.Write(sibling.Name + " ");

            Console.WriteLine();
        }

        //show descendants
        private static void ShowDescendants(string name)
        {
            //check if node exists
            if (rg.GetNode(name) == null)
            {
                Console.WriteLine(name + " not found");
                return;
            }

            // check if there are descendants
            if (rg.GetChildNodes(name).Count < 1)
            {
                Console.WriteLine(name + " has no descendants");
                return;
            }

            List<GraphNode> current_generation = new List<GraphNode>();                 //list of nodes in current generation being printed
            List<GraphNode> next_generation = rg.GetChildNodes(name);
            int generation_number = 1;                                                  //count of generations

            // print children and get grandchildren
            Console.WriteLine("*children: ");

            //while there are kids in each next generation, print them and get their kids
            while (next_generation.Count > 0)
            {
                generation_number++;

                current_generation.Clear();
                copy_list(current_generation, next_generation);
                next_generation.Clear();


                if (generation_number > 2)
                {
                    Console.WriteLine();
                    Console.Write("*great ");
                    // print the required number of 'greats'
                    for (int i = 2; i < generation_number; i++)
                    {
                        Console.Write("great ");
                    }
                    Console.WriteLine("grandchildren: ");
                }

                foreach (GraphNode child in current_generation)
                {
                    Console.Write(child.Name + " ");
                    child.Label = "visited";
                    foreach (GraphNode next_kid in rg.GetChildNodes(child.Name))
                    {
                        if (node_visited(next_kid))
                        {
                            Console.WriteLine("Cycle detected!");
                            return;
                        }
                        next_generation.Add(next_kid);
                    }
                }
                Console.WriteLine();
            }

            //reset visited labels to unvisited
            reset_label();
            return;
        }

        // copy function to copy next generation list into current generation
        private static void copy_list(List<GraphNode> current, List<GraphNode> next)
        {
            foreach (GraphNode person in next)
                current.Add(person);
        }

        // check if a node has been visited
        private static bool node_visited(GraphNode node)
        {
            return node.Label == "visited";
        }

        // reset visit labels to unvisited after a descendant search
        private static void reset_label()
        {
            foreach (GraphNode person in rg.nodes)
                person.Label = "Unvisited";
        }

        // accept, parse, and execute user commands
        private static void CommandLoop()
        {
            string command = "";
            string[] commandWords;
            Console.Write("Welcome to Harry's Dutch Bingo Parlor!\n");

            while (command != "exit")
            {
                Console.Write("\nEnter a command: ");
                command = Console.ReadLine();
                commandWords = Regex.Split(command, @"\s+");        // split input into array of words
                command = commandWords[0];

                if (command == "exit")
                    return;                                               // do nothing

                // read a relationship graph from a file
                else if (command == "read" && commandWords.Length > 1)
                    ReadRelationshipGraph(commandWords[1]);

                // show information for one person
                else if (command == "show" && commandWords.Length > 1)
                    ShowPerson(commandWords[1]);

                else if (command == "friends" && commandWords.Length > 1)
                    ShowFriends(commandWords[1]);

                //show list of orphans
                else if (command == "orphans")
                    ShowOrphans();

                //show list of descendants
                else if (command == "descendants" && commandWords.Length > 1)
                    ShowDescendants(commandWords[1]);

                else if (command == "siblings" && commandWords.Length > 1)
                    ShowSiblings(commandWords[1]);

                //find bingo
                else if (command == "bingo" && commandWords.Length > 1)
                    Bingo(commandWords[1], commandWords[2]);
                // dump command prints out the graph
                else if (command == "dump")
                    rg.Dump();

                // illegal command
                else
                    Console.Write("\nLegal commands: read [filename], dump, show [personname],\n  friends [personname], descendants [personname], bingo[personname personname] exit\n");
            }
        }

        static void Main(string[] args)
        {
            CommandLoop();
        }
    }
}