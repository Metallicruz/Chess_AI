using System;
using System.Collections.Generic;
using System.Text;
using UvsChess;

//
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
			/*
			if (the moveToCheck To and From has a piece of same color) then return false
			if (moveToCheck To results in ChessFlag.Check or ChessFlag.Checkmate for colorOfPlayerMoving) then return false
			*/
			bool isValid = true;	
			switch (Math.abs(boardBeforeMove[moveToCheck.From().X, moveToCheck.From().Y))
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
        /// <returns>Returns true if the move was valid</returns>
        static private bool PawnToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck)
        {
			if(moveToCheck.From.Y == 6 && boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y] > 0){// starting 2nd row and is white
				
			}
			else if(moveToCheck.From.Y == 1 && boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y] < 0){//starting from 7th row
				
			}
			
            throw (new NotImplementedException());
        }

        /// <summary>
        /// Contains movement logic for knights.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool KnightToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck)
        {
			/*
			//offset 2 in one direction 1 in the other
			var offsetX=Math.abs(targetColumn-originColumn);
			offsetY=Math.abs(targetRow-originRow);
			if(offsetX==1 || offsetY==1){
				if(offsetX==2 || offsetY==2){
					return true;
				}
			}
			return false;
			*/
        }
		
        /// <summary>
        /// Contains movement logic for knights.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool BishopToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck)
        {
			/*
			if(targetRow==originRow||targetColumn==originColumn){//diagonal movement means both row/column will change
				return false;
			}
			if(Math.abs(targetRow-originRow)!=Math.abs(targetColumn-originColumn)){
				return false;
			}
			if(targetRow<originRow){//move upward
				if(targetColumn>originColumn){//move right
					var j = originColumn;
					for(var i=originRow-1;i>targetRow;i--){
						++j;
						if(position[gameTurn][i][j]!=undefined){//piece in the way
							return false;
						}
					}
					return true;
				}else{//move left
					var j = originColumn;
					for(var i=originRow-1;i>targetRow;i--){
						--j;
						if(position[gameTurn][i][j]!=undefined){//piece in the way
							return false;
						}
					}
					return true;
				}
			}else{//move downwards
				if(targetColumn>originColumn){//move right
					var j = originColumn;
					for(var i=originRow+1;i<targetRow;i++){
						++j;
						if(position[gameTurn][i][j]!=undefined){//piece in the way
							return false;
						}
					}
					return true;
				}else{//move left
					var j = originColumn;
					for(var i=originRow+1;i<targetRow;i++){
						--j;
						if(position[gameTurn][i][j]!=undefined){//piece in the way
							return false;
						}
					}
					return true;
				}
			}
			*/
        }
		
        /// <summary>
        /// Contains movement logic for knights.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool RookToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck)
        {
			
			/*'
			if(targetRow==originRow){//move sideways
				if(targetColumn>originColumn){//moving right
					for(var i=1; i<Math.abs(targetColumn-originColumn); i++){
						if(position[gameTurn][originRow][originColumn+i]!=undefined){//piece in the way
							return false;
						}
					}
					return true;
				}else{//moving left
					for(var i=1; i<Math.abs(targetColumn-originColumn); i++){
						if(position[gameTurn][originRow][originColumn-i]!=undefined){//piece in the way
							return false;
						}
					}
					return true;
				}
			}else if(targetColumn==originColumn){//move up/down
				if(targetRow>originRow){//moving up
					for(var i=1; i<Math.abs(targetRow-originRow); i++){
						if(position[gameTurn][originRow+i][originColumn]!=undefined){//piece in the way
							return false;
						}
					}
					return true;
				}else{//moving down
					for(var i=1; i<Math.abs(targetRow-originRow); i++){
						if(position[gameTurn][originRow-i][originColumn]!=undefined){//piece in the way
							return false;
						}
					}
					return true;
				}
			}	
			*/
        }

        /// <summary>
        /// Contains movement logic for queens.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool QueenToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck)
        {
			/*
			if(bishopMove() || rookMove()){
				return true;
			}
			*/
        }
		
        /// <summary>
        /// Contains movement logic for kings.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        private bool KingToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck)
        {
			/*
			var offsetX=Math.abs(targetColumn-originColumn);
			offsetY=Math.abs(targetRow-originRow);
			if(offsetX>1 || offsetY>1){
				return false;
			}
			if((gameTurn%2)==0){//blacks turn
				bKingPos = targetRow+originRow;
			}else{
				wKingPos = targetRow+originRow;
			}
			return true;
			*/
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
