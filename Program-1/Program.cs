/* Program to compute the floor of the lg lg of a number n
 * @author: James Eapen
 * 2019 September 17
 * CS-212
 */

using System;

namespace Program_1
{
    class Program
    {
        /* Main program
         * Takes user input for n and displays the lg lg n
         */
        static void Main(string[] args)
        {
            Console.WriteLine("Enter n: ");
            int n = Convert.ToInt32(Console.ReadLine());
            int floor = get_lg(get_lg(n));
            Console.WriteLine("Floor of lg({0}) = {1}", n, floor);
        }

        /* lg function
         * @params: n, the number whose log is to be computed
         * computes the lg of n
         * @returns: floor of the lg n or the number of times n is divided by 2 until n <=2
         */
        static int get_lg(int n) 
        {
            int i = 0;
            while (n >= 2)
            {
                n = n / 2;
                i++;
            }

            return i;
        }
    }
}
