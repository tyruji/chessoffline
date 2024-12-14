using Godot;
using System;

public partial class Bishop : Piece
{
    public override string PieceName => "bishop";

    public override void FillLegalMoveBuffer( Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        int buffer_idx = 0;
        
        FillForMovesInDirection( true, ref buffer_idx, new Vector2I( 1, 1 ), legal_move_buffer, board_manager );

        FillForMovesInDirection( true, ref buffer_idx, new Vector2I( -1, 1 ), legal_move_buffer, board_manager );

        FillForMovesInDirection( true, ref buffer_idx, new Vector2I( -1, -1 ), legal_move_buffer, board_manager );

        FillForMovesInDirection( true, ref buffer_idx, new Vector2I( 1, -1 ), legal_move_buffer, board_manager );
    }

    public override void FillLegalMoveBufferWithoutCheckChecking( Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        int buffer_idx = 0;
        
        FillForMovesInDirection( false, ref buffer_idx, new Vector2I( 1, 1 ), legal_move_buffer, board_manager );

        FillForMovesInDirection( false, ref buffer_idx, new Vector2I( -1, 1 ), legal_move_buffer, board_manager );

        FillForMovesInDirection( false, ref buffer_idx, new Vector2I( -1, -1 ), legal_move_buffer, board_manager );

        FillForMovesInDirection( false, ref buffer_idx, new Vector2I( 1, -1 ), legal_move_buffer, board_manager );
    }

    private void FillForMovesInDirection( bool check_checking, ref int buffer_idx, Vector2I dir, Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        for( Vector2I offset = dir; !board_manager.IsPositionOutOfBounds( BoardPosition + offset ); 
            offset += dir )
        {
            Vector2I move = BoardPosition + offset;
            var piece_at_move = board_manager.GetPieceAt( move );

                // Can't move into own pieces, and if one is
                // in the way, then we give up on the rest of the path.
            if( piece_at_move != null && piece_at_move.PieceColor == this.PieceColor ) break;

                // Check if this move puts the king in check.
            if( check_checking && WillMovePutKingInCheck( move, board_manager ) ) continue;

                // If we stumble upon an opponent's piece, we can capture it
                // so add this move to the legal move buffer.
            if( piece_at_move != null )
            {
                legal_move_buffer[ buffer_idx++ ] = move;

                break;
            }

            legal_move_buffer[ buffer_idx++ ] = move;
        }
    }
}
