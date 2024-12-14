using Godot;
using System;
using System.Threading.Tasks;

public partial class King : Piece
{
    public override string PieceName => "king";

    public static readonly Vector2I[] MoveDirections
                                    = { new Vector2I(  1,  0  ), new Vector2I( -1,  0  ),
                                        new Vector2I(  0,  1  ), new Vector2I(  0, -1  ),
                                        new Vector2I(  1,  1  ), new Vector2I(  1, -1  ),
                                        new Vector2I( -1, -1  ), new Vector2I( -1,  1  ), };

    private bool _hasMoved = false;
    private Vector2I _castleOffset = 2 * Vector2I.Right;

    public override void FillLegalMoveBuffer( Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        int buffer_idx = 0;

        foreach( var dir in MoveDirections )
        {
            var move = BoardPosition + dir;

            if( board_manager.IsPositionOutOfBounds( move ) ) continue;

            var piece_at_move = board_manager.GetPieceAt( move );

                // Can't move into own pieces.
            if( piece_at_move != null && piece_at_move.PieceColor == this.PieceColor ) continue;

                // Check if this move puts the king in check.
            if( WillMovePutKingInCheck( move, board_manager ) ) continue;

                // Add the move
            legal_move_buffer[ buffer_idx++ ] = move;
        }

            // Check for right castle.

            // Cant castle if the king moved or is in check.
        if( _hasMoved || IsKingInCheck( board_manager, PieceColor ) ) return;

        HandleCastleRight( ref buffer_idx, legal_move_buffer, board_manager );
        
        HandleCastleLeft( ref buffer_idx, legal_move_buffer, board_manager );
    }



    public override void FillLegalMoveBufferWithoutCheckChecking( Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        int buffer_idx = 0;

        foreach( var dir in MoveDirections )
        {
            var move = BoardPosition + dir;

            if( board_manager.IsPositionOutOfBounds( move ) ) continue;

            var piece_at_move = board_manager.GetPieceAt( move );

                // Can't move into own pieces.
            if( piece_at_move != null && piece_at_move.PieceColor == this.PieceColor ) continue;

                // Add the move
            legal_move_buffer[ buffer_idx++ ] = move;
        }
    }

    public async override Task OnPerformMove( BoardManager board_manager )
    {
        //////////////////////////
        //  Check for castling  //
        //////////////////////////

        if( _hasMoved ) return;

        _hasMoved = true;

        MoveInfo move_info = board_manager.LastMove;

        var diff = move_info.to - move_info.from;

            // Perform right castle.
        if( diff.X == 2 )
        {
            Piece right_rook = board_manager.GetPieceAt( move_info.to + Vector2I.Right );

            await board_manager.PerformMove( right_rook.BoardPosition, move_info.to + Vector2I.Left, false );
        }
            // Perform left castle.
        if( diff.X == -2 )
        {
            Piece left_rook = board_manager.GetPieceAt( move_info.to + 2 * Vector2I.Left );

            await board_manager.PerformMove( left_rook.BoardPosition, move_info.to + Vector2I.Right, false );
        }

        
    }

    private void HandleCastleRight( ref int buffer_idx, Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        Piece rightmost_piece = board_manager.GetPieceAt( BoardPosition + 3 * Vector2I.Right );

        if( rightmost_piece == null || rightmost_piece is not Rook right_rook ) return;
        if( right_rook.HasMoved || right_rook.PieceColor != PieceColor ) return;

        for( Vector2I move = BoardPosition + Vector2I.Right;
            move.X < BoardPosition.X + 3; move += Vector2I.Right )
        {
            if( board_manager.GetPieceAt( move ) != null ) return;
        }

        for( Vector2I move = BoardPosition + Vector2I.Right;
            move.X < BoardPosition.X + 3; move += Vector2I.Right )
        {
            if( WillMovePutKingInCheck( move, board_manager ) ) return;
        }

        legal_move_buffer[ buffer_idx++ ] = BoardPosition + 2 * Vector2I.Right;
    }

    private void HandleCastleLeft( ref int buffer_idx, Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        Piece leftmost_piece = board_manager.GetPieceAt( BoardPosition + 4 * Vector2I.Left );

        if( leftmost_piece == null || leftmost_piece is not Rook left_rook ) return;
        if( left_rook.HasMoved || left_rook.PieceColor != PieceColor ) return;

        for( Vector2I move = BoardPosition + Vector2I.Left;
            move.X > BoardPosition.X - 4; move += Vector2I.Left )
        {
            if( board_manager.GetPieceAt( move ) != null ) return;
        }

        for( Vector2I move = BoardPosition + Vector2I.Left;
            move.X > BoardPosition.X - 4; move += Vector2I.Left )
        {
            if( WillMovePutKingInCheck( move, board_manager ) ) return;
        }
        
        legal_move_buffer[ buffer_idx++ ] = BoardPosition + 2 * Vector2I.Left;
    }
}
