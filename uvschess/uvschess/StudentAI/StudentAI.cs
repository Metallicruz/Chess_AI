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
            /*
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
            */
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
            throw (new NotImplementedException());
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
            throw (new NotImplementedException());
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
            throw (new NotImplementedException());
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
            // This method only generates moves for pawns to move one space forward.
            // It does not generate moves for any other pieces.
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
                                break;

                            case ChessPiece.WhiteRook:
                                break;

                            case ChessPiece.WhiteQueen:
                                break;

                            case ChessPiece.WhiteKing:
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
                                break;

                            case ChessPiece.BlackRook:
                                break;

                            case ChessPiece.BlackQueen:
                                break;

                            case ChessPiece.BlackKing:
                                break;

                        }
                    }

                    
                    
                    
                }
            }

            return allMoves;
        }

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
        private bool isEnemy(ChessPiece chessPiece, ChessColor myColor)
        {
            List<ChessPiece> enemyPieces;

            if (myColor == ChessColor.White)
            {
                enemyPieces = new List<ChessPiece> {ChessPiece.BlackBishop, ChessPiece.BlackKing, ChessPiece.BlackKnight,
                    ChessPiece.BlackPawn, ChessPiece.BlackQueen, ChessPiece.BlackRook };
            }
            else
            {
                enemyPieces = new List<ChessPiece> {ChessPiece.WhiteBishop, ChessPiece.WhiteKing, ChessPiece.WhiteKnight,
                    ChessPiece.WhitePawn, ChessPiece.WhiteQueen, ChessPiece.WhiteRook };
            }

            if (enemyPieces.Contains(chessPiece))
            {
                return true;
            }
            else
            {
                return false;
            }
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
