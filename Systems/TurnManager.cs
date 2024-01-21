using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MyGame;

public enum TurnState
{
    WaitingForPlayer,
    RunningPlayer,
    RunningEnemies,
    EndingEnemies,
    RunningEnvironment
}

class TurnManager
{
    private Gameplay _gameplay;
    private TurnState _state;

    public GameObject Player { get; set; }
    public List<GameObject> Environment { get; set; }
    public List<GameObject> Enemies { get; set; }

    public TurnManager(Gameplay gameplay)
    {
        _gameplay = gameplay;
        _state = TurnState.WaitingForPlayer;
    }

    public void Update()
    {
        switch (_state)
        {
            case TurnState.WaitingForPlayer:
                UpdateWaitingForPlayer();
                break;
            case TurnState.RunningPlayer:
                UpdateRunningPlayer();
                break;
            case TurnState.RunningEnemies:
                UpdateRunningEnemies();
                break;
            case TurnState.EndingEnemies:
                UpdateEndingEnemies();
                break;
            case TurnState.RunningEnvironment:
                UpdateRunningEnvironment();
                break;
        }
    }

    private void UpdateWaitingForPlayer()
    {
        if (_gameplay.CurrentFrame.KeyboardState.IsKeyDown(Keys.Space) && !_gameplay.LastFrame.KeyboardState.IsKeyDown(Keys.Space))
        {
            _gameplay.Player.EndTurn();
            _state = TurnState.RunningPlayer;
        }
    }

    private void UpdateRunningPlayer()
    {
        if (_gameplay.GameObjectsDone())
        {
            _gameplay.Enemies((e) => e.StartTurn());
            _state = TurnState.RunningEnemies;
        }   
    }

    private void UpdateRunningEnemies()
    {
        if (_gameplay.GameObjectsDone())
        {
            _gameplay.Enemies((e) => e.EndTurn());
            _state = TurnState.RunningEnvironment;
        }
    }

    private void UpdateEndingEnemies()
    {
        if (_gameplay.GameObjectsDone())
        {
            _gameplay.Envs((e) => e.StartTurn());
            _state = TurnState.RunningEnvironment;
        }
    }

    private void UpdateRunningEnvironment()
    {
        if (_gameplay.GameObjectsDone())
        {
            _gameplay.Player.StartTurn();
            _state = TurnState.WaitingForPlayer;
        }
    }
}