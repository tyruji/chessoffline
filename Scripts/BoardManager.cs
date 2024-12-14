using Godot;
using System;
using System.Threading.Tasks;

public partial class BoardManager : Node2D
{
    public const int BOARD_SIDE_SIZE = 8;
    public const int SQUARE_SIZE = 64;

    [Export]
    public PackedScene _BoardSquarePrefab = null;

    [Export]
    public Color _WhiteSquareColor = Colors.White;

    [Export]
    public Color _BlackSquareColor = Colors.Black;

    [Export]
    public PromotionChoice PromotionChoice { get; set;}

    public MoveInfo LastMove { get; private set; }

    public Piece[,] Board = new Piece[ BOARD_SIDE_SIZE, BOARD_SIDE_SIZE ];
    public BoardSquare[,] BoardSquares = new BoardSquare[ BOARD_SIDE_SIZE, BOARD_SIDE_SIZE ];

    public override void _Ready()
    {
        PromotionChoice ??= GetNode<PromotionChoice>( "../CanvasLayer/PromotionChoice" );

            // ( 256, 256 ) is the center of the screen for now.
        CreateBoard( new Vector2( 256, 256 ) );
    }

    private Piece _previousPieceAtTestedPosition = null;
    private Piece _testedPiece = null;
    private Vector2I _previousPiecePositionAtTest = Vector2I.Zero;
    private Vector2I _lastTestedPiecePosition = Vector2I.Zero;

        // These methods are shorthands for testing a move
    public void TestMovePiece( Vector2I from, Vector2I to )
    {
        _previousPieceAtTestedPosition = GetPieceAt( to );
        _testedPiece = GetPieceAt( from );
        _previousPiecePositionAtTest = to;
        _lastTestedPiecePosition = from;

        _testedPiece.BoardPosition = to;
        SetPieceAt( to, _testedPiece );
        SetPieceAt( _lastTestedPiecePosition, null );
    }
    public void RevertTestMovePiece()
    {
        _testedPiece.BoardPosition = _lastTestedPiecePosition;
        SetPieceAt( _previousPiecePositionAtTest, _previousPieceAtTestedPosition );
        SetPieceAt( _lastTestedPiecePosition, _testedPiece );
    }

    public async Task PerformMove( Vector2I from, Vector2I to, bool notifyPiece = true )
    {
        var captured_piece = CapturePieceAt( to );

        var moved_piece = GetPieceAt( from );
        moved_piece.BoardPosition = to;

        SetPieceAt( to, moved_piece );
        SetPieceAt( from, null );

        LastMove = new MoveInfo
        {
            pieceMoved = moved_piece,
            pieceCaptured = captured_piece,
            from = from,
            to = to
        };

        if( !notifyPiece ) return;

        await moved_piece.OnPerformMove( this );
    }

        /// <summary>
        /// Removes the piece from the board and hides it.
        /// Returns the captured piece object.
        /// </summary>
        /// <param name="board_pos"></param>
        /// <returns></returns>
    public Piece CapturePieceAt( Vector2I board_pos )
    {
        var captured_piece = GetPieceAt( board_pos );
        SetPieceAt( board_pos, null );
        captured_piece?.Hide();

        return captured_piece;
    }

    public Piece GetPieceAt( Vector2I board_pos )
    {
            // If the position is out of bounds, return nothing.
        if( IsPositionOutOfBounds( board_pos ) ) return null;
        
        return Board[ board_pos.X, board_pos.Y ];
    }

    public Piece GetPieceAt( int x, int y ) => GetPieceAt( new Vector2I( x, y ) );

    public void SetPieceAt( Vector2I board_pos, Piece piece )
    {
        if( IsPositionOutOfBounds( board_pos ) ) return;

        Board[ board_pos.X, board_pos.Y ] = piece;
    }
    
    public void SetPieceAt( int x, int y, Piece piece ) => SetPieceAt( new Vector2I( x, y ), piece );

    public bool IsPositionOutOfBounds( Vector2I board_pos )
    {
        if( board_pos.X < 0 || board_pos.X >= BOARD_SIDE_SIZE ) return true;
        if( board_pos.Y < 0 || board_pos.Y >= BOARD_SIDE_SIZE ) return true;
        return false;
    }

    public bool IsPositionOutOfBounds( int x, int y ) => IsPositionOutOfBounds( new Vector2I( x, y ) );

    public void CreateBoard( Vector2 center )
    {
        for( int i = 0; i < BOARD_SIDE_SIZE; ++i )
        {
            for( int j = 0; j < BOARD_SIDE_SIZE; ++j )
            {
                Vector2 pos = BoardToWorldPosition( i, j );

                BoardSquare new_square = _BoardSquarePrefab.Instantiate<BoardSquare>();
                AddChild( new_square );
                new_square.GlobalPosition = pos;

                BoardSquares[ i, j ] = new_square;

                    // Create black-white grid.
                new_square.SelfModulate = _WhiteSquareColor;
                if( ( i + j ) % 2 == 0 ) continue;
                new_square.SelfModulate = _BlackSquareColor;
            }
        }

        GlobalPosition = center;

        SpawnPieces();
    }

    private void SpawnPieces()
    {
        CreateBothPlayerPiecesMirroredFromBlack<Rook>( 0, 0 );
        
        CreateBothPlayerPiecesMirroredFromBlack<Knight>( 1, 0 );

        CreateBothPlayerPiecesMirroredFromBlack<Bishop>( 2, 0 );

        CreateBothPlayerPiecesMirroredFromBlack<Pawn>( 0, 1 );
        CreateBothPlayerPiecesMirroredFromBlack<Pawn>( 1, 1 );
        CreateBothPlayerPiecesMirroredFromBlack<Pawn>( 2, 1 );
        CreateBothPlayerPiecesMirroredFromBlack<Pawn>( 3, 1 );

        CreateBothPlayerPieceFromBlack<Queen>( 3, 0 ); 
        CreateBothPlayerPieceFromBlack<King>( 4, 0 ); 
    }

    /// <summary>
    /// This creates symmetrical pieces for both players by using 
    /// just the black piece positions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="square_size"></param>
    private void CreateBothPlayerPiecesMirroredFromBlack<T>( int x, int y ) 
        where T : Piece, new()
    {
        CreateBothPlayerPieceFromBlack<T>( x, y ); 

            // Mirror the pieces on the other side.
        CreateBothPlayerPieceFromBlack<T>( BOARD_SIDE_SIZE - x - 1, y ); 
    }

    /// <summary>
    /// This creates a piece for both players by using 
    /// just the black piece position.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="square_size"></param>
    private void CreateBothPlayerPieceFromBlack<T>( int x, int y ) 
        where T : Piece, new()
    {
        Piece black_piece = Piece.CreateBlack<T>( x, y, SQUARE_SIZE );
        Board[ x, y ] = black_piece;
        AddChild( black_piece );

            // Flip the y for the white pieces.
        y = BOARD_SIDE_SIZE - y - 1;

        Piece white_piece = Piece.CreateWhite<T>( x, y, SQUARE_SIZE );
        Board[ x, y ] = white_piece;
        AddChild( white_piece );
    }

    public static Vector2I WorldToBoardPosition( Vector2 world_pos )
        => ( Vector2I ) ( ( world_pos / SQUARE_SIZE ) + Vector2.One * .5f * BOARD_SIDE_SIZE );

    public static Vector2 BoardToWorldPosition( Vector2I board_pos )
        => SQUARE_SIZE * ( board_pos - .5f * ( BOARD_SIDE_SIZE - 1 ) * Vector2.One );

    public static Vector2 BoardToWorldPosition( int x, int y )
        => BoardToWorldPosition( new Vector2I( x, y ) );
}

public struct MoveInfo
{
    public Piece pieceMoved;
    public Piece pieceCaptured;
    public Vector2I from;
    public Vector2I to;
}
