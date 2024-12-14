using Godot;
using System;
using System.Threading.Tasks;

public partial class Pawn : Piece
{
    public override string PieceName => "pawn";

    private Vector2I _enPassantMove = GameHandler.NOT_A_MOVE;
    private Vector2I _enPassantPieceToCapturePosition = GameHandler.NOT_A_MOVE;

    public override void FillLegalMoveBuffer( Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        int buffer_idx = 0;

        if( PieceColor == ePieceColor.WHITE )
        {
            HandleWhitePawn( true, ref buffer_idx, legal_move_buffer, board_manager );            
        }
        else
        {
            HandleBlackPawn( true, ref buffer_idx, legal_move_buffer, board_manager );   
        }
    }

    public override void FillLegalMoveBufferWithoutCheckChecking( Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        int buffer_idx = 0;

        if( PieceColor == ePieceColor.WHITE )
        {
            HandleWhitePawn( false, ref buffer_idx, legal_move_buffer, board_manager );            
        }
        else
        {
            HandleBlackPawn( false, ref buffer_idx, legal_move_buffer, board_manager );   
        }
    }

    public async override Task OnPerformMove( BoardManager board_manager )
    {
        MoveInfo move_info = board_manager.LastMove;
        if( move_info.to == _enPassantMove )
        {
            board_manager.CapturePieceAt( _enPassantPieceToCapturePosition );
        }

            // Check for promotion.
        int last_Y = PieceColor == ePieceColor.WHITE ? 0 : BoardManager.BOARD_SIDE_SIZE - 1;

        if( move_info.to.Y != last_Y ) return;

        board_manager.PromotionChoice.ChoiceBegin( GlobalPosition, PieceColor );
        await ToSignal( board_manager.PromotionChoice, PromotionChoice.SignalName.FinishedChosing );

        Piece new_piece = board_manager.PromotionChoice.ChosenPromotionPiece;

        if( new_piece == null ) return;

        board_manager.AddChild( new_piece );

            // Replace this pawn with the new promoted piece.
        board_manager.SetPieceAt( move_info.to, new_piece );
        new_piece.BoardPosition = this.BoardPosition;
        this.QueueFree();
    }

    private void HandleWhitePawn( bool check_checking, ref int buffer_idx, Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        Vector2I move = BoardPosition + Vector2I.Up;

        if( IsMoveValid( check_checking, move, board_manager ) )
        {
            legal_move_buffer[ buffer_idx++ ] = move;

            move += Vector2I.Up;

                // If the pawn is still in the beginning position.
            if( BoardPosition.Y == BoardManager.BOARD_SIDE_SIZE - 2 && IsMoveValid( check_checking, move, board_manager ) )
            {
                legal_move_buffer[ buffer_idx++ ] = move;
            }
        }

        move = BoardPosition - new Vector2I( 1, 1 );

        if( IsCaptureValid( check_checking, move, board_manager ) )
        {
            legal_move_buffer[ buffer_idx++ ] = move;
        }

        move = BoardPosition - new Vector2I( -1, 1 );

        if( IsCaptureValid( check_checking, move, board_manager ) )
        {
            legal_move_buffer[ buffer_idx++ ] = move;
        }

        HandleEnPassant( check_checking, Vector2I.Up,
            ref buffer_idx, legal_move_buffer, board_manager );
    }

    private void HandleBlackPawn( bool check_checking, ref int buffer_idx, Vector2I[] legal_move_buffer, BoardManager board_manager )
    {
        Vector2I move = BoardPosition + Vector2I.Down;

        if( IsMoveValid( check_checking, move, board_manager ) )
        {
            legal_move_buffer[ buffer_idx++ ] = move;

            move += Vector2I.Down;

                // If the pawn is still in the beginning position.
            if( BoardPosition.Y == 1 && IsMoveValid( check_checking, move, board_manager ) )
            {
                legal_move_buffer[ buffer_idx++ ] = move;
            }
        }

        move = BoardPosition + new Vector2I( 1, 1 );

        if( IsCaptureValid( check_checking, move, board_manager ) )
        {
            legal_move_buffer[ buffer_idx++ ] = move;
        }

        move = BoardPosition + new Vector2I( -1, 1 );

        if( IsCaptureValid( check_checking, move, board_manager ) )
        {
            legal_move_buffer[ buffer_idx++ ] = move;
        }

        HandleEnPassant( check_checking, Vector2I.Down,
            ref buffer_idx, legal_move_buffer, board_manager );
    }

    private void HandleEnPassant( bool check_checking, Vector2I pawn_move_direction,
        ref int buffer_idx, Vector2I[] legal_move_buffer, BoardManager board_manager )
    {      
        _enPassantMove = GameHandler.NOT_A_MOVE;
        _enPassantPieceToCapturePosition = GameHandler.NOT_A_MOVE;

        MoveInfo last_move = board_manager.LastMove;
        
            // If the last opponent's move was a pawn moving two squares ahead. 
        if( last_move.pieceMoved is Pawn
            && Mathf.Abs( last_move.from.Y - last_move.to.Y ) == 2 )
        {
                // Direction from our pawn to the opponent's pawn.
            var dir = last_move.to - BoardPosition;

                // If the pieces are next to each other
            if( dir == Vector2I.Right || dir == Vector2I.Left )
            {
                var move = BoardPosition + pawn_move_direction + dir;

                if( IsMoveValid( check_checking, move, board_manager ) )
                {
                    legal_move_buffer[ buffer_idx++ ] = move;
                    _enPassantMove = move;
                    _enPassantPieceToCapturePosition = last_move.to;
                }
            }
        }
    }


    private bool IsMoveValid( bool check_checking, Vector2I move, BoardManager board_manager )
    {
        if( board_manager.IsPositionOutOfBounds( move ) ) return false;

        Piece piece_at_move = board_manager.GetPieceAt( move );
        if( piece_at_move != null ) return false;

            // Check if this move puts the king in check.
        if( check_checking && WillMovePutKingInCheck( move, board_manager ) ) return false;

        return true;
    }

    private bool IsCaptureValid( bool check_checking, Vector2I move, BoardManager board_manager )
    {
        if( board_manager.IsPositionOutOfBounds( move ) ) return false;

        Piece piece_at_move = board_manager.GetPieceAt( move );
        
            // If there is no piece at move, or it is our own piece - we cant capture it.
        if( piece_at_move == null || piece_at_move.PieceColor == this.PieceColor ) return false;

            // Check if this move puts the king in check.
        if( check_checking && WillMovePutKingInCheck( move, board_manager ) ) return false;

        return true;
    }
}
