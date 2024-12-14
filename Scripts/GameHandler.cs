using Godot;
using System;

public partial class GameHandler : Node2D
{

    public static readonly Vector2I NOT_A_MOVE = Vector2I.MinValue;

    public event Action OnPiecePickup;

    public event Action OnPieceDrop;

    public event Action OnGameOver;

    public event Action OnStalemate;

    [Export]
    public BoardManager BoardManager { get; set; }

    [Export]
    private float _DragSpeed = 9.0f;

    public Piece PickedUpPiece { get; set; } = null;

    public ePieceColor PlayerToPlay = ePieceColor.WHITE;

    private Vector2 _piecePositionBeforePickup = Vector2.Zero;
    private Vector2I _pieceBoardPositionBeforePickup = Vector2I.Zero;
    private Vector2 _mouseGlobalPosition = Vector2.Zero;

        // For each square on the board. Most likely not gonna happen,
        // but what if you want to make a new cool piece with a super power?
    public Vector2I[] LegalMoveBuffer { get; private set; }
        = new Vector2I[ BoardManager.BOARD_SIDE_SIZE * BoardManager.BOARD_SIDE_SIZE ];

    public override void _UnhandledInput( InputEvent @event )
    {
        if( @event is not InputEventMouseButton mouseButtonEvent ) return;
        

        if( mouseButtonEvent.Pressed )
        {
            PickupPiece();
        }
        else    // Is released
        {
            ReleasePiece();
        }
    }

    public override void _Ready()
    {
        BoardManager ??= GetNode<BoardManager>( "../BoardManager" );
    }

    public override void _Process( double delta )
    {
        if( PickedUpPiece == null ) return;

            // Move picked up piece.
        PickedUpPiece.GlobalPosition = PickedUpPiece
            .GlobalPosition
            .Lerp( GetMouseWorldPostion(), _DragSpeed * ( float ) delta );
    }

    private void PickupPiece()
    {
        Piece piece = BoardManager.GetPieceAt( GetMousePositionOnBoard() );

        if( piece == null || piece.PieceColor != PlayerToPlay ) return;

        PickedUpPiece = piece;
        _piecePositionBeforePickup = PickedUpPiece.GlobalPosition;
        _pieceBoardPositionBeforePickup = PickedUpPiece.BoardPosition;

            // Check for legal moves.
        EmptyLegalMoveBuffer();
        piece.FillLegalMoveBuffer( LegalMoveBuffer, BoardManager );
        
        OnPiecePickup?.Invoke();
    }

    private async void ReleasePiece()
    {
        OnPieceDrop?.Invoke();

        if( PickedUpPiece == null ) return;

        Vector2I move = GetMousePositionOnBoard();

            // PLACE PIECE HERE, TURN THIS FUNCTION INTO SOMETHING ELSE
        if( IsMoveLegal( move ) )
        {
            PickedUpPiece = null;
            await BoardManager.PerformMove( _pieceBoardPositionBeforePickup, move );
            
            PlayerToPlay = ( ePieceColor )( ( ( ( int ) PlayerToPlay ) + 1 ) % 2 );

            var game_over_state = CheckGameOver();

            switch( game_over_state )
            {
                case eGameOverState.GAME_OVER:
                    OnGameOver?.Invoke();
                    break;
                case eGameOverState.STALEMATE:
                    OnStalemate?.Invoke();
                break;
            }
        }
        else
        {
            PickedUpPiece.GlobalPosition = _piecePositionBeforePickup;
        }
        
        PickedUpPiece = null;
    }

    public Vector2 GetMouseWorldPostion()
    {
            // If it comes to multiplayer then we will send this information.
        return GetGlobalMousePosition();
    }

    public Vector2I GetMousePositionOnBoard()
    {
        var mouse_pos = GetGlobalMousePosition();
        
        var relative_mouse_pos = mouse_pos - BoardManager.GlobalPosition;
        return BoardManager.WorldToBoardPosition( relative_mouse_pos );
    }

    private eGameOverState CheckGameOver()
    {
        Piece king = Piece.FindKing( BoardManager, PlayerToPlay );

        for( int i = 0; i < BoardManager.BOARD_SIDE_SIZE; ++i )
        {
            for( int j = 0; j < BoardManager.BOARD_SIDE_SIZE; ++j )
            {
                Piece piece_at = BoardManager.GetPieceAt( i, j );

                if( piece_at == null || piece_at.PieceColor != PlayerToPlay ) continue;

                EmptyLegalMoveBuffer();

                piece_at.FillLegalMoveBuffer( LegalMoveBuffer, BoardManager );

                foreach( var move in LegalMoveBuffer )
                {
                    if( move == NOT_A_MOVE ) break;

                        // If any piece can move, then it's not game over yet.
                    return eGameOverState.NOT_OVER;
                }
            }
        }

        if( king.IsKingInCheck( BoardManager, PlayerToPlay ) ) return eGameOverState.GAME_OVER;
        return eGameOverState.STALEMATE;
    }

    private void EmptyLegalMoveBuffer()
    {
        for( int i = 0; i < LegalMoveBuffer.Length; ++i ) LegalMoveBuffer[ i ] = NOT_A_MOVE;
    }

    private bool IsMoveLegal( Vector2I move )
    {
        for( int i = 0; i < LegalMoveBuffer.Length; ++i )
        {
            var legal_move = LegalMoveBuffer[ i ];

            if( legal_move == NOT_A_MOVE ) return false;
        
            if( move != legal_move ) continue;
            return true;
        }
        return false;
    }
}

public enum eGameOverState
{
    NOT_OVER,
    STALEMATE,
    GAME_OVER
}