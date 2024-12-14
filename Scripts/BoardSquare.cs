using Godot;
using System;

public partial class BoardSquare : Sprite2D
{
    private Sprite2D _pieceMoveOutline = null;
    private Sprite2D _pieceCaptureOutline = null;
    private Sprite2D _moveHighlight = null;

    public override void _Ready()
    {
        _pieceMoveOutline = GetNode<Sprite2D>( "PieceMoveOutline" );
        _pieceCaptureOutline = GetNode<Sprite2D>( "PieceCaptureOutline" );
        _moveHighlight = GetNode<Sprite2D>( "MoveHighlight" );
    }

    public void HighlightMove() => _moveHighlight.Show();

    public void HighlightCapture( Piece piece_to_capture )
    {
        _pieceCaptureOutline.Texture = piece_to_capture.Sprite.Texture;
        _pieceCaptureOutline.Show();
    }

    public void HighlightSelect( Piece piece_to_select )
    {
        _pieceMoveOutline.Texture = piece_to_select.Sprite.Texture;
        _pieceMoveOutline.Show();
    }

    public void UnhighlightAll()
    {
        _pieceMoveOutline.Hide();
        _pieceCaptureOutline.Hide();
        _moveHighlight.Hide();
    }
}
