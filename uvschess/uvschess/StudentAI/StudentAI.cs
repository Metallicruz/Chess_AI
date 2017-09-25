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

            ChessMove myNextMove = null;

            this.Log($"{IsMyTurnOver()}");

            while (!IsMyTurnOver())
            {
                Random random = new Random();
                List<ChessMove> allMoves = GetAllMoves(board, myColor);
                int moveNumber = random.Next(allMoves.Count);

                if (allMoves.Count == 0)
                {
                    // If I couldn't find a valid move easily, 
                    // I'll just create an empty move and flag a stalemate.
                    myNextMove = new ChessMove(null, null);
                    myNextMove.Flag = ChessFlag.Stalemate;
                }
                else
                {
                    myNextMove = allMoves[moveNumber];

                    AddAllPossibleMovesToDecisionTree(allMoves, myNextMove, board.Clone(), myColor);
                }

                this.Log(myColor.ToString() + " (" + this.Name + ") just moved.");
                this.Log(string.Empty);

                // Since I have a move, break out of loop
                break;
                // Implement Greedy on all legal moves
            }

            //Profiler.SetDepthReachedDuringThisTurn(2);
            return myNextMove;
        }

        public void AddAllPossibleMovesToDecisionTree(List<ChessMove> allMyMoves, ChessMove myChosenMove,
                                                      ChessBoard currentBoard, ChessColor myColor)
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

        #endregion

        #region Methods to check if a move is valid.

        /// <summary>
        /// Validates a move. The framework uses this to validate the opponents move.
        /// </summary>
        /// <param name="boardBeforeMove">The board as it currently is _before_ the move.</param>
        /// <param name="moveToCheck">This is the move that needs to be checked to see if it's valid.</param>
        /// <param name="colorOfPlayerMoving">This is the color of the player who's making the move.</param>
        /// <returns>Returns true if the move was valid</returns>
        public bool IsValidMove(ChessBoard boardBeforeMove, ChessMove moveToCheck, ChessColor colorOfPlayerMoving)
        {

            /*
			if (the moveToCheck To and From has a piece of same color) then return false
			if (moveToCheck To results in ChessFlag.Check or ChessFlag.Checkmate for colorOfPlayerMoving) then return false
			*/
            bool isValid = true;

            switch (boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y])
            {
                case ChessPiece.WhitePawn:
                case ChessPiece.BlackPawn:
                    //isValid = PawnMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
                    break;
                case ChessPiece.WhiteRook:
                case ChessPiece.BlackRook:
                    isValid = RookToMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
                    break;
                case ChessPiece.WhiteKnight:
                case ChessPiece.BlackKnight:
                    //isValid = KnightMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
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
                        isValid = false;
                    }
					break;
                case ChessPiece.BlackKing:
				case ChessPiece.WhiteKing:
					//isValid = KingMove(boardBeforeMove, moveToCheck, colorOfPlayerMoving);
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
            if (moveToCheck.From.Y == 6 && boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y] > 0) {// starting 2nd row and is white

            }
            else if (moveToCheck.From.Y == 1 && boardBeforeMove[moveToCheck.From.X, moveToCheck.From.Y] < 0) {//starting from 7th row

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
            //offset 2 in one direction 1 in the other
            int offsetX=Math.Abs(moveToCheck.To.X - moveToCheck.From.X);
			int offsetY=Math.Abs(moveToCheck.To.Y - moveToCheck.From.Y);
			if(offsetX==1 || offsetY==1){
				if(offsetX==2 || offsetY==2){
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
			
            throw (new NotImplementedException());
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
            throw (new NotImplementedException());
        }
        #endregion

        #region Methods to generate a list of valid moves.


        /// <summary>
        /// This method generates all valid moves for myColor based on the currentBoard
        /// </summary>
        /// <param name="currentBoard">This is the current board to generate the moves for.</param>
        /// <param name="myColor">This is the color of the player to generate the moves for.</param>
        /// <returns>List of ChessMoves</returns>
        List<ChessMove> GetAllMoves(ChessBoard currentBoard, ChessColor myColor)
        {
#if DEBUG
            Profiler.IncrementTagCount((int)StudentAIProfilerTags.GetAllMoves);
#endif
         
            List<ChessMove> allMoves = new List<ChessMove>();

            // Got through the entire board one tile at a time looking for chess pieces I can move
            for (int Y = 0; Y < ChessBoard.NumberOfRows; Y++)
            {
                for (int X = 0; X < ChessBoard.NumberOfColumns; X++)
                {
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
        /// This method returns a list of all possible moves the King piece make.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="myColor"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        private List<ChessMove> GetKingMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
        {
            List<ChessMove> kingMoves = new List<ChessMove>();

            // Down
            if (Y < 7)
            {
                if (currentBoard[X, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X, Y + 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y + 1)));
                }
            }

            // Up
            if (Y > 0)
            {
                if (currentBoard[X, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X, Y - 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X, Y - 1)));
                }
            }

            // Right
            if (X < 7)
            {
                if (currentBoard[X + 1, Y] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y)));
                }
            }

            // Left
            if (X > 0)
            {
                if (currentBoard[X - 1, Y] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y)));
                }
            }

            // DownRight
            if (Y < 7 && X < 7)
            {
                if (currentBoard[X + 1, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y + 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y + 1)));
                }
            }

            // DownLeft
            if (Y < 7 && X > 0)
            {
                if (currentBoard[X - 1, Y + 1] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y + 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y + 1)));
                }
            }

            // UpRight
            if (Y > 0 && X < 7)
            {
                if (currentBoard[X + 1, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X + 1, Y - 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X + 1, Y - 1)));
                }
            }

            // UpLeft
            if (Y > 0 && X > 0)
            {
                if (currentBoard[X - 1, Y - 1] == ChessPiece.Empty || isEnemy(currentBoard[X - 1, Y - 1], myColor))
                {
                    kingMoves.Add(new ChessMove(new ChessLocation(X, Y), new ChessLocation(X - 1, Y - 1)));
                }
            }

            return kingMoves;
        }

        /// <summary>
        /// This method returns a list of all possible moves the Rook piece make.
        /// </summary>
        /// <param name="currentBoard"></param>
        /// <param name="myColor"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        private List<ChessMove> GetRookMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
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
        /// <returns></returns>
        private List<ChessMove> GetBishopMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
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
        /// <returns></returns>
        private List<ChessMove> GetKnightMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
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
        /// <returns></returns>
        private List<ChessMove> GetPawnMoves(ChessBoard currentBoard, ChessColor myColor, int X, int Y)
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

                    if (Y == 1 && currentBoard[X, Y + 2] == ChessPiece.Empty && currentBoard[X, Y - 1] == ChessPiece.Empty)
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
        /// <returns></returns>
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

        #region Methods related to checks on kings

        /// <summary>
        /// This method determines whether a king is in check
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>returns the king that is in check or empty if neither are in check</returns>
        private static ChessPiece KingInCheck(ref ChessBoard board)
        {
            throw (new NotImplementedException());
            if (true)
            {
                return ChessPiece.WhiteKing;
            }
            else if (true)
            {
                return ChessPiece.BlackKing;
            }
            return ChessPiece.Empty;
        }

        /// <summary>
        /// This method determines whether a king is in check more efficiently by factoring in only a single moves impact on board
        /// </summary>
        /// <param name="board"></param>
        /// <param name="move"></param>
        /// <returns>returns the king that is in check or empty if neither are in check</returns>
        private static ChessPiece KingInCheck(ref ChessBoard board, ref ChessMove move)
        {
            throw (new NotImplementedException());
            if (true)
            {
                return ChessPiece.WhiteKing;
            }
            else if (true)
            {
                return ChessPiece.BlackKing;
            }
            return ChessPiece.Empty;
        }

        #endregion

        #region Methods to calculate position cost.

        /// <summary>
        /// This method determines the total cost of the pieces on board relative to a player color (positive is beneficial)
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static short CalcPieceCost(ChessBoard board, ChessColor myColor)
        { // Got through the entire board one tile at a time adding up piece cost
            short cost = 0;
            for (short Y = 0; Y < ChessBoard.NumberOfRows; Y++)
            {
                for (short X = 0; X < ChessBoard.NumberOfColumns; X++)//iterate through every square on board
                {
                    switch (board[X, Y])
                    {
                        // Add up cost of all pieces currently on board.
                        case ChessPiece.WhitePawn:
                            ++cost;
                            break;

                        case ChessPiece.WhiteKnight:
                        case ChessPiece.WhiteBishop:
                            cost += 3;
                            break;

                        case ChessPiece.WhiteRook:
                            cost += 5;
                            break;

                        case ChessPiece.WhiteQueen:
                            cost += 9;
                            break;

                        case ChessPiece.WhiteKing:
                            cost += 100;
                            break;
                        case ChessPiece.BlackPawn:
                            --cost;
                            break;

                        case ChessPiece.BlackKnight:
                        case ChessPiece.BlackBishop:
                            cost -= 3;
                            break;

                        case ChessPiece.BlackRook:
                            cost -= 5;
                            break;

                        case ChessPiece.BlackQueen:
                            cost -= 9;
                            break;

                        case ChessPiece.BlackKing:
                            cost -= 100;
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

        /// <summary>
        /// This method determines the cost of the board based on defended pieces relative to a player color (positive is beneficial)
        /// </summary>
        /// <param name="chessPiece"></param>
        /// <param name="myColor"></param>
        /// <returns>Total board value for a player (my pieces value minus enemy pieces value)</returns>
        private static short CalcDefendedCost(ChessBoard board, ChessColor myColor)
        { // Got through the entire board one tile at a adding up defended pieces
            throw (new NotImplementedException());
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

        #endregion

        #region Search algorithms and heuristic functions

        /// <summary>
        /// This method determines the best move for current tree based off min and max values
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="myColor"></param>
        private static void MiniMax(ref DecisionTree tree, ChessColor myColor)
        {
            throw (new NotImplementedException());
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
        private static ChessMove Greedy(ChessBoard board, ref List<ChessMove> moves, ref DecisionTree tree, ChessColor myColor, Heuristic choice)
        {
            int currentValue = 0;
            int bestValue = 0;
            ChessMove bestMove = null;
            ChessPiece tempPieceTo;
            ChessPiece tempPieceFrom;
            foreach (ChessMove move in moves)
            {
                tempPieceTo = board[move.To];// save previous board state piece to move to
                tempPieceFrom = board[move.From];// save previous board state piece to be moved
                board.MakeMove(move);//make temporary move
                switch (choice)
                {
                    case Heuristic.PieceCost:
                        currentValue = CalcPieceCost(board, myColor);
                        break;
                    case Heuristic.Defenders:
                        currentValue = CalcDefendedCost(board, myColor);
                        break;
                    default:
                        throw (new NotImplementedException());
                        break;
                }
                if (currentValue > bestValue)
                {
                    bestValue = currentValue;
                    bestMove = move;
                }
                //restore board to original state
                board[move.To] = tempPieceTo;
                board[move.From] = tempPieceFrom;
            }
            return bestMove;
        }


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
