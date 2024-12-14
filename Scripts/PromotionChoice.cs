using Godot;
using System;

public partial class PromotionChoice : Control
{
    [Signal]
    public delegate void FinishedChosingEventHandler();

    public Piece ChosenPromotionPiece { get; set;}

    private HBoxContainer _hBoxContainer = null;

    private ePieceColor _playerColor = ePieceColor.WHITE;

    public override void _Ready()
    {
        _hBoxContainer = GetNode<HBoxContainer>( "PanelContainer/HBoxContainer" );
        this.Hide();

        InitializeChoiceButtons();
    }

    public void ChoiceBegin( Vector2 world_pos, ePieceColor player_color )
    {
        _playerColor = player_color;
        
        var color = player_color == ePieceColor.WHITE
            ? Piece.WHITE_PIECE_COLOR : Piece.BLACK_PIECE_COLOR;

        _hBoxContainer.Modulate = color;

        this.Show();
        GlobalPosition = world_pos;

        if( player_color == ePieceColor.BLACK ) GlobalPosition -= new Vector2( 256, 64 );
    }

    private void InitializeChoiceButtons()
    {
        var queen_button = _hBoxContainer.GetNode<TextureButton>( "Queen" );
        var rook_button = _hBoxContainer.GetNode<TextureButton>( "Rook" );
        var bishop_button = _hBoxContainer.GetNode<TextureButton>( "Bishop" );
        var knight_button = _hBoxContainer.GetNode<TextureButton>( "Knight" );

        queen_button.Pressed += () => 
        {
            ChosenPromotionPiece = Piece.Create<Queen>( _playerColor, 0, 
                0, BoardManager.SQUARE_SIZE );
            this.Hide();
            EmitSignal( SignalName.FinishedChosing );
        };

        rook_button.Pressed += () => 
        {
            ChosenPromotionPiece = Piece.Create<Rook>( _playerColor, 0, 
                0, BoardManager.SQUARE_SIZE );
            this.Hide();
            EmitSignal( SignalName.FinishedChosing );
        };

        bishop_button.Pressed += () => 
        {
            ChosenPromotionPiece = Piece.Create<Bishop>( _playerColor, 0, 
                0, BoardManager.SQUARE_SIZE );
            this.Hide();
            EmitSignal( SignalName.FinishedChosing );
        };

        knight_button.Pressed += () => 
        {
            ChosenPromotionPiece = Piece.Create<Knight>( _playerColor, 0, 
                0, BoardManager.SQUARE_SIZE );
            this.Hide();
            EmitSignal( SignalName.FinishedChosing );
        };
    }
}
