using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Mankalah
{
    /*****************************************************************/
    /*
    /* An intelligent Mankalah player. 
    /*
    /*****************************************************************/
    public class Tux : Player
    {
        int turn_count = 0;
        Position us;

        // Tux inherits from base player class
        public Tux(Position pos, int timeLimit) : base(pos, "Tux", timeLimit) { us = pos; }

        // winning message
        public override string gloat()
        {
            return "Tux wins!!";
        }

        // Choose move in a given time
        public override int chooseMove(Board b)
        {
            turn_count++;
            //first and second moves
            if (turn_count == 1)
            {
                if (us == Position.Top)
                    return 9;
                else if (us == Position.Bottom)
                    return 2;
            }

            else if (turn_count == 2)
            {
                if (us == Position.Top)
                    return 12;
                else if (us == Position.Bottom)
                    return 5;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            int depth_count = 5;
            Result move = new Result(0, 0);
            while (watch.ElapsedMilliseconds < getTimePerMove())
            {
                move = minimax(b, depth_count++, Int32.MinValue, int.MaxValue);
            }
            return move.getBestMove();
        }

        /* Minimax search
         * top player is max and bottom is min
         * returns a resulting move from recursive calculations
         */
        private Result minimax(Board board, int depth, int alpha, int beta)
        {
            if (board.gameOver() || depth == 0)
                return new Result(0, evaluate(board));

            int bestVal = int.MinValue;
            int bestMove = 0;
            if (board.whoseMove() == Position.Top)
            {   
                for (int move = 7; move < 13 && alpha < beta; move++)
                {
                    if (board.legalMove(move))
                    {
                        Board b1 = new Board(board);
                        b1.makeMove(move, false);
                        Result val = minimax(b1, depth - 1, alpha, beta);
                        if (val.getBestScore() > bestVal)
                        {
                            bestVal = val.getBestScore();
                            bestMove = move;
                        }
                        if (bestVal > alpha)
                            alpha = bestVal;
                    }
                }
            }

            else
            {
                bestVal = Int32.MaxValue;
                for (int move = 0; move < 6 && alpha < beta; move++)
                {
                    if (board.legalMove(move))
                    {
                        Board b1 = new Board(board);
                        b1.makeMove(move, false);
                        Result val = minimax(b1, depth - 1, alpha, beta);
                        if (val.getBestScore() < bestVal)
                        {
                            bestVal = val.getBestScore();
                            bestMove = move;
                        }
                        if (bestVal < beta)
                            beta = bestVal;
                    }
                }
            }
            return new Result(bestMove, bestVal);
        }

        // evaluates the board from minimax search for total stones, possible replays and captures
        public override int evaluate(Board b)
        {
            int score = b.stonesAt(13) - b.stonesAt(6);
            int totalStones = 0;    //total stones on the board
            int playAgain = 0;      //total go-agains from ending in own bin
            int captures = 0;       //total number of possible captures
            int targetPit = 0;      //last pit into which stones from current pit will go


            if (b.whoseMove() == Position.Top)
            {
                for (int i = 7; i < 13; i++)
                {
                    totalStones += b.stonesAt(i);
                    if (b.stonesAt(i) == (13 - i))
                        playAgain++;

                    targetPit = i + b.stonesAt(i);

                    if (targetPit < 13 && (b.stonesAt(targetPit) == 0 && b.stonesAt(12 - i) > 0))
                        captures++;
                }
                
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    totalStones -= b.stonesAt(i);
                    if (b.stonesAt(i) == (13 - i))
                        playAgain--;

                    targetPit = i + b.stonesAt(i);

                    if (targetPit < 13 && (b.stonesAt(targetPit) == 0 && b.stonesAt(12 - i) > 0))
                        captures--;
                }
            }
            return score + playAgain + captures + totalStones;
        }


        // gets an image for tux
        public override String getImage() { return "https://en.wikipedia.org/wiki/Tux_(mascot)#/media/File:Tux.png"; }

    }

    /* Result class
     * Minimax search returns a result object containing the best move and predicted score
     */
    class Result
    {
        private int bestMove;
        private int bestScore;

        public Result(int move, int score)
        {
            bestMove = move;
            bestScore = score;
        }

        public int getBestMove() { return bestMove; }
        public int getBestScore() { return bestScore; }

    }
}
