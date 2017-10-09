// Project Prolog
// Author: Roberto De La Cruz 
// Author: Wesley Mangrum 
// CS4470
// Project: Lab 01
// Purpose: Chess AI
// 
// I declare that the following code was written by me or provided 
// by the instructor for this project. I understand that copying source
// code from any other source constitutes cheating, and that I will receive
// a zero on this project if I am found in violation of this policy.
// ---------------------------------------------------------------------------
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
            get { return "Group One (debug)"; }
#else
            get { return "Group One (debug)"; }
#endif
        }
        /// <summary>
        /// The previous move to avoid repeated moves
        /// </summary>
        private int NumberOfPreviousStates { get; set; }
        /// <summary>
        /// The turn counter used for generating random
        /// </summary>
        private int TurnCount { get; set; }
        /// <summary>
        /// The turn counter used for generating random
        /// </summary>
        private int previousMinBoardValue { get; set; }
        /// <summary>
        /// The turn counter used for generating random
        /// </summary>
        private int previousMaxBoardValue { get; set; }
        /// <summary>
        /// The previous move to avoid repeated moves
        /// </summary>
        private List<ChessBoard> PreviousStates{get;set;}

        /// <summary>
        /// Evaluates the chess board and decided which move to make. This is the main method of the AI.
        /// The framework will call this method when it's your turn.
        /// </summary>
        /// <param name="board">Current chess board</param>
        /// <param name="yourColor">Your color</param>
        /// <returns> Returns the best chess move the player has for the given chess board</returns>
        public ChessMove GetNextMove(ChessBoard board, ChessColor myColor)
        {
#if DEBUG
            //For more information about using the profiler, visit http://code.google.com/p/uvschess/wiki/ProfilerHowTo
            // This is how you setup the profiler. This should be done in GetNextMove.
            Profiler.TagNames = Enum.GetNames(typeof(StudentAIProfilerTags));

            // In order for the profiler to calculate the AI's nodes/sec value,
            // I need to tell the profiler which method key's are for MiniMax.
            // In this case our mini and max methods are the same,
            // but usually they are not.
            Profiler.MinisProfilerTag = (int)StudentAIProfilerTags.GetAllMoves;
            Profiler.MaxsProfilerTag = (int)StudentAIProfilerTags.GetAllMoves;

            // This increments the method call count by 1 for GetNextMove in the profiler
            Profiler.IncrementTagCount((int)StudentAIProfilerTags.GetNextMove);
#endif
            ++TurnCount;
            int depth = 1;
            previousMinBoardValue = 0;
            previousMaxBoardValue = 0;
            NumberOfPreviousStates = 6;
            if (PreviousStates == null)
            {
                PreviousStates = new List<ChessBoard>();
            }
            if (PreviousStates.Count < NumberOfPreviousStates)
            {
                PreviousStates.Add(board);
            }
            else
            {
                PreviousStates.RemoveAt(0);
                PreviousStates.Add(board);
            }
            ChessMove myNextMove = null;
            List<ChessMove> allMoves = GetAllMoves(board, myColor);
            List<ChessMove> validMoves = new List<ChessMove>();
            foreach (ChessMove move in allMoves)
            {
                if(!MovesIntoCheck(ref board, move))
                {
                    validMoves.Add(move);
                }
            }
            allMoves = validMoves;
            this.Log($"{IsMyTurnOver()}");
            while (!IsMyTurnOver())
            {
                if (allMoves.Count == 0)
                {
                    // If I couldn't find a valid move easily, 
                    // I'll just create an empty move and flag a stalemate.
                    myNextMove = new ChessMove(null, null);
                    ChessPiece kingColor = ChessPiece.BlackKing;
                    if(myColor == ChessColor.White)
                    {
                        kingColor = ChessPiece.WhiteKing;
                    }
                    if(KingInCheck(ref board, kingColor))
                    {
                        myNextMove.Flag = ChessFlag.Checkmate;
                    }
                    else
                    {
                        myNextMove.Flag = ChessFlag.Stalemate;
                    }
                }
                else
                {
                    //myNextMove = Greedy(ref board, ref allMoves,myColor,Heuristic.PieceCost);
                    ChessMove tempBest = MiniMax(depth, ref board, myColor, allMoves);
                    if (IsMyTurnOver())//search didn't finish so return previous best move
                    {
                        break;
                    }
                    if (myNextMove == null || tempBest.ValueOfMove > myNextMove.ValueOfMove)
                    {
                        myNextMove = tempBest;
                    }
                    SetFlags(ref board, ref myNextMove, myColor);
                    allMoves.Sort((y, x) => x.ValueOfMove.CompareTo(y.ValueOfMove));
                    ++depth;
                    //AddAllPossibleMovesToDecisionTree(allMoves, myNextMove, board.Clone(), myColor,1);
                    /*
                    DecisionTree tree = new DecisionTree(board);
                    // Tell UvsChess about the decision tree object
                    SetDecisionTree(tree);
                    AddAllChildrenToDecisionTree(tree, board, myColor, depth);
                    tree.BestChildMove = myNextMove;
                    */
                }
            }
            Profiler.SetDepthReachedDuringThisTurn(depth-1);
            this.Log(myColor.ToString() + " (" + this.Name + ") just moved.");
            this.Log(myColor.ToString() + " (" + this.Name + ") reached depth of " + (depth-1)+".");
            this.Log("value of move ("+myNextMove.From.X+", " + myNextMove.From.Y+ ") to (" + myNextMove.To.X + ", " + myNextMove.To.Y + "): value = " + myNextMove.ValueOfMove);
            this.Log(string.Empty);
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
            ChessPiece tempPieceTo = boardBeforeMove[moveToCheck.To];// save previous board state piece at to location
            ChessPiece tempPieceFrom = boardBeforeMove[moveToCheck.From];// save previous board state piece at from location
            ChessPiece playerKing = ChessPiece.BlackKing;
            ChessPiece enemyKing = ChessPiece.WhiteKing;
            if (colorOfPlayerMoving == ChessColor.White)
            {
                playerKing = ChessPiece.WhiteKing;
                enemyKing = ChessPiece.BlackKing;
            }
            if (MovesIntoCheck(ref boardBeforeMove, moveToCheck))//if the move leave the player in check it's an invalid move
            {
                this.Log("Invalid move: " + colorOfPlayerMoving + " moved into check");
                return false;
            }

            bool isValid = true;
            switch (boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y])
            {
                case ChessPiece.WhitePawn:
                case ChessPiece.BlackPawn:
                    isValid = PawnToMove(boardBeforeMove, moveToCheck);
                    break;
                case ChessPiece.WhiteRook:
                case ChessPiece.BlackRook:
                    isValid = RookToMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
                    break;
                case ChessPiece.WhiteKnight:
                case ChessPiece.BlackKnight:
                    isValid = KnightToMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
                    break;
                case ChessPiece.WhiteBishop:
                case ChessPiece.BlackBishop:
                    isValid = BishopToMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
                    break;
                case ChessPiece.WhiteQueen:
                case ChessPiece.BlackQueen:
                    // Queens can make Bishop OR Rook moves. Only one has to return true;
                    if (BishopToMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving) ||
                        RookToMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving))
                    {
                        isValid = true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case ChessPiece.BlackKing:
                case ChessPiece.WhiteKing:
                    isValid = KingToMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
                    break;
                case ChessPiece.Empty:
                    return false;
                default:
                    throw new Exception("Invalid chess piece");
            }

            //check the flag state
            List<ChessMove> playerMoves = GetAllMoves(boardBeforeMove, colorOfPlayerMoving);
            boardBeforeMove.MakeMove(moveToCheck);
            List<ChessMove> enemyMoves = GetAllMoves(boardBeforeMove, (ChessColor)Math.Abs((int)colorOfPlayerMoving - 1));
            if (moveToCheck.Flag == ChessFlag.NoFlag)//if nothing is flagged
            {
                bool noMoves = true;
                foreach (ChessMove move in playerMoves)//check every possible move
                {
                    if (MovesIntoCheck(ref boardBeforeMove, move) == false)//if the move doesn't leave them in check
                    {
                        noMoves = false;
                        break;
                    }
                }
                if (noMoves)//player no moves
                {
                    this.Log("Invalid move: " + colorOfPlayerMoving + " has no valid moves. It's stalemate");
                    return false;//its stalemate
                }

                noMoves = true;
                foreach (ChessMove move in enemyMoves)//check every possible move
                {
                    if (MovesIntoCheck(ref boardBeforeMove, move) == false)//if the move doesn't leave them in check
                    {
                        noMoves = false;
                        break;
                    }
                }
                if (noMoves && KingInCheck(ref boardBeforeMove, enemyKing) == true)//make sure king isn't in checkmate
                {
                    this.Log("Invalid move: " + (ChessColor)Math.Abs((int)colorOfPlayerMoving - 1) + " is in checkmate.");
                    return false;//its check
                }
                else if (KingInCheck(ref boardBeforeMove, enemyKing) == true)//make sure king isn't in checkmate
                {
                    this.Log("Invalid move: " + (ChessColor)Math.Abs((int)colorOfPlayerMoving - 1) + " is in check.");
                    return false;//its check
                }
            }
            else if (moveToCheck.Flag == ChessFlag.Check)//if check is flagged
            {
                if (KingInCheck(ref boardBeforeMove, enemyKing) == false)//make sure king is in check
                {
                    this.Log("Invalid check: " + (ChessColor)Math.Abs((int)colorOfPlayerMoving - 1) + " isn't in check.");
                    return false;//its not check
                }
            }
            else if (moveToCheck.Flag == ChessFlag.Stalemate)//if stalemate is flagged
            {
                boardBeforeMove[moveToCheck.To] = tempPieceTo;// reset board state to previous state
                boardBeforeMove[moveToCheck.From] = tempPieceFrom;// reset board state to previous state
                if (KingInCheck(ref boardBeforeMove, playerKing) == true)//make sure players king is in check
                {
                    this.Log("Invalid stalemate: " + colorOfPlayerMoving + " is in check.");
                    return false;//its not a stalemate they are in check
                }
                foreach (ChessMove move in playerMoves)//check every possible move
                {
                    if (MovesIntoCheck(ref boardBeforeMove, move) == false)//if the move doesn't leave them in check
                    {
                        this.Log("Invalid stalemate: " + colorOfPlayerMoving + "can still move their " + boardBeforeMove[move.From].ToString());
                        return false;//its not a stalemate
                    }
                }
                return true;//it really is a stalemate
            }
            else if (moveToCheck.Flag == ChessFlag.Checkmate)//if checkmate is flagged
            {
                if (KingInCheck(ref boardBeforeMove, enemyKing) == false)//make sure enemy king is actually in check
                {
                    this.Log("Invalid checkmate: " + (ChessColor)Math.Abs((int)colorOfPlayerMoving - 1) + " isn't in check.");
                    return false;//its not a checkmate
                }
                foreach (ChessMove move in enemyMoves)//check every possible move to see if they can get out of checkmate
                {
                    if (MovesIntoCheck(ref boardBeforeMove, move) == false)//if the move doesn't leave them in check
                    {
                        this.Log("Invalid checkmate: " + (ChessColor)Math.Abs((int)colorOfPlayerMoving - 1) + "can still move their " + boardBeforeMove[move.From].ToString());
                        return false;//its not a checkmate
                    }
                }
                return true;//it really is a checkmate
            }
            boardBeforeMove[moveToCheck.To] = tempPieceTo;// reset board state to previous state
            boardBeforeMove[moveToCheck.From] = tempPieceFrom;// reset board state to previous state


            if (isValid == false)
            {
                this.Log("Invalid move: piece can't move that way");
            }
            return isValid;
        }

        #endregion

        #region Methods to check if a move is valid

        /// <summary>
        /// Contains movement logic for pawns.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool PawnToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck)
        {
            short originRow = (short)moveToCheck.From.Y;
            short targetRow = (short)moveToCheck.To.Y;
            short originColumn = (short)moveToCheck.From.X;
            short targetColumn = (short)moveToCheck.To.X;
            if(boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y] == ChessPiece.Empty)
            {
                throw new Exception("No piece found.");
            }
            else if (boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y] > ChessPiece.Empty)//white piece
            {
                if (originRow == 6 &&
                    targetRow == 4 &&
                    originColumn == targetColumn &&
                    boardBeforeMove[originColumn, originRow - 1] == ChessPiece.Empty &&
                    boardBeforeMove[originColumn, originRow - 2] == ChessPiece.Empty)
                {// If moving from start position and nothing is blocking it, it can move 2 spaces forward
                    return true;
                }

                if (boardBeforeMove[originColumn, originRow - 1] == ChessPiece.Empty &&
                    targetRow == originRow - 1 &&
                    originColumn == targetColumn)
                {// If moving one space forward and that space is empty
                    return true;
                }

                if (Math.Abs(targetColumn - originColumn) == 1 &&
                    (originRow - targetRow) == 1 &&
                    isEnemy(boardBeforeMove[targetColumn, targetRow], ChessColor.White))
                {// If capturing diagonally, moving forward, and an enemy is on that tile
                    return true;
                }
            }
            else//black piece
            {
                if (originRow == 1 &&
                    targetRow == 3 &&
                    originColumn == targetColumn &&
                    boardBeforeMove[originColumn, originRow + 1] == ChessPiece.Empty &&
                    boardBeforeMove[originColumn, originRow + 2] == ChessPiece.Empty)
                {// If moving from start position and nothing is blocking it, it can move 2 spaces forward
                    return true;
                }

                if (boardBeforeMove[originColumn, originRow + 1] == ChessPiece.Empty &&
                    targetRow == originRow + 1 &&
                    originColumn == targetColumn)
                {// If moving one space forward and that space is empty
                    return true;
                }

                if (Math.Abs(targetColumn - originColumn) == 1 &&
                    (originRow - targetRow) == -1 &&
                    isEnemy(boardBeforeMove[targetColumn, targetRow], ChessColor.Black))
                {// If capturing diagonally, moving forward, and an enemy is on that tile
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Contains movement logic for knights.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool KnightToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            //offset 2 in one direction 1 in the other
            int offsetX=Math.Abs(moveToCheck.To.X - moveToCheck.From.X);
			int offsetY=Math.Abs(moveToCheck.To.Y - moveToCheck.From.Y);
            if (boardBeforeMove[moveToCheck.From] == ChessPiece.Empty)//no piece
            {
                throw new Exception("No piece found.");
            }
			if((offsetX==1 || offsetY==1) && (offsetX == 2 || offsetY == 2))
            {
			    if(boardBeforeMove[moveToCheck.To] == ChessPiece.Empty ||
                    isEnemy(boardBeforeMove[moveToCheck.To.X,moveToCheck.To.Y], colorOfPlayerMoving))
                {
					return true;
				}
			}
			return false;
        }

        /// <summary>
        /// Contains movement logic for bishops
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool BishopToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            short originRow = (short)moveToCheck.From.Y;
            short targetRow = (short)moveToCheck.To.Y;
            short originColumn = (short)moveToCheck.From.X;
            short targetColumn = (short)moveToCheck.To.X;

			if(targetRow==originRow||targetColumn==originColumn){//diagonal movement means both row/column will change
				return false;
			}
			if(Math.Abs(targetRow-originRow)!=Math.Abs(targetColumn-originColumn)){ // Not a diagonal movement
				return false;
			}
            if(!isEnemy(boardBeforeMove[targetColumn, targetRow], colorOfPlayerMoving) && 
                boardBeforeMove[targetColumn, targetRow] != ChessPiece.Empty) // Player captured its own piece
            {
                return false;
            }
			if(targetRow<originRow){//move upward
				if(targetColumn>originColumn){//move right
					var j = originColumn;
					for(var i=originRow-1;i>targetRow;i--){
						++j;
						if(boardBeforeMove[j, i]!= ChessPiece.Empty){//piece in the way
							return false;
						}
					}
					return true;
				}else{//move left
					var j = originColumn;
					for(var i=originRow-1;i>targetRow;i--){
						--j;
						if(boardBeforeMove[j, i] != ChessPiece.Empty)
                        {//piece in the way
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
						if(boardBeforeMove[j, i] != ChessPiece.Empty)
                        {//piece in the way
							return false;
						}
					}
					return true;
				}else{//move left
					var j = originColumn;
					for(var i=originRow+1;i<targetRow;i++){
						--j;
						if(boardBeforeMove[j, i] != ChessPiece.Empty)
                        {//piece in the way
							return false;
						}
					}
					return true;
				}
			}
			
            //throw (new NotImplementedException());
        }

        /// <summary>
        /// Contains movement logic for knights.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        static private bool RookToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            short originRow = (short)moveToCheck.From.Y;
            short targetRow = (short)moveToCheck.To.Y;
            short originColumn = (short)moveToCheck.From.X;
            short targetColumn = (short)moveToCheck.To.X;

            if (!isEnemy(boardBeforeMove[targetColumn, targetRow], colorOfPlayerMoving) && 
                boardBeforeMove[targetColumn, targetRow] != ChessPiece.Empty) // Player captured its own piece
            {
                return false;
            }

            if (targetRow==originRow){//move sideways
				if(targetColumn>originColumn){//moving right
					for(var i=1; i<Math.Abs(targetColumn-originColumn); i++){
						if(boardBeforeMove[originColumn + i, originRow] != ChessPiece.Empty)
                        {//piece in the way
							return false;
						}
					}
					return true;
				}else{//moving left
					for(var i=1; i<Math.Abs(targetColumn-originColumn); i++){
						if(boardBeforeMove[originColumn - i, originRow] != ChessPiece.Empty)
                        {//piece in the way
							return false;
						}
					}
					return true;
				}
			}else if(targetColumn==originColumn){//move up/down
				if(targetRow>originRow){//moving down
					for(var i=1; i<Math.Abs(targetRow-originRow); i++){
						if(boardBeforeMove[originColumn, originRow + i] != ChessPiece.Empty)
                        {//piece in the way
							return false;
						}
					}
					return true;
				}else{//moving up
					for(var i=1; i<Math.Abs(targetRow-originRow); i++){
						if(boardBeforeMove[originColumn, originRow - i] != ChessPiece.Empty)
                        {//piece in the way
							return false;
						}
					}
					return true;
				}
			}
            return false;
        }

        

        /// <summary>
        /// Contains movement logic for kings.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <returns>Returns true if the move was valid</returns>
        private bool KingToMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {
            int offsetX = Math.Abs(moveToCheck.To.X - moveToCheck.From.X);
			int offsetY = Math.Abs(moveToCheck.To.Y - moveToCheck.From.Y);

			if(offsetX>1 || offsetY>1) // Can only move at most 1 in each direction
            {
				return false;
			}
            if(boardBeforeMove[moveToCheck.To] == ChessPiece.Empty 
                || isEnemy(boardBeforeMove[moveToCheck.To], colorOfPlayerMoving))
            {// Destination must be empty or have an enemy piece
                return true;
            }
			return false;
        }
        #endregion

        #region Methods to generate a list of valid moves

        public void AddAllPossibleMovesToDecisionTree(List<ChessMove> allMyMoves, ChessMove myChosenMove,
                                                     ChessBoard currentBoard, ChessColor myColor, int depth)
        {
#if DEBUG
            Profiler.IncrementTagCount((int)StudentAIProfilerTags.AddAllPossibleMovesToDecisionTree);
#endif
            Random random = new Random();

            // Create the decision tree object
            DecisionTree dt = new DecisionTree(currentBoard);

            // Tell UvsChess about the decision tree object
            SetDecisionTree(dt);
            dt.BestChildMove = myChosenMove;

            // Go through all of my moves, add them to the decision tree
            // Then go through each of these moves and generate all of my
            // opponents moves and add those to the decision tree as well.
            for (int ix = 0; ix < allMyMoves.Count; ix++)
            {
                ChessMove myCurMove = allMyMoves[ix];
                if (MovesIntoCheck(ref currentBoard, myCurMove))//make sure move is valid
                {
                    continue;
                }
                ChessBoard boardAfterMyCurMove = currentBoard.Clone();
                boardAfterMyCurMove.MakeMove(myCurMove);

                // Add the new move and board to the decision tree
                dt.AddChild(boardAfterMyCurMove, myCurMove);
                // Descend the decision tree to the last child added so we can 
                // add all of the opponents response moves to our move.
                dt = dt.LastChild;

                // Get all of the opponents response moves to my move
                ChessColor oppColor = (myColor == ChessColor.White ? ChessColor.Black : ChessColor.White);
                List<ChessMove> allOppMoves = GetAllMoves(boardAfterMyCurMove, oppColor);

                // Go through all of my opponent moves and add them to the decision tree
                foreach (ChessMove oppCurMove in allOppMoves)
                {
                    ChessBoard boardAfterOppCurMove = boardAfterMyCurMove.Clone();
                    if (MovesIntoCheck(ref boardAfterOppCurMove, oppCurMove))//make sure move is valid
                    {
                        continue;
                    }
                    boardAfterOppCurMove.MakeMove(oppCurMove);
                    dt.AddChild(boardAfterOppCurMove, oppCurMove);

                    // Setting all of the opponents eventual move values to 0 (see below).
                    dt.LastChild.EventualMoveValue = "0";
                }

                if (allOppMoves.Count > 0)
                {
                    // Tell the decision tree which move we think our opponent will choose.
                    int chosenOppMoveNumber = random.Next(allOppMoves.Count);
                    dt.BestChildMove = allOppMoves[chosenOppMoveNumber];
                }

                // Tell the decision tree what this moves eventual value will be.
                // Since this AI can't evaulate anything, I'm just going to set this
                // value to 0.
                dt.EventualMoveValue = "0";

                // All of the opponents response moves have been added to this childs move, 
                // so return to the parent so we can do the loop again for our next move.
                dt = dt.Parent;
            }
        }

        public void AddAllChildrenToDecisionTree(DecisionTree tree, ChessBoard currentBoard, ChessColor myColor, int depth)
        {
            if (depth <= 0)
            {
                return;
            }
            List<ChessMove> allMyMoves = GetAllMoves(currentBoard, myColor);
            // Go through all of my moves, add them to the decision tree
            foreach(ChessMove move in allMyMoves)
            {
                if (MovesIntoCheck(ref currentBoard, move)){//make sure move is valid
                    continue;
                }
                ChessBoard boardAfterMove = currentBoard.Clone();
                boardAfterMove.MakeMove(move);
                tree.AddChild(boardAfterMove, move);
                DecisionTree child = tree.LastChild;//desend to child node
                ChessColor oppColor = (myColor == ChessColor.White ? ChessColor.Black : ChessColor.White);
                AddAllChildrenToDecisionTree(child, boardAfterMove, oppColor, depth - 1);
            }
        }

        /// <summary>
        /// This method generates all valid moves for myColor based on the currentBoard
        /// </summary>
        /// <param name="currentBoard">This is the current board to generate the moves for.</param>
        /// <param name="myColor">This is the color of the player to generate the moves for.</param>
        /// <returns>List of ChessMoves</returns>
        static List<ChessMove> GetAllMoves(ChessBoard currentBoard, ChessColor myColor)
        {
#if DEBUG
            //Profiler.IncrementTagCount((int)StudentAIProfilerTags.GetAllMoves);
#endif
         
            List<ChessMove> allMoves = new List<ChessMove>();

            // Got through the entire board one tile at a time looking for chess pieces I can move
            for (int Y = 0; Y < ChessBoard.NumberOfRows; Y++)
            {
                for (int X = 0; X < ChessBoard.NumberOfColumns; X++)
                {
                    if (currentBoard[X, Y] == ChessPiece.Empty)
                    {
                        continue;
                    }
                    if (myColor == ChessColor.White)
                    {
                        switch (currentBoard[X, Y])
                        {
                            // This block handles move generations for all WhitePawn pieces.
                            case ChessPiece.WhitePawn:
                                allMoves.AddRange(GetPawnMoves(currentBoard, myColor, X, Y));
                                break;

                            // This block handles move generation for all WhiteKnight pieces
                            case ChessPiece.WhiteKnight:
                                allMoves.AddRange(GetKnightMoves(currentBoard, myColor, X, Y));
                                break;

                            case ChessPiece.WhiteBishop:
                                allMoves.AddRange(GetBishopMoves(currentBoard, myColor, X, Y));
                                break;

                            case ChessPiece.WhiteRook:
                                allMoves.AddRange(GetRookMoves(currentBoard, myColor, X, Y));
                                break;

                            case ChessPiece.WhiteQueen:
                                allMoves.AddRange(GetRookMoves(currentBoard, myColor, X, Y));
                                allMoves.AddRange(GetBishopMoves(currentBoard, myColor, X, Y));
                                break;

                            case ChessPiece.WhiteKing:
                                allMoves.AddRange(GetKingMoves(currentBoard, myColor, X, Y));
                                break;

                        }
                    }
                    else
                    {
                        switch (currentBoard[X, Y])
                        {
                            // This block handles move generations for all BlackPawn pieces.
                            case ChessPiece.BlackPawn:
                                allMoves.AddRange(GetPawnMoves(currentBoard, myColor, X, Y));
                                break;

                            // This block handles move generation for all BlackKnight pieces
                            case ChessPiece.BlackKnight:
                                allMoves.AddRange(GetKnightMoves(currentBoard, myColor, X, Y));
                                break;

                            case ChessPiece.BlackBishop:
                                allMoves.AddRange(GetBishopMoves(currentBoard, myColor, X, Y));
                                break;

                            case ChessPiece.BlackRook:
                                allMoves.AddRange(GetRookMoves(currentBoard, myColor, X, Y));
                                break;

                            case ChessPiece.BlackQueen:
                                allMoves.AddRange(GetRookMoves(currentBoard, myColor, X, Y));
                                allMoves.AddRange(GetBishopMoves(currentBoard, myColor, X, Y));
                                break;

                            case ChessPiece.BlackKing:
                                allMoves.AddRange(GetKingMoves(currentBoard, myColor, X, Y));
                                break;
                        }
                    }
                }
            }

            return allMoves;
        }

        /// <summary>
        /// This method returns a list of all possible moves the King piece can make factoring in check.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="myColor"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>List of legal King moves</returns>
        private static List<ChessMove> GetKingMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
        {
            ChessMove kingMove;
            ChessLocation enemyKingLocation = new ChessLocation(X,Y);
            List<ChessMove> kingMoves = new List<ChessMove>();
            ChessPiece myKing = ChessPiece.BlackKing;
            if (myColor == ChessColor.White)
            {
                myKing = ChessPiece.WhiteKing;
            }
            if (X < 0 || X > 7 || Y < 0 || Y > 7)//if coordinates are invalid for king postion
            {
                // Got through the entire board one tile at a time looking for my king
                for (int y = 0; y < ChessBoard.NumberOfRows; y++)
                {
                    for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
                    {
                        if (currentBoard[x, y] == myKing)
                        {
                            X = x;
                            Y = y;
                            break;
                        }
                    }
                    if (y == Y)
                    {
                        break;
                    }
                }
            }
            // Down
            if (Y < 7)
            {
                if (currentBoard[X, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X, Y + 1], myColor))
                {
                    kingMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y + 1));
                    if (MovesIntoCheck(ref currentBoard,kingMove) == false)
                    {
                        kingMoves.Add(kingMove);
                    }
                }
            }
            // Up
            if (Y > 0)
            {
                if (currentBoard[X, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X, Y - 1], myColor))
                {
                    kingMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y - 1));
                    if (MovesIntoCheck(ref currentBoard, kingMove) == false)
                    {
                        kingMoves.Add(kingMove);
                    }
                }
            }

            // Right
            if (X < 7)
            {
                if (currentBoard[X + 1, Y] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y], myColor))
                {
                    kingMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X+1, Y));
                    if (MovesIntoCheck(ref currentBoard, kingMove) == false)
                    {
                        kingMoves.Add(kingMove);
                    }
                }
            }

            // Left
            if (X > 0)
            {
                if (currentBoard[X - 1, Y] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y], myColor))
                {
                    kingMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X-1, Y));
                    if (MovesIntoCheck(ref currentBoard, kingMove) == false)
                    {
                        kingMoves.Add(kingMove);
                    }
                }
            }

            // DownRight
            if (Y < 7 && X < 7)
            {
                if (currentBoard[X + 1, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y + 1], myColor))
                {
                    kingMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X+1, Y + 1));
                    if (MovesIntoCheck(ref currentBoard, kingMove) == false)
                    {
                        kingMoves.Add(kingMove);
                    }
                }
            }

            // DownLeft
            if (Y < 7 && X > 0)
            {
                if (currentBoard[X - 1, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y + 1], myColor))
                {
                    kingMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X-1, Y + 1));
                    if (MovesIntoCheck(ref currentBoard, kingMove) == false)
                    {
                        kingMoves.Add(kingMove);
                    }
                }
            }

            // UpRight
            if (Y > 0 && X < 7)
            {
                if (currentBoard[X + 1, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y - 1], myColor))
                {
                    kingMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X+1, Y - 1));
                    if (MovesIntoCheck(ref currentBoard, kingMove) == false)
                    {
                        kingMoves.Add(kingMove);
                    }
                }
            }

            // UpLeft
            if (Y > 0 && X > 0)
            {
                if (currentBoard[X - 1, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y - 1], myColor))
                {
                    kingMove = new ChessMove(new ChessLocation(X, Y), new ChessLocation(X-1, Y - 1));
                    if (MovesIntoCheck(ref currentBoard, kingMove) == false)
                    {
                        kingMoves.Add(kingMove);
                    }
                }
            }

            return kingMoves;
        }
/*
        /// <summary>
        /// This method returns a list of all possible moves the King piece make.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="myColor"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>List of legal King moves</returns>
        private List<ChessMove> GetKingMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
        {
            List<ChessMove> kingMoves = new List<ChessMove>();
            ChessPiece enemyKing = ChessPiece.BlackKing;
            ChessLocation enemyKingLocation = new ChessLocation(X, Y);
            if (myColor == ChessColor.White)
            {
                enemyKing = ChessPiece.BlackKing;
            }
            // Got through the entire board one tile at a time looking for enemy king
            for (int y = 0; y < ChessBoard.NumberOfRows; y++)
            {
                for (int x = 0; x < ChessBoard.NumberOfColumns; x++)
                {
                    if(currentBoard[x,y]== enemyKing)//set location for enemy king
                    {
                        enemyKingLocation.X = x;
                        enemyKingLocation.Y = y;
                    }
                }
            }

            // Down
            if (Y < 7 && !(enemyKingLocation.Y == Y+2 && (enemyKingLocation.X == X || enemyKingLocation.X == X+1 || enemyKingLocation.X == X-1)))//make sure king is 
            {
                if (currentBoard[X, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X, Y + 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y + 1)));
                }
            }

            // Up
            if (Y > 0 && !(enemyKingLocation.Y == Y - 2 && (enemyKingLocation.X == X || enemyKingLocation.X == X - 1 || enemyKingLocation.X == X + 1)))
            {
                if (currentBoard[X, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X, Y - 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y - 1)));
                }
            }

            // Right
            if (X < 7 && !(enemyKingLocation.X == X + 2 && (enemyKingLocation.Y == Y || enemyKingLocation.Y == Y - 1 || enemyKingLocation.Y == Y + 1)))
            {
                if (currentBoard[X + 1, Y] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y)));
                }
            }

            // Left
            if (X > 0 && !(enemyKingLocation.X == X - 2 && (enemyKingLocation.Y == Y || enemyKingLocation.Y == Y - 1 || enemyKingLocation.Y == Y + 1)))
            {
                if (currentBoard[X - 1, Y] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y)));
                }
            }

            // DownRight
            if (Y < 7 && X < 7
                && !(enemyKingLocation.Y == Y + 2 && (enemyKingLocation.X == X || enemyKingLocation.X == X + 1 || enemyKingLocation.X == X +2))//check the three squares below location to move to
                && !(enemyKingLocation.X == X + 2 && (enemyKingLocation.Y == Y || enemyKingLocation.Y == Y + 1)))//check two square to the right of location to move to (third square was checked just above)
            {
                if (currentBoard[X + 1, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y + 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y + 1)));
                }
            }

            // DownLeft
            if (Y < 7 && X > 0
                && !(enemyKingLocation.Y == Y + 2 && (enemyKingLocation.X == X || enemyKingLocation.X == X - 1 || enemyKingLocation.X == X - 2))//check the three squares below location to move to
                && !(enemyKingLocation.X == X - 2 && (enemyKingLocation.Y == Y || enemyKingLocation.Y == Y + 1)))//check two square to the left of location to move to (third square was checked just above)
            {
                if (currentBoard[X - 1, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y + 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y + 1)));
                }
            }

            // UpRight
            if (Y > 0 && X < 7
                 && !(enemyKingLocation.Y == Y - 2 && (enemyKingLocation.X == X || enemyKingLocation.X == X + 1 || enemyKingLocation.X == X + 2))//check the three squares above location to move to
                && !(enemyKingLocation.X == X + 2 && (enemyKingLocation.Y == Y || enemyKingLocation.Y == Y - 1)))//check two square to the right of location to move to (third square was checked just above)
            {
                if (currentBoard[X + 1, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y - 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y - 1)));
                }
            }

            // UpLeft
            if (Y > 0 && X > 0
                 && !(enemyKingLocation.Y == Y - 2 && (enemyKingLocation.X == X || enemyKingLocation.X == X - 1 || enemyKingLocation.X == X - 2))//check the three squares above location to move to
                && !(enemyKingLocation.X == X - 2 && (enemyKingLocation.Y == Y || enemyKingLocation.Y == Y - 1)))//check two square to the right of location to move to (third square was checked just above)
            {
                if (currentBoard[X - 1, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y - 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y - 1)));
                }
            }

            return kingMoves;
        }
        */

        /// <summary>
        /// This method returns a list of all possible moves the Rook piece make.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="myColor"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>List of legal Rook moves</returns>
        private static List<ChessMove> GetRookMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
        {
            List<ChessMove> rookMoves = new List<ChessMove>();
            int curX = X;
            int curY = Y;

            // Down
            for (int i = 1; curY >= 0 && curY < 7; i++)
            {
                curY++;
                if (currentBoard[curX, curY] == ChessPiece.Empty)
                {
                    rookMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                }
                else if (isEnemy(currentBoard[curX, curY], myColor))
                {
                    rookMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                    break; // Can move to enemy tile, but no further
                }
                else
                {
                    break; // Friendly piece is blocking that direction.
                }
            }

            curY = Y;

            // Up
            for (int i = 1; curY > 0 && curY <= 7; i++)
            {
                curY--;
                if (currentBoard[curX, curY] == ChessPiece.Empty)
                {
                    rookMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                }
                else if (isEnemy(currentBoard[curX, curY], myColor))
                {
                    rookMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                    break; // Can move to enemy tile, but no further
                }
                else
                {
                    break; // Friendly piece is blocking that direction.
                }
            }

            curY = Y;

            // Right
            for (int i = 1; curX >= 0 && curX < 7; i++)
            {
                curX++;
                if (currentBoard[curX, curY] == ChessPiece.Empty)
                {
                    rookMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                }
                else if (isEnemy(currentBoard[curX, curY], myColor))
                {
                    rookMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                    break; // Can move to enemy tile, but no further
                }
                else
                {
                    break; // Friendly piece is blocking that direction.
                }
            }

            curX = X;


            // Left
            for (int i = 1; curX > 0 && curX <= 7; i++)
            {
                curX--;
                if (currentBoard[curX, curY] == ChessPiece.Empty)
                {
                    rookMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                }
                else if (isEnemy(currentBoard[curX, curY], myColor))
                {
                    rookMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                    break; // Can move to enemy tile, but no further
                }
                else
                {
                    break; // Friendly piece is blocking that direction.
                }
            }

            return rookMoves;

        }

        /// <summary>
        /// This method returns a list of all possible moves the Bishop piece make.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="myColor"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>List of legal Bishop moves</returns>
        private static List<ChessMove> GetBishopMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
        {
            List<ChessMove> bishopMoves = new List<ChessMove>();
            int curX = X;
            int curY = Y;

            // DownRight
            for (int i = 1; curY >= 0 && curX >= 0 && curY < 7 && curX < 7; i++)
            {
                curX++;
                curY++;
                if (currentBoard[curX, curY] == ChessPiece.Empty)
                {
                    bishopMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                }
                else if (isEnemy(currentBoard[curX, curY], myColor))
                {
                    bishopMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                    break; // Can move to enemy tile, but no further
                }
                else
                {
                    break; // Friendly piece is blocking that direction.
                }
            }

            curX = X;
            curY = Y;

            // DownLeft
            for (int i = 1; curY >= 0 && curX > 0 && curY < 7 && curX <= 7; i++)
            {
                curX--;
                curY++;
                if (currentBoard[curX, curY] == ChessPiece.Empty)
                {
                    bishopMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                }
                else if (isEnemy(currentBoard[curX, curY], myColor))
                {
                    bishopMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                    break; // Can move to enemy tile, but no further
                }
                else
                {
                    break; // Friendly piece is blocking that direction.
                }
            }

            curX = X;
            curY = Y;

            // UpRight
            for (int i = 1; curY > 0 && curX >= 0 && curY <= 7 && curX < 7; i++)
            {
                curX++;
                curY--;
                if (currentBoard[curX, curY] == ChessPiece.Empty)
                {
                    bishopMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                }
                else if (isEnemy(currentBoard[curX, curY], myColor))
                {
                    bishopMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                    break; // Can move to enemy tile, but no further
                }
                else
                {
                    break; // Friendly piece is blocking that direction.
                }
            }

            curX = X;
            curY = Y;

            // UpLeft
            for (int i = 1; curY > 0 && curX > 0 && curY <= 7 && curX <= 7; i++)
            {
                curX--;
                curY--;
                if (currentBoard[curX, curY] == ChessPiece.Empty)
                {
                    bishopMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                }
                else if (isEnemy(currentBoard[curX, curY], myColor))
                {
                    bishopMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(curX, curY)));
                    break; // Can move to enemy tile, but no further
                }
                else
                {
                    break; // Friendly piece is blocking that direction.
                }
            }

            return bishopMoves;
        }

        /// <summary>
        /// This method returns a list of all possible moves the Knight piece make.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="myColor"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>List of legal Knight moves</returns>
        private static List<ChessMove> GetKnightMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
        {
            List<ChessMove> knightMoves = new List<ChessMove>();
            // UpUpLeft 
            if (X > 0 && Y > 1)
            {
                if (currentBoard[X - 1, Y - 2] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y - 2], myColor))
                {
                    // Generates a move for a Knigt to move UpUpLeft to an empty tile or one with an enemy chess piece
                    knightMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y - 2)));
                }
            }
            // UpLeftLeft
            if (X > 1 && Y > 0)
            {
                if (currentBoard[X - 2, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X - 2, Y - 1], myColor))
                {
                    knightMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 2, Y - 1)));
                }
            }
            // UpUpRight
            if (X < 7 && Y > 1)
            {
                if (currentBoard[X + 1, Y - 2] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y - 2], myColor))
                {
                    knightMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y - 2)));
                }
            }
            // UpRightRight
            if (X < 6 && Y > 0)
            {
                if (currentBoard[X + 2, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X + 2, Y - 1], myColor))
                {
                    knightMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 2, Y - 1)));
                }
            }
            // DownDownLeft
            if (X > 0 && Y < 6)
            {
                if (currentBoard[X - 1, Y + 2] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y + 2], myColor))
                {
                    knightMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y + 2)));
                }
            }
            // DownLeftLeft
            if (X > 1 && Y < 7)
            {
                if (currentBoard[X - 2, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X - 2, Y + 1], myColor))
                {
                    knightMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 2, Y + 1)));
                }
            }
            // DownDownRight
            if (X < 7 && Y < 6)
            {
                if (currentBoard[X + 1, Y + 2] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y + 2], myColor))
                {
                    // Generates a move for a Knigt to move UpUpLeft to an empty tile or one with an enemy chess piece
                    knightMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y + 2)));
                }
            }
            // DownRightRight
            if (X < 6 && Y < 7)
            {
                if (currentBoard[X + 2, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X + 2, Y + 1], myColor))
                {
                    knightMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 2, Y + 1)));
                }
            }

            return knightMoves;
        }

        /// <summary>
        /// This method returns a list of all possible moves the Pawn piece make.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="myColor"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>List of legal Pawn moves</returns>
        private static List<ChessMove> GetPawnMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
        {
            List<ChessMove> pawnMoves = new List<ChessMove>();
            switch (myColor)
            {
                // White Pawn
                case ChessColor.White:
                    if (currentBoard[X, Y - 1] == ChessPiece.Empty)
                    {
                        // Generate a move to move my pawn 1 tile forward
                        pawnMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y - 1)));

                    }

                    if (Y == 6 && currentBoard[X, Y - 2] == ChessPiece.Empty && currentBoard[X, Y - 1] == ChessPiece.Empty)
                    {
                        // Generates a move for a pawn 2 tiles forward if at start location
                        pawnMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y - 2)));
                    }

                    if (X > 0 && Y > 0)
                    {
                        if (isEnemy(currentBoard[X - 1, Y - 1], myColor))
                        {
                            // Generates a move for a pawn to move diagonally to a tile with an enemy chess piece
                            pawnMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y - 1)));
                        }
                    }

                    if (X < 7 && Y > 0)
                    {
                        if (isEnemy(currentBoard[X + 1, Y - 1], myColor))
                        {
                            // Generates a move for a pawn to move diagonally to a tile with an enemy chess piece
                            pawnMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y - 1)));
                        }
                    }
                    break;

                // Black Pawn
                case ChessColor.Black:
                    if (currentBoard[X, Y + 1] == ChessPiece.Empty)
                    {
                        // Generate a move to move my pawn 1 tile forward
                        pawnMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y + 1)));
                    }

                    if (Y == 1 && currentBoard[X, Y + 2] == ChessPiece.Empty && currentBoard[X, Y + 1] == ChessPiece.Empty)
                    {
                        // Generates a move for a pawn 2 tiles forward if at start location
                        pawnMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y + 2)));
                    }

                    if (X < 7 && Y < 7)
                    {
                        if (isEnemy(currentBoard[X + 1, Y + 1], myColor))
                        {
                            // Generates a move for a pawn to move diagonally to a tile with an enemy chess piece
                            pawnMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y + 1)));
                        }
                    }

                    if (X > 0 && Y < 7)
                    {
                        if (isEnemy(currentBoard[X - 1, Y + 1], myColor))
                        {
                            // Generates a move for a pawn to move diagonally to a tile with an enemy chess piece
                            pawnMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y + 1)));
                        }
                    }
                    break;

                default:
                    break;
            }
            return pawnMoves;
        }

        /// <summary>
        /// This method determines whether a certain chessPiece is considered an enemy piece or not.
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>True or False depending on if the chessPiece is an enemy piece</returns>
        private static bool isEnemy(ChessPiece chessPiece, ChessColor myColor)
        {
            if (myColor == ChessColor.White && chessPiece < ChessPiece.Empty)//black is less than than empty
            {
                return true;
            }
            if (myColor == ChessColor.Black && chessPiece > ChessPiece.Empty)//white is greater than empty
            {
                return true;
            }
            return false; //if pieces are same color
        }

        #endregion

        #region Methods that test for check on kings and set flags
        /// <summary>
        /// Checks current move and sets flags if needed
        /// </summary>
        /// <param name="move"></param>
        private static void SetFlags(ref ChessBoard board, ref ChessMove move, ChessColor myColor)
        {
            if (move == null || move.To == null || move.From == null || MovesIntoCheck(ref board, move))//if we have to move into check thats stalemate
            {
                move = new ChessMove(null, null);
                move.Flag = ChessFlag.Stalemate;
                return;
            }
            ChessPiece enemyKing = ChessPiece.WhiteKing;
            if (myColor == ChessColor.White)
            {
                enemyKing = ChessPiece.BlackKing;
            }
            ChessBoard boardAfterMove =  board.Clone();
            move.Flag = ChessFlag.NoFlag;
            boardAfterMove.MakeMove(move);
            if (KingInCheck(ref boardAfterMove, enemyKing))//if enemy king is in check after move
            {
                move.Flag = ChessFlag.Check;//set check flag

                //see whether check is actually mate
                ChessColor oppColor = (myColor == ChessColor.White ? ChessColor.Black : ChessColor.White);
                List<ChessMove> oppMoves = GetAllMoves(boardAfterMove, oppColor);
                bool noMoves = true;
                foreach (ChessMove oppMove in oppMoves)//check every possible move
                {
                    if (MovesIntoCheck(ref boardAfterMove, oppMove) == false)//if the move doesn't leave them in check
                    {
                        noMoves = false;
                        break;
                    }
                }
                if (noMoves)//opponent has no way out of check
                {
                    move.Flag = ChessFlag.Checkmate;//so set mate
                }
            }
        }

        /// <summary>
        /// This method determines whether a move causes their king to be in check
        /// </summary>
        /// <param name="board"></param>
        /// <param name="move"></param>
        /// <returns>returns true if the move leaves players king in check</returns>
        private static bool MovesIntoCheck(ref ChessBoard board, ChessMove move)
        {
            ChessPiece tempPieceTo = board[move.To];// save previous board state piece at to location
            ChessPiece tempPieceFrom = board[move.From];// save previous board state piece at from location
            ChessPiece myKing = ChessPiece.BlackKing;
            if(board[move.From] > ChessPiece.Empty)//white piece
            {
                myKing = ChessPiece.WhiteKing;
            }
            bool inCheck = false;
            board.MakeMove(move);
            inCheck = KingInCheck(ref board, myKing);
            board[move.To] = tempPieceTo;// reset board state to previous state
            board[move.From] = tempPieceFrom;// reset board state to previous state
            return inCheck;
        }

        /// <summary>
        /// This method determines whether any king is in check
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>returns the king that is in check or empty if neither are in check</returns>
        private static ChessPiece KingInCheck(ref ChessBoard board)
        {
            short kingsChecked = 0;
            // Go through the entire board one tile until both kings are found
            for (int Y = 0; Y < ChessBoard.NumberOfRows; Y++)
            {
                for (int X = 0; X < ChessBoard.NumberOfColumns; X++)
                {
                    if (kingsChecked >= 2)//there are always only 2 kings so return if both have been checked
                    {
                        return ChessPiece.Empty;
                    }
                    if (board[X, Y] == ChessPiece.WhiteKing || board[X, Y] == ChessPiece.BlackKing)//a king is found
                    {
                        if(CheckDiagonal(ref board, new ChessLocation(X, Y)))//test its diagonals for check
                        {
                            return board[X,Y];
                        }
                        if(CheckHorizontalAndVertical(ref board, new ChessLocation(X, Y)))//test the horizontals and verticals for check
                        {
                            return board[X,Y];
                        }
                        if(CheckFromKnight(ref board, new ChessLocation(X, Y)))//test for tricky knight checks
                        {
                            return board[X,Y];
                        }
                        ++kingsChecked;
                    }
                }
            }
            return ChessPiece.Empty;
        } 
        
        /// <summary>
        /// This method determines whether a specific king is in check
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>returns the king that is in check or empty if neither are in check</returns>
        private static bool KingInCheck(ref ChessBoard board, ChessPiece kingToCheck)
        {
            // Go through the entire board one tile until specific king is found
            for (int Y = 0; Y < ChessBoard.NumberOfRows; Y++)
            {
                for (int X = 0; X < ChessBoard.NumberOfColumns; X++)
                {
                    if (board[X, Y] == kingToCheck)//king is found
                    {
                        if (CheckDiagonal(ref board, new ChessLocation(X, Y)))//test its diagonals for check
                        {
                            return true;
                        }
                        if (CheckHorizontalAndVertical(ref board, new ChessLocation(X, Y)))//test the horizontals and verticals for check
                        {
                            return true;
                        }
                        if (CheckFromKnight(ref board, new ChessLocation(X, Y)))//test for tricky knight checks
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }
            return false;
        }

        /*
        /// <summary>
        /// This method determines whether a king is in check more efficiently by factoring in only a single moves impact on board
        /// </summary>
        /// <param name="board"></param>
        /// <param name="move"></param>
        /// <returns>returns the king that is in check or empty if neither are in check</returns>
        private static ChessPiece KingInCheck(ref ChessBoard board, ref ChessMove move)
        {
            if (true)
            {
                return ChessPiece.WhiteKing;
            }
            else if (true)
            {
                return ChessPiece.BlackKing;
            }
            return ChessPiece.Empty;
        }*/

        /// <summary>
        /// This method tests the diagonals of a king to see if there is check from a pawn, bishop, or queen
        /// </summary>
        /// <param name="board"></param>
        /// <param name="kingPosition"></param>
        /// <returns>true if the king is in check or false if he is not</returns>
        private static bool CheckDiagonal(ref ChessBoard board, ChessLocation kingPosition)
        {
            ChessColor colorOfKing;
            ChessPiece pieceToCheck;
            bool diagonalUpSafe = false;
            bool diagonalDownSafe = false;
            if (board[kingPosition] == ChessPiece.Empty)
            {
                throw new Exception("No piece found.");
            }
            else if (board[kingPosition] > ChessPiece.Empty)//white king
            {
                colorOfKing = ChessColor.White;
                if (kingPosition.X - 1 >= 0//if upleft from white king is in board boundaries
                && kingPosition.Y - 1 >= 0
                && board[kingPosition.X - 1, kingPosition.Y - 1] == ChessPiece.BlackPawn)//if it's an enemy black pawn
                {
                    return true;//check
                }
                if (kingPosition.X + 1 <= 7//if upright from white king is in board boundaries
                && kingPosition.Y - 1 >= 0
                && board[kingPosition.X + 1, kingPosition.Y - 1] == ChessPiece.BlackPawn)//if it's an enemy black pawn
                {
                    return true;//check
                }
            }
            else//black king
            {
                colorOfKing = ChessColor.Black;
                if (kingPosition.X - 1 >= 0//if downleft from black king is in board boundaries
                && kingPosition.Y + 1 <= 7
                && board[kingPosition.X - 1, kingPosition.Y + 1] == ChessPiece.WhitePawn)//if it's an enemy white pawn
                {
                    return true;//check
                }
                if (kingPosition.X + 1 <= 7//if downright from black king is in board boundaries
                && kingPosition.Y + 1 <= 7
                && board[kingPosition.X + 1, kingPosition.Y + 1] == ChessPiece.WhitePawn)//if it's an enemy white pawn
                {
                    return true;//check
                }
            }

            for(int i = 1; i <= kingPosition.X; ++i)//check two diagonals left of king
            {
                if(diagonalUpSafe && diagonalDownSafe)//if both diagonals are being blocked no need to check more squares
                {
                    break;
                }
                if(diagonalUpSafe == false && (kingPosition.Y-i) >= 0)//Y within top of board and more squares on this diagonal need to checked
                {
                    pieceToCheck = board[kingPosition.X - i, kingPosition.Y - i];//up and to the left
                    if (pieceToCheck == ChessPiece.Empty) { }//don't perform any checks if there is no piece
                    else if (isEnemy(pieceToCheck, colorOfKing))//encounter enemy piece
                    {
                        if(pieceToCheck == ChessPiece.BlackBishop //if the enemy piece is a bishop there is check
                            || pieceToCheck == ChessPiece.WhiteBishop
                            || pieceToCheck == ChessPiece.WhiteQueen//if the enemy piece is a queen there is check
                            || pieceToCheck == ChessPiece.BlackQueen)
                        {
                            return true;
                        }
                        if (i == 1 && (pieceToCheck == ChessPiece.WhiteKing || pieceToCheck == ChessPiece.BlackKing))//can't move next to enemy king
                        {
                            return true;
                        }
                        diagonalUpSafe = true;//enemy piece can't capture on diagonal and is blocking it
                    }
                    else//friendly piece blocking diagonal means it's safe
                    {
                        diagonalUpSafe = true;
                    }
                }
                if (diagonalDownSafe == false && (kingPosition.Y + i) <= 7)//Y within bottom of board and more squares on this diagonal need to checked
                {
                    pieceToCheck = board[kingPosition.X - i, kingPosition.Y + i];//down and to the left
                    if (pieceToCheck == ChessPiece.Empty)
                    {
                        continue;
                    }
                    else if (isEnemy(pieceToCheck, colorOfKing))//encounter enemy piece
                    {
                        if (pieceToCheck == ChessPiece.BlackBishop //if the enemy piece is a bishop there is check
                            || pieceToCheck == ChessPiece.WhiteBishop
                            || pieceToCheck == ChessPiece.WhiteQueen//if the enemy piece is a queen there is check
                            || pieceToCheck == ChessPiece.BlackQueen)
                        {
                            return true;
                        }
                        if (i == 1 && (pieceToCheck == ChessPiece.WhiteKing || pieceToCheck == ChessPiece.BlackKing))//can't move next to enemy king
                        {
                            return true;
                        }
                        diagonalDownSafe = true;//enemy piece can't capture on diagonal and is blocking it
                    }
                    else//friendly piece blocking diagonal means it's safe
                    {
                        diagonalDownSafe = true;
                    }
                }
            }

            diagonalUpSafe = false;
            diagonalDownSafe = false;
            for (int i = 1; i <= (7-kingPosition.X); ++i)//check two diagonals right of king
            {
                if (diagonalUpSafe && diagonalDownSafe)//if both diagonals are being blocked no need to check more squares
                {
                    break;
                }
                if (diagonalUpSafe == false && (kingPosition.Y - i) >= 0)//Y within top of board and more squares on this diagonal need to checked
                {
                    pieceToCheck = board[kingPosition.X + i, kingPosition.Y - i];//up and to the right
                    if (pieceToCheck == ChessPiece.Empty) { }//don't perform any checks if there is no piece
                    else if (isEnemy(pieceToCheck, colorOfKing))//encounter enemy piece
                    {
                        if (pieceToCheck == ChessPiece.BlackBishop //if the enemy piece is a bishop there is check
                            || pieceToCheck == ChessPiece.WhiteBishop
                            || pieceToCheck == ChessPiece.WhiteQueen//if the enemy piece is a queen there is check
                            || pieceToCheck == ChessPiece.BlackQueen)
                        {
                            return true;
                        }
                        if (i == 1 && (pieceToCheck == ChessPiece.WhiteKing || pieceToCheck == ChessPiece.BlackKing))//can't move next to enemy king
                        {
                            return true;
                        }
                        diagonalUpSafe = true;//enemy piece can't capture on diagonal and is blocking it
                    }
                    else//friendly piece blocking diagonal means it's safe
                    {
                        diagonalUpSafe = true;
                    }
                }
                if (diagonalDownSafe == false && (kingPosition.Y + i) <= 7)//Y within bottom of board and more squares on this diagonal need to checked
                {
                    pieceToCheck = board[kingPosition.X + i, kingPosition.Y + i];//down and to the right
                    if (pieceToCheck == ChessPiece.Empty)//no checks needed if there is no piece
                    {
                        continue;
                    }
                    else if (isEnemy(pieceToCheck, colorOfKing))//encounter enemy piece
                    {
                        if (pieceToCheck == ChessPiece.BlackBishop //if the enemy piece is a bishop there is check
                            || pieceToCheck == ChessPiece.WhiteBishop
                            || pieceToCheck == ChessPiece.WhiteQueen//if the enemy piece is a queen there is check
                            || pieceToCheck == ChessPiece.BlackQueen)
                        {
                            return true;
                        }
                        if (i == 1 && (pieceToCheck == ChessPiece.WhiteKing || pieceToCheck == ChessPiece.BlackKing))//can't move next to enemy king
                        {
                            return true;
                        }
                        diagonalDownSafe = true;//enemy piece can't capture on diagonal and is blocking it
                    }
                    else//friendly piece blocking diagonal means it's safe
                    {
                        diagonalDownSafe = true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method tests the horiztonal and vertical lines of a king to see if there is check from a rook or queen
        /// </summary>
        /// <param name="board"></param>
        /// <param name="kingPosition"></param>
        /// <returns>true if the king is in check or false if he is not</returns>
        private static bool CheckHorizontalAndVertical(ref ChessBoard board, ChessLocation kingPosition)
        {
            ChessColor colorOfKing;
            ChessPiece pieceToCheck;
            if (board[kingPosition] == ChessPiece.Empty)
            {
                throw new Exception("No piece found.");
            }
            else if (board[kingPosition] > ChessPiece.Empty)//white king
            {
                colorOfKing = ChessColor.White;
            }
            else//black king
            {
                colorOfKing = ChessColor.Black;
            }

            for (int i = 1; i <= kingPosition.X; ++i)//check left of king
            {
                pieceToCheck = board[kingPosition.X - i, kingPosition.Y];//to the left i squares
                if (pieceToCheck == ChessPiece.Empty)//don't perform any checks if there is no piece
                {
                    continue;
                }
                else if (isEnemy(pieceToCheck, colorOfKing))//encounter enemy piece
                {
                    if (pieceToCheck == ChessPiece.BlackRook //if the enemy piece is a bishop there is check
                        || pieceToCheck == ChessPiece.WhiteRook
                        || pieceToCheck == ChessPiece.WhiteQueen//if the enemy piece is a queen there is check
                        || pieceToCheck == ChessPiece.BlackQueen)
                    {
                        return true;
                    }
                    if (i == 1 && (pieceToCheck == ChessPiece.WhiteKing || pieceToCheck == ChessPiece.BlackKing))//can't move next to enemy king
                    {
                        return true;
                    }
                    break;//enemy piece blocking rest of squares in that direction
                }
                else//friendly piece blocking left means the rest of squares in that direction are safe
                {
                    break;
                }
            }
            for (int i = 1; i <= (7-kingPosition.X); ++i)//check right of king
            {
                pieceToCheck = board[kingPosition.X + i, kingPosition.Y];//to the right i squares
                if (pieceToCheck == ChessPiece.Empty)//don't perform any checks if there is no piece
                {
                    continue;
                }
                else if (isEnemy(pieceToCheck, colorOfKing))//encounter enemy piece
                {
                    if (pieceToCheck == ChessPiece.BlackRook //if the enemy piece is a bishop there is check
                        || pieceToCheck == ChessPiece.WhiteRook
                        || pieceToCheck == ChessPiece.WhiteQueen//if the enemy piece is a queen there is check
                        || pieceToCheck == ChessPiece.BlackQueen)
                    {
                        return true;
                    }
                    if (i == 1 && (pieceToCheck == ChessPiece.WhiteKing || pieceToCheck == ChessPiece.BlackKing))//can't move next to enemy king
                    {
                        return true;
                    }
                    break;//enemy piece blocking rest of squares in that direction
                }
                else//friendly piece blocking left means the rest of squares in that direction are safe
                {
                    break;
                }
            }
            for (int i = 1; i <= kingPosition.Y; ++i)//check up from the king
            {
                pieceToCheck = board[kingPosition.X, kingPosition.Y-i];//up i squares
                if (pieceToCheck == ChessPiece.Empty)//don't perform any checks if there is no piece
                {
                    continue;
                }
                else if (isEnemy(pieceToCheck, colorOfKing))//encounter enemy piece
                {
                    if (pieceToCheck == ChessPiece.BlackRook //if the enemy piece is a bishop there is check
                        || pieceToCheck == ChessPiece.WhiteRook
                        || pieceToCheck == ChessPiece.WhiteQueen//if the enemy piece is a queen there is check
                        || pieceToCheck == ChessPiece.BlackQueen)
                    {
                        return true;
                    }
                    if (i == 1 && (pieceToCheck == ChessPiece.WhiteKing || pieceToCheck == ChessPiece.BlackKing))//can't move next to enemy king
                    {
                        return true;
                    }
                    break;//enemy piece blocking rest of squares in that direction
                }
                else//friendly piece blocking left means the rest of squares in that direction are safe
                {
                    break;
                }
            }
            for (int i = 1; i <= (7-kingPosition.Y); ++i)//check down from the king
            {
                pieceToCheck = board[kingPosition.X, kingPosition.Y + i];//down i squares
                if (pieceToCheck == ChessPiece.Empty)//don't perform any checks if there is no piece
                {
                    continue;
                }
                else if (isEnemy(pieceToCheck, colorOfKing))//encounter enemy piece
                {
                    if (pieceToCheck == ChessPiece.BlackRook //if the enemy piece is a bishop there is check
                        || pieceToCheck == ChessPiece.WhiteRook
                        || pieceToCheck == ChessPiece.WhiteQueen//if the enemy piece is a queen there is check
                        || pieceToCheck == ChessPiece.BlackQueen)
                    {
                        return true;
                    }
                    if (i == 1 && (pieceToCheck == ChessPiece.WhiteKing || pieceToCheck == ChessPiece.BlackKing))//can't move next to enemy king
                    {
                        return true;
                    }
                    break;//enemy piece blocking rest of squares in that direction
                }
                else//friendly piece blocking left means the rest of squares in that direction are safe
                {
                    break;
                }
            }
            return false;
        }

        /// <summary>
        /// This method test for tricky knights giving check
        /// </summary>
        /// <param name="board"></param>
        /// <param name="kingPosition"></param>
        /// <returns>true if the king is in check or false if he is not</returns>
        private static bool CheckFromKnight(ref ChessBoard board, ChessLocation kingPosition)
        {
            int offsetX = 1;
            int offsetY = 2;
            ChessLocation locationToTest;
            if (board[kingPosition] == ChessPiece.Empty)
            {
                throw new Exception("No piece found.");
            }
            else if (board[kingPosition] > ChessPiece.Empty)//white king
            {
                locationToTest = new ChessLocation(kingPosition.X + offsetX, kingPosition.Y + offsetY);
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.BlackKnight)){ return true; }//check offset +1,+2
                locationToTest.X = kingPosition.X + offsetX;
                locationToTest.Y = kingPosition.Y - offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.BlackKnight)) { return true; }//check offset +1,-2
                locationToTest.X = kingPosition.X - offsetX;
                locationToTest.Y = kingPosition.Y - offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.BlackKnight)) { return true; }//check offset -1,-2
                locationToTest.X = kingPosition.X - offsetX;
                locationToTest.Y = kingPosition.Y + offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.BlackKnight)) { return true; }//check offset -1,+2

                offsetX = 2;//switch the offsets
                offsetY = 1;

                locationToTest.X = kingPosition.X + offsetX;
                locationToTest.Y = kingPosition.Y + offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.BlackKnight)) { return true; }//check offset +2,+1
                locationToTest.X = kingPosition.X + offsetX;
                locationToTest.Y = kingPosition.Y - offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.BlackKnight)) { return true; }//check offset +2,-1
                locationToTest.X = kingPosition.X - offsetX;
                locationToTest.Y = kingPosition.Y - offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.BlackKnight)) { return true; }//check offset -2,-1
                locationToTest.X = kingPosition.X - offsetX;
                locationToTest.Y = kingPosition.Y + offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.BlackKnight)) { return true; }//check offset -2,+1
            }
            else//black king
            {
                locationToTest = new ChessLocation(kingPosition.X + offsetX, kingPosition.Y + offsetY);
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.WhiteKnight)) { return true; }//check offset +1,+2
                locationToTest.X = kingPosition.X + offsetX;
                locationToTest.Y = kingPosition.Y - offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.WhiteKnight)) { return true; }//check offset +1,-2
                locationToTest.X = kingPosition.X - offsetX;
                locationToTest.Y = kingPosition.Y - offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.WhiteKnight)) { return true; }//check offset -1,-2
                locationToTest.X = kingPosition.X - offsetX;
                locationToTest.Y = kingPosition.Y + offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.WhiteKnight)) { return true; }//check offset -1,+2

                offsetX = 2;//switch the offsets
                offsetY = 1;

                locationToTest.X = kingPosition.X + offsetX;
                locationToTest.Y = kingPosition.Y + offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.WhiteKnight)) { return true; }//check offset +2,+1
                locationToTest.X = kingPosition.X + offsetX;
                locationToTest.Y = kingPosition.Y - offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.WhiteKnight)) { return true; }//check offset +2,-1
                locationToTest.X = kingPosition.X - offsetX;
                locationToTest.Y = kingPosition.Y - offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.WhiteKnight)) { return true; }//check offset -2,-1
                locationToTest.X = kingPosition.X - offsetX;
                locationToTest.Y = kingPosition.Y + offsetY;
                if (validLocation(ref locationToTest) && (board[locationToTest] == ChessPiece.WhiteKnight)) { return true; }//check offset -2,+1
            }
            return false;
        }
        /// <summary>
        /// This method checks if a location is is valid (on the board)
        /// </summary>
        /// <param name="location"></param>
        /// <returns>true if X and Y coordinates are valid</returns>
        private static bool validLocation(ref ChessLocation location)
        {
            if (location.X < 0 || location.X > 7) { return false; }
            if (location.Y < 0 || location.Y > 7) { return false; }
            return true;
        }

        #endregion

        #region Methods to calculate position cost

            /// <summary>
            /// This method determines the total cost of the pieces on board relative to a player color (positive is beneficial)
            /// </summary>
            /// <param name="chessPiece"></param>
            /// <param name="myColor"></param>
            /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static int CalcPieceCost(ChessBoard board, ChessColor myColor, out int enemyPieceCount)
        { // Got through the entire board one tile at a time adding up piece cost
            int cost = 0;
            int whiteCount = 0;
            int blackCount = 0;
            for (short Y = 0; Y < ChessBoard.NumberOfRows; Y++)
            {
                for (short X = 0; X < ChessBoard.NumberOfColumns; X++)//iterate through every square on board
                {
                    switch (board[X, Y])
                    {
                        // Add up cost of all pieces currently on board.
                        case ChessPiece.WhitePawn:
                            if(Y>4 && (X==3 || X== 4))//control middle
                            {
                                cost -= 5;//push pawns
                            }
                            if((X>0 && Y>0 && board[X-1,Y-1] > ChessPiece.Empty) || (X<7 && Y>0 && board[X+1,Y-1] > ChessPiece.Empty))
                            {
                                cost+=10;//defended piece
                            }
                            cost+=1000;
                            ++whiteCount;
                            break;

                        case ChessPiece.WhiteKnight:
                            if(X<=1 || X >= 6 || Y <= 1 || Y>= 6)//keep knights away from edges
                            {
                                cost-=2;
                            }
                            cost += 3000;
                            ++whiteCount;
                            break;
                        case ChessPiece.WhiteBishop:
                            if (Y < 7)//get bishop out of the backline
                            {
                                cost+=3;
                            }
                            cost += 3000;
                            ++whiteCount;
                            break;

                        case ChessPiece.WhiteRook:
                            cost += 5000;
                            ++whiteCount;
                            break;

                        case ChessPiece.WhiteQueen:
                            cost += 9000;
                            ++whiteCount;
                            break;

                        case ChessPiece.WhiteKing:
                            cost += 99999;
                            if (Y ==6)//get bishop out of the backline
                            {
                                ++cost;
                            }
                            ++whiteCount;
                            break;
                        case ChessPiece.BlackPawn:
                            cost-=1000;
                            if (Y < 4 && (X == 3 || X == 4))//control middle
                            {
                                cost += 5;//push pawns
                            }
                            if ((X>0 && Y<7 && board[X - 1, Y + 1] < ChessPiece.Empty) || (X<7 && Y>0 && board[X + 1, Y + 1] < ChessPiece.Empty))
                            {
                                cost-=10;//defended piece
                            }
                            ++blackCount;
                            break;

                        case ChessPiece.BlackKnight:
                            if (X <= 1 || X >= 6 || Y <= 1 || Y >= 6)//keep knights away from edges
                            {
                                cost+=2;
                            }
                            cost -= 3000;
                            ++blackCount;
                            break;
                        case ChessPiece.BlackBishop:
                            if (Y > 0)//get bishop out of the backline
                            {
                                cost-=3;
                            }
                            cost -= 3000;
                            ++blackCount;
                            break;

                        case ChessPiece.BlackRook:
                            cost -= 5000;
                            ++blackCount;
                            break;

                        case ChessPiece.BlackQueen:
                            cost -= 9000;
                            ++blackCount;
                            break;

                        case ChessPiece.BlackKing:
                            if (Y == 0)//get bishop out of the backline
                            {
                                --cost;
                            }
                            cost -= 99999;
                            ++blackCount;
                            break;
                        default://empty square
                            continue;
                    }
                }
            }
            if (myColor == ChessColor.Black)//player is black so negative value is good
            {
                cost *= -1;//therefore switch signs
                enemyPieceCount = whiteCount;
            }
            else
            {
                enemyPieceCount = blackCount;
            }
            return cost;
        }

        /*
        /// <summary>
        /// This method determines the cost of the board based on defended pieces relative to a player color (positive is beneficial)
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static short CalcDefendedCost(ChessBoard board, ChessColor myColor)
        { // Got through the entire board one tile at a adding up defended pieces
            short cost = 0;
            ChessLocation currentLocation = new ChessLocation(0, 0);
            ChessLocation[] defendedPieces = new ChessLocation[ChessBoard.NumberOfColumns * 4];//board has a max of 4 rows of pieces
            short numOfDefendedPieces = 0;
            for (short Y = 0; Y < ChessBoard.NumberOfRows; Y++)
            {
                for (short X = 0; X < ChessBoard.NumberOfColumns; X++)//iterate through every square on board
                {
                    currentLocation.X = X;
                    currentLocation.Y = Y;
                    switch (board[X, Y])
                    {
                        // Add up cost of all pieces currently on board.
                        case ChessPiece.WhitePawn:
                            PawnIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            break;

                        case ChessPiece.WhiteKnight:
                            KnightIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            cost += 3;
                            break;
                        case ChessPiece.WhiteBishop:
                            BishopIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            cost += 3;
                            break;

                        case ChessPiece.WhiteRook:
                            RookIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            cost += 5;
                            break;

                        case ChessPiece.WhiteQueen:
                            QueenIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            cost += 9;
                            break;
                        case ChessPiece.BlackPawn:
                            PawnIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            --cost;
                            break;

                        case ChessPiece.BlackKnight:
                            KnightIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            cost -= 3;
                            break;
                        case ChessPiece.BlackBishop:
                            BishopIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            cost -= 3;
                            break;

                        case ChessPiece.BlackRook:
                            RookIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            cost -= 5;
                            break;

                        case ChessPiece.BlackQueen:
                            QueenIsDefending(ref board, ref currentLocation, ref defendedPieces, ref myColor);
                            cost -= 9;
                            break;
                        default://empty square
                            continue;
                    }
                }
            }
            if (myColor == ChessColor.Black)//player is black so negative value is good
            {
                cost *= -1;//therefore switch signs
            }
            return cost;
        }
        */

            /*
        /// <summary>
        /// This method which pieces are being defended by a certain pawn
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static int PawnIsDefending(ref ChessBoard board, ref ChessLocation defender, ref ChessLocation[] defending, ref ChessColor color)
        { // find which pieces are being defended
            int count = 0;
            int cost = 1;
            if(color == ChessColor.White)
            {
                if (board[defender.X - 1, defender.Y - 1]!=ChessPiece.Empty &&
                    isEnemy(board[defender.X-1,defender.Y-1], color) == false)//friendly piece
                {
                    ++count;
                    ++cost;
                }
                if (board[defender.X + 1, defender.Y - 1] != ChessPiece.Empty &&
                    isEnemy(board[defender.X - 1, defender.Y - 1], color) == false)//friendly piece
                {
                    ++count;
                    ++cost;
                }
            }
            else
            {
                if (board[defender.X - 1, defender.Y + 1] != ChessPiece.Empty &&
                    isEnemy(board[defender.X - 1, defender.Y - 1], color) == false)//friendly piece
                {
                    ++count;
                    ++cost;
                }
                if (board[defender.X + 1, defender.Y + 1] != ChessPiece.Empty &&
                    isEnemy(board[defender.X - 1, defender.Y - 1], color) == false)//friendly piece
                {
                    ++count;
                    ++cost;
                }
            }
            return (cost - count);
        }
        /// <summary>
        /// This method which pieces are being defended by a certain bishop
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static int BishopIsDefending(ref ChessBoard board, ref ChessLocation defender, ref ChessLocation[] defending, ref ChessColor color)
        { // find which pieces are being defended
            throw (new NotImplementedException());
            int count = 0;
            int cost = 1;
            return (cost - count);
        }
        /// <summary>
        /// This method which pieces are being defended by a certain pawn
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static int KnightIsDefending(ref ChessBoard board, ref ChessLocation defender, ref ChessLocation[] defending, ref ChessColor color)
        { // find which pieces are being defended
            throw (new NotImplementedException());
            int count = 0;
            int cost = 1;
            return (cost - count);
        }
        /// <summary>
        /// This method which pieces are being defended by a certain pawn
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static int RookIsDefending(ref ChessBoard board, ref ChessLocation defender, ref ChessLocation[] defending, ref ChessColor color)
        { // find which pieces are being defended
            throw (new NotImplementedException());
            int count = 0;
            int cost = 1;
            return (cost - count);
        }
        /// <summary>
        /// This method which pieces are being defended by the queen
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static int QueenIsDefending(ref ChessBoard board, ref ChessLocation defender, ref ChessLocation[] defending, ref ChessColor color)
        { // find which pieces are being defended
            throw (new NotImplementedException());
            int count = 0;
            int cost = 1;
            return (cost - count);
        }
        */
        #endregion

        #region Search algorithms and heuristic functions

        /// <summary>
        /// An enum that represents the different heuristic functions.
        /// </summary>
        private enum Heuristic
        {
            PieceCost,
            Defenders,
            Pin,
            Fork
        }

        /// <summary>
        /// This method determines the best move for current tree based off min and max values
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="board"></param>
        /// <param name="myColor"></param>
        private ChessMove MiniMax(int depth, ref ChessBoard board, ChessColor myColor, List<ChessMove> myMoves)
        {
            int alpha = -999999999;//-infinity
            int beta = 999999999;
            int currentValue;
            bool noMoves = true;
            previousMinBoardValue = 0;
            previousMaxBoardValue = 0;
            Random random = new Random();
            int randomValue = random.Next(myMoves.Count) % myMoves.Count;
            if (depth > 1)//list is sorted after depth 1
            {
                randomValue = 0;//check the most promising first
            }
            ChessMove bestMove = myMoves[randomValue];
            ChessBoard boardAfterMove = board.Clone();
            boardAfterMove.MakeMove(bestMove);
            alpha = Min(depth, ref boardAfterMove, myColor, alpha, beta);
            bestMove.ValueOfMove = alpha;
            foreach (ChessMove move in myMoves)
            {
                if (IsMyTurnOver())
                {
                    return bestMove;
                }
                if (move.From.X == bestMove.From.X && move.From.Y == bestMove.From.Y && move.To.X == bestMove.To.X && move.To.Y == bestMove.To.Y)//don't check nodes that have already been checked
                {
                    continue;
                }
                noMoves = false;
                boardAfterMove = board.Clone();
                boardAfterMove.MakeMove(move);
                currentValue = Min(depth, ref boardAfterMove, myColor, alpha, beta);//check full depth limit
                move.ValueOfMove = currentValue;
                if(currentValue > alpha)
                {
                    alpha = currentValue;
                    bestMove = move;
                    bestMove.ValueOfMove = alpha;
                }
            }
            return bestMove;
        }

        /// <summary>
        /// This method determines the best move for opponent
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="board"></param>
        /// <param name="myColor"></param>
        private int Min(int depth, ref ChessBoard board, ChessColor myColor, int alpha, int beta)
        {
            int currentValue = 0;
            int minValue = 999999999;
            //if (depth <= 0){ return CalcPieceCost(board, myColor, out currentValue); }
            if (IsMyTurnOver())
            {
                return CalcPieceCost(board, myColor, out currentValue);
            }
            ChessColor oppColor = (myColor == ChessColor.White ? ChessColor.Black : ChessColor.White);
            List<ChessMove> oppMoves = GetAllMoves(board, oppColor);
            /*if (depth <= 0)//reached final node depth return value
            {
                foreach (ChessMove oppMove in oppMoves)
                {
                    if (MovesIntoCheck(ref board, oppMove))//skip invalid moves
                    {
                        continue;
                    }
                    if (board[oppMove.To] != ChessPiece.Empty && (Math.Abs((int)board[oppMove.From]) - 7) <= (int)board[oppMove.To])
                    {
                        ChessPiece tempPieceFrom = board[oppMove.From];// save previous board state piece at from location
                        ChessPiece tempPieceTo = board[oppMove.To];// save previous board state piece at to location
                        board.MakeMove(oppMove);
                        currentValue = Max(depth - 1, ref board, myColor, alpha, beta);
                        board[oppMove.To] = tempPieceTo;//restore previous board state piece at to location
                        board[oppMove.From] = tempPieceFrom;// restore previous board state piece at from location
                        if (currentValue < minValue)
                        {
                            minValue = currentValue;
                        }
                        if (minValue <= alpha) { return minValue; }
                        if (minValue < beta)
                        {
                            beta = minValue;
                        }
                    }
                    if (IsMyTurnOver())
                    {
                        return minValue;
                    }
                }
                if (minValue != 999999999)
                {
                    return minValue;
                }
                return CalcPieceCost(board, myColor, out currentValue);
            }*/

            foreach (ChessMove oppMove in oppMoves)
            {
                if (MovesIntoCheck(ref board, oppMove))//skip invalid moves
                {
                    continue;
                }
                if (IsMyTurnOver())
                {
                    return minValue;
                }
                ChessPiece tempPieceTo = board[oppMove.To];// save previous board state piece at to location
                ChessPiece tempPieceFrom = board[oppMove.From];// save previous board state piece at from location
                board.MakeMove(oppMove);
                if(depth > 1 && tempPieceTo == ChessPiece.Empty)//quiet state (no captures)
                {
                    currentValue = Max(depth-2, ref board, myColor, alpha, beta);//quiet position so don't search too deeply
                }
                else
                {
                    currentValue = Max(depth - 1, ref board, myColor, alpha, beta);
                }
                board[oppMove.To] = tempPieceTo;//restore previous board state piece at to location
                board[oppMove.From] =  tempPieceFrom;// restore previous board state piece at from location
                if (currentValue < minValue)
                {
                    minValue = currentValue;
                }
                if (minValue <= alpha) { return minValue; }
                if (minValue < beta)
                {
                    beta = minValue;
                }
            }
            return minValue;//return worst moves value for myColor at this depth
        }

        /// <summary>
        /// This method determines the best move for my color
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="board"></param>
        /// <param name="myColor"></param>
        private int Max(int depth, ref ChessBoard board, ChessColor myColor, int alpha, int beta)
        {
            int currentValue = 0;
            int maxValue = -999999999;
            if (depth <= 0) { return CalcPieceCost(board, myColor, out currentValue); }//reached leaf node return value
            if (IsMyTurnOver())
            {
                return CalcPieceCost(board, myColor, out currentValue);
            }
            List<ChessMove> myMoves = GetAllMoves(board, myColor);
            if (depth <= 0)
            {
                foreach (ChessMove myMove in myMoves)
                {
                    if (MovesIntoCheck(ref board, myMove))//skip invalid moves
                    {
                        continue;
                    }
                    if (board[myMove.To] != ChessPiece.Empty && (Math.Abs((int)board[myMove.From])-7) <= (int)board[myMove.To])
                    {
                        ChessPiece tempPieceTo = board[myMove.To];// save previous board state piece at to location
                        ChessPiece tempPieceFrom = board[myMove.From];// save previous board state piece at from location
                        board.MakeMove(myMove);
                        currentValue = Min(depth, ref board, myColor, alpha, beta);
                        board[myMove.To] = tempPieceTo;//restore previous board state piece at to location
                        board[myMove.From] = tempPieceFrom;// restore previous board state piece at from location
                        if (currentValue > maxValue)
                        {
                            maxValue = currentValue;
                        }
                        if (maxValue >= beta) { return maxValue; }
                        if (maxValue > alpha)
                        {
                            alpha = maxValue;
                        }
                    }
                    if (IsMyTurnOver())
                    {
                        return CalcPieceCost(board, myColor, out currentValue);
                    }
                }
                if(maxValue != -999999999)
                {
                    return maxValue;
                }
                return CalcPieceCost(board, myColor, out currentValue);
            }

            foreach (ChessMove move in myMoves)
            {
                if (MovesIntoCheck(ref board, move))//skip invalid moves
                {
                    continue;
                }
                if (IsMyTurnOver())
                {
                    return CalcPieceCost(board, myColor, out currentValue);
                }
                ChessPiece tempPieceTo = board[move.To];// save previous board state piece at to location
                ChessPiece tempPieceFrom = board[move.From];// save previous board state piece at from location
                board.MakeMove(move);
                currentValue = Min(depth, ref board, myColor, alpha, beta);
                board[move.To] = tempPieceTo;//restore previous board state piece at to location
                board[move.From] = tempPieceFrom;// restore previous board state piece at from location
                if (currentValue > maxValue)
                {
                    maxValue = currentValue;
                }
                if (maxValue >= beta) { return maxValue; }
                if (maxValue > alpha)
                {
                    alpha = maxValue;
                }
            }
            return maxValue;//return best moves value for myColor at this depth
        }

        /// <summary>
        /// This method determines the best move based soley on a heuristic functions value
        /// </summary>
        /// <param name="board"></param>
        /// <param name="moves"></param>
        /// <param name="tree"></param>
        /// <param name="myColor"></param>
        /// <param name="choice"></param>
        /// <returns></returns>
        private ChessMove Greedy(ref ChessBoard board, ref List<ChessMove> moves, ChessColor myColor, Heuristic choice)
        {
            int currentValue = 0;
            int bestValue = 0;
            int enemyPieceCount=0;
            Random random = new Random();
            ChessMove bestMove;
            ChessPiece tempPieceTo;
            ChessPiece tempPieceFrom;
            ChessPiece enemyKing;
            ChessPiece myKing;
            if (myColor == ChessColor.White)
            {
                myKing = ChessPiece.WhiteKing;
                enemyKing = ChessPiece.BlackKing;
            }
            else
            {
                myKing = ChessPiece.BlackKing;
                enemyKing = ChessPiece.WhiteKing;
            }
            do//pick a random move to initialize bestMove but make sure it doesn't put our king in check
            {
                if (moves.Count == 0)//no moves left
                {
                    bestMove = new ChessMove(null, null);
                    if(KingInCheck(ref board, myKing))
                    {
                        bestMove.Flag = ChessFlag.Checkmate;
                    }
                    else
                    {
                        bestMove.Flag = ChessFlag.Stalemate;
                    }
                    return bestMove;
                }
                int randomValue = random.Next(moves.Count) % moves.Count;
                bestMove = moves[randomValue];
                if(MovesIntoCheck(ref board, bestMove) == false)//if my king isn't in check
                {
                    break;//break out of loop
                }
                moves.RemoveAt(randomValue);//remove move that puts me into check
            } while (true);//repeat until our king isn't in check

            foreach (ChessMove move in moves)
            {
                if (MovesIntoCheck(ref board, move) == true)//if my king is in check
                {
                    move.Flag = ChessFlag.Check;
                    continue;
                }
                tempPieceTo = board[move.To];// save previous board state piece to
                tempPieceFrom = board[move.From];// save previous board state piece from
                board.MakeMove(move);
                if ((move.To.Y == 0 || move.To.Y == 7) && (tempPieceFrom == ChessPiece.BlackPawn || tempPieceFrom == ChessPiece.WhitePawn))//queening
                {
                    board[move.To] = ChessPiece.WhiteQueen;//promote pawn
                    if(tempPieceFrom == ChessPiece.BlackPawn)//to black queen if pawn was black
                    {
                        board[move.To] = ChessPiece.BlackQueen;
                    }
                }
                switch (choice)
                {
                    case Heuristic.PieceCost://adds up the value of pieces on board for each player
                        currentValue = CalcPieceCost(board, myColor, out enemyPieceCount);
                        break;
                    case Heuristic.Defenders:
                        //currentValue = CalcDefendedCost(board, myColor);//adds up defended pieces for each player
                        break;
                    default:
                        throw (new NotImplementedException());
                }
                if (enemyPieceCount <= 3)//only a few enemy pieces are left so it's okay to do some costly operations
                {
                    List<ChessMove> oppMoves = GetAllMoves(board, (ChessColor)Math.Abs((int)myColor - 1));//get all opponents moves
                    bool noValidMoves = true;
                    foreach (ChessMove oppMove in oppMoves)
                    {
                        if (MovesIntoCheck(ref board, oppMove) == false)
                        {
                            noValidMoves = false;
                            break;
                        }
                    }
                    if (noValidMoves)//if opponent is out of moves
                    {
                        if (KingInCheck(ref board, enemyKing))//checkmate!
                        {
                            move.Flag = ChessFlag.Checkmate;
                            return move;
                        }
                        currentValue -= 1000;//avoid stalemate!
                    }
                    if (tempPieceFrom==ChessPiece.BlackPawn || tempPieceFrom == ChessPiece.WhitePawn)//prioritize queening at endgame
                    {
                        currentValue+=2;
                    }
                }
                if(KingInCheck(ref board, enemyKing))//if enemy king is in check with current move
                {
                    move.Flag = ChessFlag.Check;
                    ++currentValue;//make this move worth more
                }
                if (currentValue > bestValue)//position is better than previously checked postions
                {
                    bestValue = currentValue;
                    bestMove = move;
                }
                //restore board to original state
                board[move.To] = tempPieceTo;
                board[move.From] = tempPieceFrom;
            }
            tempPieceTo = board[bestMove.To];// save previous board state piece to
            tempPieceFrom = board[bestMove.From];// save previous board state piece from
            board.MakeMove(bestMove);
            if (bestMove.Flag == ChessFlag.Check && GetKingMoves(board, (ChessColor)Math.Abs((int)myColor - 1), -1, -1).Count == 0)//king can't move
            {
                //do some costly operations to check for checkmate since bestMove is found, king can't move, and king in check
                //note this catches checkmate in early and mid game. Above it only checks for checkmate when few pieces are left
                List<ChessMove> oppMoves = GetAllMoves(board, (ChessColor)Math.Abs((int)myColor - 1));//get all opponents moves
                bool noValidMoves = true;

                foreach (ChessMove oppMove in oppMoves)
                {
                    if (MovesIntoCheck(ref board, oppMove) == false)
                    {
                        noValidMoves = false;
                        break;
                    }
                }
                if (noValidMoves)//if opponent is out of moves
                {
                    if (KingInCheck(ref board, enemyKing))//checkmate!
                    {
                        bestMove.Flag = ChessFlag.Checkmate;
                    }
                }
            }
            //restore board to original state
            board[bestMove.To] = tempPieceTo;
            board[bestMove.From] = tempPieceFrom;
            return bestMove;
        }

        /*
        /// <summary>
        /// Gets rid of moves that don't have potential to be the best
        /// </summary>
        /// <param name="board"></param>
        /// <param name="myColor"></param>
        /// <param name="choice"></param>
        /// <returns></returns>
        private static DecisionTree AlphaBetaPruning(ChessBoard board, ChessColor myColor, Heuristic choice)
        { // Got through the entire board one tile at a time adding up piece cost
            throw (new NotImplementedException());
            return null;
        }
        */
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

#if DEBUG
        private enum StudentAIProfilerTags
        {
            AddAllPossibleMovesToDecisionTree,
            GetAllMoves,
            GetNextMove,
            IsValidMove,
            MoveAPawn
        }
#endif
    }
}
