using Godot;
using System;
using System.Threading.Tasks;

public abstract partial class Piece : Area2D
{
    private const string TEXTURES_PATH = @"res://textures/"; 

    public static readonly Color WHITE_PIECE_COLOR = Colors.White;

    public static readonly Color BLACK_PIECE_COLOR = Colors.Brown;

    public abstract string PieceName { get; }

    /// <summary>
    /// Gives an index position from [0;7] for both X and Y
    /// </summary>
    public Vector2I BoardPosition
    {
        get => BoardManager.WorldToBoardPosition( Position );
        set => Position = BoardManager.BoardToWorldPosition( value );
    }

    public float squareSize = 64;

    public Sprite2D Sprite { get; private set; } = null;

    public ePieceColor PieceColor { get; private set; } = ePieceColor.WHITE;

    protected readonly Vector2I[] _kingCheckMoveBuffer 
            = new Vector2I[ BoardManager.BOARD_SIDE_SIZE * BoardManager.BOARD_SIDE_SIZE ];

    public RectangleShape2D SharedRectangleShape
    {
        get
        {
            if( _sharedRectangleShape == null )
            {
                _sharedRectangleShape = new RectangleShape2D();
                _sharedRectangleShape.Size = Vector2.One * squareSize;
            }
            
            return _sharedRectangleShape;
        }
    }
    private static RectangleShape2D _sharedRectangleShape = null;

    public static Piece CreateWhite<T>( int board_x,
        int board_y, float square_size ) where T : Piece, new()
    {
        return Create<T>( ePieceColor.WHITE, board_x, board_y, square_size );
    }
    public static Piece CreateBlack<T>( int board_x,
        int board_y, float square_size ) where T : Piece, new()
    {
        return Create<T>( ePieceColor.BLACK, board_x, board_y, square_size );
    }

    public static Piece Create<T>( ePieceColor piece_color, int board_x, 
        int board_y, float square_size ) where T : Piece, new()
    {
        T piece = new T();

        piece.PieceColor = piece_color;
        
        piece.squareSize = square_size;

        piece.Sprite = new Sprite2D();
        piece.AddChild( piece.Sprite );

        CollisionShape2D col_shape = new CollisionShape2D();
        col_shape.Shape = piece.SharedRectangleShape;
        piece.AddChild( col_shape );

        piece.Position = BoardManager.BoardToWorldPosition( new Vector2I( board_x, board_y ) );


        return piece;
    }


        // Main method
    public abstract void FillLegalMoveBuffer( Vector2I[] legal_move_buffer, BoardManager board_manager );

    public abstract void FillLegalMoveBufferWithoutCheckChecking( Vector2I[] legal_move_buffer, BoardManager board_manager );

    public async virtual Task OnPerformMove( BoardManager board_manager ) {}

    public override void _Ready()
    {
        Sprite.Texture = GD.Load<Texture2D>( TEXTURES_PATH + PieceName + ".png" );

        var color = BLACK_PIECE_COLOR;
        if( PieceColor == ePieceColor.WHITE ) color = WHITE_PIECE_COLOR;

        Sprite.Modulate = color;
    }

    public bool WillMovePutKingInCheck( Vector2I move, BoardManager board_manager )
    {
        board_manager.TestMovePiece( BoardPosition, move );

        bool will_move_put_king_in_check = IsKingInCheck( board_manager, this.PieceColor );

            // Put the pieces back.
        board_manager.RevertTestMovePiece();

        return will_move_put_king_in_check;
    }

    public bool IsKingInCheck( BoardManager board_manager, ePieceColor king_color )
    {
        Piece king = FindKing( board_manager, king_color );

            // lol
        if( king == null ) return false;

            // Check if any opponent's pieces put the king in check.

        for( int i = 0; i < BoardManager.BOARD_SIDE_SIZE; ++i )
        {
            for( int j = 0; j < BoardManager.BOARD_SIDE_SIZE; ++j )
            {
                var opponent_piece = board_manager.GetPieceAt( i, j );

                if( opponent_piece == null || opponent_piece.PieceColor == king_color ) continue;

                EmptyKingCheckBuffer();

                opponent_piece.FillLegalMoveBufferWithoutCheckChecking( _kingCheckMoveBuffer, board_manager );

                foreach( var opponent_move in _kingCheckMoveBuffer )
                {
                    if( opponent_move == GameHandler.NOT_A_MOVE ) break;

                    if( opponent_move != king.BoardPosition ) continue;

                    return true;
                }
            }
        }
        return false;
    }

    public static Piece FindKing( BoardManager board_manager, ePieceColor king_color )
    {
        for( int i = 0; i < BoardManager.BOARD_SIDE_SIZE; ++i )
        {
            for( int j = 0; j < BoardManager.BOARD_SIDE_SIZE; ++j )
            {
                var piece = board_manager.GetPieceAt( i, j );

                if( piece is not King king || king.PieceColor != king_color ) continue;

                return king;
            }
        }
        return null;    // This will probably never happen.
    }

    private void EmptyKingCheckBuffer()
    {
        for( int i = 0; i < _kingCheckMoveBuffer.Length; ++i )
            _kingCheckMoveBuffer[ i ] = GameHandler.NOT_A_MOVE;
    }
}

public enum ePieceColor
{
    WHITE,
    BLACK
}
