using Godot;
using System;

public partial class SquareHighlighter : Node2D
{
    [Export]
    public GameHandler GameHandler { get; set; }

    private BoardSquare _highlightedSquare = null;

    public override void _Ready()
    {
        GameHandler ??= GetNode<GameHandler>( "../GameHandler" );

        GameHandler.OnPiecePickup += HighlightSquares;
        GameHandler.OnPieceDrop += UnhighlightAllSquares;
    }

    public override void _Process( double delta )
    {
        if( _highlightedSquare != null )
        {
            _highlightedSquare.UnhighlightAll();
            _highlightedSquare = null;
        }
        
        if( GameHandler == null ) return;

        var board_pos = GameHandler.GetMousePositionOnBoard();

        Piece piece = GameHandler.BoardManager.GetPieceAt( board_pos );

        if( piece == null || piece.PieceColor != GameHandler.PlayerToPlay ) return;
        if( GameHandler.PickedUpPiece != null ) return;

        _highlightedSquare = GameHandler.BoardManager.BoardSquares[ board_pos.X, board_pos.Y ];
        _highlightedSquare.HighlightSelect( piece );
    }

    private void HighlightSquares()
    {
        foreach( var move in GameHandler.LegalMoveBuffer )
        {
            if( move == GameHandler.NOT_A_MOVE ) break;

            var piece_at_move = GameHandler.BoardManager.GetPieceAt( move );

            if( piece_at_move == null ) 
            {
                HighlightMove( move );
                continue;
            }

            HighlightCapture( move );
        }
    }

    private void UnhighlightAllSquares()
    {
        for( int i = 0; i < BoardManager.BOARD_SIDE_SIZE; ++i )
        {
            for( int j = 0; j < BoardManager.BOARD_SIDE_SIZE; ++j )
            {
                var square = GameHandler.BoardManager.BoardSquares[ i, j ];
                square.UnhighlightAll();
            }
        }
    }

    private void HighlightMove( Vector2I move )
    {
        var square = GameHandler.BoardManager.BoardSquares[ move.X, move.Y ];
        square.HighlightMove();
    }

    private void HighlightCapture( Vector2I move )
    {
        var square = GameHandler.BoardManager.BoardSquares[ move.X, move.Y ];
        var piece_to_capture = GameHandler.BoardManager.GetPieceAt( move );

        square.HighlightCapture( piece_to_capture );
    }
}
