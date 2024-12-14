using Godot;
using System;

public partial class GameOverScreen : Control
{
    [Export]
    private GameHandler _GameHandler { get; set; }

    [Export]
    private string _MainGameScenePath { get; set; } = @"res://Scenes/game.tscn";

    public override void _Ready()
    {
        _GameHandler ??= GetNode<GameHandler>( "../../GameHandler" );

        _GameHandler.OnGameOver += GameLost;
        _GameHandler.OnStalemate += Stalemate;
    }

    private void GameLost()
    {
        if( _GameHandler.PlayerToPlay == ePieceColor.BLACK )
        {
            GetNode<TextureRect>( "VBoxContainer/CenterContainer/WhiteWins" ).Show();
        }
        else
        {
            GetNode<TextureRect>( "VBoxContainer/CenterContainer/BlackWins" ).Show();
        }

        GetNode<TextureButton>( "VBoxContainer/TextureButton" ).Show();
    }

    private void Stalemate()
    {
        GetNode<TextureRect>( "VBoxContainer/CenterContainer/Stalemate" ).Show();

        GetNode<TextureButton>( "VBoxContainer/TextureButton" ).Show();
    }

    private void PlayAgain()
    {
        GetTree().ChangeSceneToFile( _MainGameScenePath );
    }
}
