using Godot;
using System;

public partial class Knight : Piece
{
    public override string PieceName => "knight";

    public static readonly Vector2I[] MoveDirections
                                    = { new Vector2I(  1,  2  ),
                                        new Vector2I( -1,  2  ),
                                        new Vector2I(  1, -2  ),
                                        new Vector2I( -1, -2  ),
                                        new Vector2I(  2,  1  ),
                                        new Vector2I(  2, -1  ),
                                        new Vector2I( -2,  1  ),
                                        new Vector2I( -2, -1  ), };

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

                // Check if this move puts our king in check.
            if( WillMovePutKingInCheck( move, board_manager ) ) continue;

                // Add the move
            legal_move_buffer[ buffer_idx++ ] = move;
        }
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
}
