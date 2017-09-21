using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

namespace StudentAI
{
    public class StudentAI : IChessAI
    {
        #region IChessAI Members that are implemented by the Student

        /// <summary>
        /// The name of your AI
        /// </summary>
        public string Name
        {
#if DEBUG
            get { return "Group One (Debug)"; }
#else
            get { return "Group One"; }
#endif
        }

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
            ChessMove myNextMove = null;

            while (!IsMyTurnOver())
            {
                if (myNextMove == null)
                {
                    //myNextMove = MoveAPawn(board, myColor);
                    this.Log(myColor.ToString() + " (" + this.Name + ") just moved.");
                    this.Log(string.Empty);

                    // Since I have a move, break out of loop
                    break;
                }
            }

            Profiler.SetDepthReachedDuringThisTurn(2);
            return myNextMove;
        }

        /// <summary>
        /// Validates a move. The framework uses this to validate the opponents move.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            throw (new NotImplementedException());
			/*if(boardBeforeMove[moveToCheck.From()]
			if the moveToCheck has a piece of same color then return false
			if moveToCheck is in check for chesscolor return false
			*/
			bool isValid = true;	
			switch (Math.abs(boardBeforeMove[moveToCheck.From()))
			{
				case ChessPiece.WhitePawn:
					isValid = PawnMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
					break;
				case ChessPiece.WhiteRook:
					isValid = RookMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
					break;
				case ChessPiece.WhiteKnight:
					isValid = KnightMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
					break;
				case ChessPiece.WhiteBishop:
					isValid = BishopMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
					break;
				case ChessPiece.WhiteQueen:
					isValid = QueenMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
					break;
				case ChessPiece.WhiteKing:
					isValid = KingMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
					break;
				case ChessPiece.Empty:
					isValid = false;
					break;
				default:
					throw new Exception("Invalid chess piece");
			}
			return isValid;
        }

        /// <summary>
        /// Contains movement logic for pawns.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool PawnMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// Contains movement logic for knights.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool KnightMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            throw (new NotImplementedException());
        }
		
        /// <summary>
        /// Contains movement logic for knights.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool BishopMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            throw (new NotImplementedException());
        }
		
        /// <summary>
        /// Contains movement logic for knights.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool RookMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            throw (new NotImplementedException());
        }

        /// <summary>
        /// Contains movement logic for queens.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool QueenMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            throw (new NotImplementedException());
        }
		
        /// <summary>
        /// Contains movement logic for kings.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        private bool KingMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            throw (new NotImplementedException());
        }
		
        #endregion
















        #region IChessAI Members that should be implemented as automatic properties and should NEVER be touched by students.
        /// <summary>
        /// This will return false when the framework starts running your AI. When the AI's time has run out,
        /// then this method will return true. Once this method returns true, your AI should return a 
        /// move immediately.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        public AIIsMyTurnOverCallback IsMyTurnOver { get; set; }

        /// <summary>
        /// Call this method to print out debug information. The framework subscribes to this event
        /// and will provide a log window for your debug messages.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AILoggerCallback Log { get; set; }

        /// <summary>
        /// Call this method to catch profiling information. The framework subscribes to this event
        /// and will print out the profiling stats in your log window.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="key"></param>
        public AIProfiler Profiler { get; set; }

        /// <summary>
        /// Call this method to tell the framework what decision print out debug information. The framework subscribes to this event
        /// and will provide a debug window for your decision tree.
        /// 
        /// You should NEVER EVER set this property!
        /// This property should be defined as an Automatic Property.
        /// This property SHOULD NOT CONTAIN ANY CODE!!!
        /// </summary>
        /// <param name="message"></param>
        public AISetDecisionTreeCallback SetDecisionTree { get; set; }
        #endregion
    }
}
